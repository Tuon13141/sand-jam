using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.IO.Compression;
using Tabtale.TTPlugins.UnityIAPWrapper;
using Tabtale.TTPlugins;

#if UIAP_INSTALLED && TTP_BILLING
using TTPIUnityPurchasing = UnityEngine.Purchasing.UnityPurchasing;
using TTPSubscriptionInfo = UnityEngine.Purchasing.SubscriptionInfo;
using TTPSubscriptionManager = UnityEngine.Purchasing.SubscriptionManager;
using TTPResult = UnityEngine.Purchasing.Result;
using TTPIProduct = UnityEngine.Purchasing.Product;
using TTPIStoreListener = UnityEngine.Purchasing.IStoreListener;
using TTPIProductType = UnityEngine.Purchasing.ProductType;
using TTPIPurchaseEventArgs = UnityEngine.Purchasing.PurchaseEventArgs;
using TTPIPurchaseProcessingResult = UnityEngine.Purchasing.PurchaseProcessingResult;
using TTPConfigurationBuilder = UnityEngine.Purchasing.ConfigurationBuilder;
using TTPIProductCollection = UnityEngine.Purchasing.ProductCollection;
using TTPIGooglePlayConfiguration = UnityEngine.Purchasing.IGooglePlayConfiguration;
using TTPStandardPurchasingModule = UnityEngine.Purchasing.StandardPurchasingModule;
using TTPFakeStoreUIMode = UnityEngine.Purchasing.FakeStoreUIMode;
using TTPIAppleExtensions = UnityEngine.Purchasing.IAppleExtensions;
using TTPIExtensionProvider = UnityEngine.Purchasing.IExtensionProvider;
using TTPIAppleConfiguration = UnityEngine.Purchasing.IAppleConfiguration;
using TTPIDs = UnityEngine.Purchasing.IDs;
using TTPIStoreController = UnityEngine.Purchasing.IStoreController;
using TTPIInitializationFailureReason = UnityEngine.Purchasing.InitializationFailureReason;
using TTPIPurchaseFailureReason = UnityEngine.Purchasing.PurchaseFailureReason;
using TTPAbstractPurchasingModule = UnityEngine.Purchasing.Extension.AbstractPurchasingModule;
using TTPIGooglePlayStoreExtensions = UnityEngine.Purchasing.IGooglePlayStoreExtensions;
using SubscriptionInfo = UnityEngine.Purchasing.SubscriptionInfo;
using Result = UnityEngine.Purchasing.Result;
using TTPAppleValidator = UnityEngine.Purchasing.Security.AppleValidator;
using TTPAppleReceipt = UnityEngine.Purchasing.Security.AppleReceipt;
using TTPAppleInAppPurchaseReceipt = UnityEngine.Purchasing.Security.AppleInAppPurchaseReceipt;
using TTPUnityServicesCore = Unity.Services.Core;


#if TTP_BILLING && (UNITY_ANDROID || UNITY_IPHONE)
using TTPAppleTangle = UnityEngine.Purchasing.Security.TTPRealAppleTangle;
#endif

#endif

namespace Tabtale.TTPlugins
{
    public class PurchaseIAPResult
    {
        public PurchaseIAPResult(InAppPurchasableItem purchasedItem,
            PurchaseIAPResultCode result = PurchaseIAPResultCode.Success, BillerErrors error = BillerErrors.NO_ERROR)
        {
            this.purchasedItem = purchasedItem;
            this.result = result;
            this.error = error;
        }

        public InAppPurchasableItem purchasedItem;
        public PurchaseIAPResultCode result;
        public BillerErrors error;
    }

    public enum PurchaseIAPResultCode
    {
        Success,
        Cancelled,
        Failed
    }

    public enum BillerErrors
    {
        NO_ERROR,
        PURCHASING_UNAVAILABLE,
        ATTEMPTING_TO_PURCHASE_PRODUCT_WITH_SAME_RECEIPT,
        PAYMENT_DECLINED,
        PRODUCT_UNAVAILABLE,
        REMOTE_VALIDATION_FAILED,
        APP_NOT_KNOWN,
        NO_PRODUCTS_AVAILABLE,
        UNKNOWN
    }

    public struct InAppPurchasableItem
    {
        public string mainId;
        public string id;
        public string name;
        public string description;
        public float localizedPrice;
        public string receipt;
        public string transactionId;
        public string currency;
        public bool isSubscription;
    }

    public class TTPUnityBillingAgent : TTPIStoreListener
    {
        private const string IAPS_PERSISTENCY_KEY = "PSDKBillingIAPs";
        private const int IAPS_ANDROID_TOKEN_FRAGMENT_LEN = 50;
        
        private TTPIStoreController _storeController;

        //private UnityEngine.Purchasing.IStoreController _storeController;
        private TTPIAppleExtensions _appleExtensions;
        private bool _isInitialised = false;
        private bool _isPurchaseInProgress = false;
        private bool _isRestoreInProgress = false;
        private InAppPurchasableItem _iapInValidation;
        private TTPIPurchaseEventArgs _iapInValidationArgs;
        private string _noAdsIapIds = "";
        private Dictionary<string, string> _iapMap;
        private Dictionary<string, object> _configuration;

        private Action<BillerErrors> _onBillingInitAction;
        private Action<PurchaseIAPResult> _onPurchasedAction;
        private Action<string, float, string, string, string, string, bool?> _validatePurchaseAction;
        private Action<bool> _reportNoAdsPurchasedAction;
        private Action<bool> _transactionsRestoredAction;
        private const long ANALYTICS_TARGET_DELTA_DNA = 1 << 1;
        private const long ANALYTICS_TARGET_FIREBASE = 1 << 2;

        public TTPUnityBillingAgent(Action<PurchaseIAPResult> onPurchasedAction,
            Action<string, float, string, string, string, string, bool?> validatePurchaseAction, Action<bool> reportNoAdsPurchasedAction,
            Action<bool> transactionsRestoredAction, Dictionary<string, object> configuration)
        {
            _validatePurchaseAction = validatePurchaseAction;
            _onPurchasedAction = onPurchasedAction;
            _reportNoAdsPurchasedAction = reportNoAdsPurchasedAction;
            _transactionsRestoredAction = transactionsRestoredAction;
            _configuration = configuration;

            RegisterAppLifeCycle(true);
        }

        ~TTPUnityBillingAgent()
        {
            RegisterAppLifeCycle(false);
        }

        public void PurchaseItem(string itemId, Action purchaseHasStarted)
        {
            Debug.Log("Billing::PurchaseItem:" + itemId);

            if (_isPurchaseInProgress)
            {
                Debug.Log("Billing::PurchaseItem:attempted to buy iap while another purchase is in progress. id: " +
                          itemId);
                return;
            }

            _isPurchaseInProgress = true;

            // An actual attempt to purchase was made, this means a restore is not in progress
            _isRestoreInProgress = false;

            string finalId = GetFinalId(itemId);
            if (_storeController == null)
            {
                _isPurchaseInProgress = false;
                return;
            }

            if (finalId != null)
            {
                purchaseHasStarted.Invoke();
                _storeController.InitiatePurchase(_storeController.products.WithStoreSpecificID(finalId));
            }
            else
            {
                Debug.LogError("Billing::PurchaseItem:" + itemId + " does not exist as iapId or id");
                _isPurchaseInProgress = false;
            }
        }

        public bool IsConsumable(string itemId)
        {
            if (!IsInitialisedWithLogging())
                return false;
            string finalId = GetFinalId(itemId);
            if (finalId != null)
                return _storeController.products.WithStoreSpecificID(finalId).definition.type ==
                       TTPIProductType.Consumable;
            else
                return false;
        }

        public string GetIAPType(string itemId)
        {
            if (!IsInitialisedWithLogging())
                return null;
            
            string finalId = GetFinalId(itemId);
            if (finalId != null)
            {
                var productType = _storeController.products.WithStoreSpecificID(finalId).definition.type;
                if (productType == TTPIProductType.Consumable)
                    return "consumable";
                else if (productType == TTPIProductType.NonConsumable)
                    return "non-consumable";
                else if (productType == TTPIProductType.Subscription)
                    return "subscription";
            }   
            return null;
        }

        public bool IsPurchased(string itemId)
        {
            string finalId = GetFinalId(itemId);
            if (!IsInitialisedWithLogging())
                return false;

            TTPIProduct item = _storeController.products.WithStoreSpecificID(finalId != null ? finalId : "");

            if (item == null)
            {
                Debug.Log("Billing::IsPurchased:Error, Cannot find item - " + finalId);
                return false;
            }

            Debug.Log("Billing::IsPurchased: " + item.receipt ?? "null");

            if (Application.platform == RuntimePlatform.Android &&
                !CheckProductPurchaseStateInPayloadForGooglePlayStore(item.receipt))
            {
                return false;
            }

            bool isSubscription = item.definition.type == TTPIProductType.Subscription;

            // Unity do not persist the receipt between app restarts, so we save the purchases locally
            if (!isSubscription)
            {
                if (item.hasReceipt) return true;
                return IsSavedPurchased(finalId);
            }
            else
            {
                return item.hasReceipt;
            }
        }

        private bool CheckProductPurchaseStateInPayloadForGooglePlayStore(string reciept)
        {
            Dictionary<string, object> recieptDic = TTPJson.Deserialize(reciept) as Dictionary<string, object>;
            string payload = null;
            if (recieptDic != null && recieptDic.ContainsKey("Payload"))
            {
                payload = recieptDic["Payload"] as string;
            }

            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            var payloadWrapper = TTPJson.Deserialize(payload) as Dictionary<string, object>;
            if (!payloadWrapper.ContainsKey("json"))
            {
                Debug.LogError(
                    "Billing:: The product receipt does not contain enough information, the 'json' field is missing");
                return false;
            }

            var originalJsonPayloadWrapper =
                TTPJson.Deserialize((string)payloadWrapper["json"]) as Dictionary<string, object>;
            if (originalJsonPayloadWrapper == null || !originalJsonPayloadWrapper.ContainsKey("purchaseState"))
            {
                Debug.LogError(
                    "Billing:: The product receipt does not contain enough information, the 'purchaseState' field is missing");
                return false;
            }

            var purchaseState = originalJsonPayloadWrapper["purchaseState"];
            return (long)purchaseState == 0;
        }

        public void RestorePurchases(Action<bool> onTransactionsRestoredAction)
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialisedWithLogging())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                Debug.Log("Billing::RestorePurchases: FAIL. Not initialized.");
                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                Debug.Log("Billing::RestorePurchases started ...");

                _isRestoreInProgress = true;


                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                _appleExtensions.RestoreTransactions((bool result) =>
                {
                    _isRestoreInProgress = false;
                    onTransactionsRestoredAction.Invoke(result);
                    _transactionsRestoredAction.Invoke(result);
                });
            }
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                Debug.Log("Billing::RestorePurchases: FAIL. Not supported on this platform. Current = " +
                          Application.platform);
            }
        }

        public void InitBilling(Action<BillerErrors> onBillingInitAction)
        {
            _onBillingInitAction = onBillingInitAction;
            if (_isInitialised)
            {
                Debug.Log("Billing::InitBilling already init");
                return;
            }

            Debug.Log("Billing::InitBilling");

            // On Android restore is called automatically on app start, we want to be aware of it and not report to analytics as revenue
            if (Application.platform == RuntimePlatform.Android)
            {
                _isRestoreInProgress = true;
            }

            TTPAbstractPurchasingModule module;
            if (Application.platform == RuntimePlatform.Android)
            {
#if TTP_GOOGLE_PLAY_PLUGINS && UIAP_INSTALLED
                module = Google.Play.Billing.GooglePlayStoreModule.Instance();
#else
                module = TTPStandardPurchasingModule.Instance();
#endif
            }
            else
            {
                module = TTPStandardPurchasingModule.Instance();
                // The FakeStore supports: no-ui (always succeeding), basic ui (purchase pass/fail), and 
                // developer ui (initialization, purchase, failure code setting). These correspond to 
                // the FakeStoreUIMode Enum values passed into StandardPurchasingModule.useFakeStoreUIMode.
                ((TTPStandardPurchasingModule)module).useFakeStoreUIMode = TTPFakeStoreUIMode.StandardUser;
            }

            var builder = TTPConfigurationBuilder.Instance(module);

            ReadProductInfo(builder);

            InitUnityServicesAndInitIAP(builder);
        }

        private async void InitUnityServicesAndInitIAP(TTPConfigurationBuilder configurationBuilder)
        {
            Debug.Log("TTPUnityBillingAgent:: start");
            try
            {
                var options = new InitializationOptions().SetEnvironmentName("production");
                await UnityServices.InitializeAsync(options);
                Debug.Log("TTPUnityBillingAgent:: unity services initiated");
                TTPIUnityPurchasing.Initialize(this, configurationBuilder);
            }
            catch (Exception exception)
            {
                Debug.Log("TTPUnityBillingAgent::InitUnityServicesAndInitIAP: exception - " + exception.Message);
                var parameters = new Dictionary<string, object>
                {
                    { "message", exception.Message },
                    { "stacktrace", exception.StackTrace }
                };
                LogEventByReflection(ANALYTICS_TARGET_FIREBASE, "ttpDebugUnityServicesFailed", parameters);
            }
        }

        public bool IsNoAdsItem(string itemId)
        {
            string finalId = GetFinalId(itemId);
            if (finalId != null)
                return _noAdsIapIds.Contains(finalId);
            else
                return false;
        }

        public string GetLocalizedPriceString(string itemId)
        {
            Debug.Log("Billing::GetLocalizedPriceString:" + itemId);

            if (!IsInitialisedWithLogging())
                return "";

            string finalId = GetFinalId(itemId);
            TTPIProduct item = null;
            if (finalId != null)
                item = _storeController.products.WithStoreSpecificID(finalId);

            if (item == null)
            {
                Debug.Log("Billing::GetLocalizedPriceString:Error, cannot find item by id: " + itemId);
                return "";
            }

            return item.metadata.localizedPriceString;
        }

        public string GetISOCurrencySymbol(string itemId)
        {
            if (!IsInitialisedWithLogging())
                return "";
            string finalId = GetFinalId(itemId);
            TTPIProduct item = null;
            if (finalId != null)
                item = _storeController.products.WithStoreSpecificID(finalId);
            if (item == null)
            {
                Debug.Log("Billing::GetISOCurrencySymbol:Error, cannot find item by id: " + itemId);
                return "";
            }

            return item.metadata.isoCurrencyCode;
        }

        public decimal GetPriceInLocalCurrency(string itemId)
        {
            if (!IsInitialisedWithLogging())
                return 0;
            string finalId = GetFinalId(itemId);
            TTPIProduct item = null;
            if (finalId != null)
                item = _storeController.products.WithStoreSpecificID(finalId);
            if (item == null)
            {
                Debug.Log("Billing::GetPriceInLocalCurrency:Error, cannot find item by id: " + itemId);
                return 0;
            }

            return item.metadata.localizedPrice;
        }

        public void ClearTransactions()
        {
            PlayerPrefs.DeleteKey(IAPS_PERSISTENCY_KEY);
        }

        private bool IsInitialisedWithLogging()
        {
            if (!_isInitialised)
                Debug.Log(
                    "Attempted to use purchasing api when unity iap is not initialized. check Failed to initialize in the log for more details");
            return _isInitialised;
        }

        private bool IsSavedPurchased(string iapId)
        {
            string saveData = PlayerPrefs.GetString(IAPS_PERSISTENCY_KEY, "");
            string[] splitData = saveData.Split(new char[] { ';' });
            bool isPurchased = false;
            foreach (string item in splitData)
            {
                if (item.Equals(iapId))
                {
                    isPurchased = true;
                    break;
                }
            }

            return isPurchased;
        }

        public string GetFinalId(string itemId)
        {
            string finalId = null;

            if (_iapMap.Values.Contains(itemId))
            {
                finalId = itemId;
            }
            else
            {
                if (_iapMap.Keys.Contains(itemId))
                {
                    _iapMap.TryGetValue(itemId, out finalId);
                }
            }

            return finalId;
        }

        #region IStoreListener implementation

        public void OnInitialized(TTPIStoreController controller, TTPIExtensionProvider extensions)
        {
            Debug.Log("Billing::OnInitialized");

            _isInitialised = true;
            _storeController = controller;


            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                _appleExtensions = extensions.GetExtension<TTPIAppleExtensions>();
                // On Apple platforms we need to handle deferred purchases caused by Apple's Ask to Buy feature.
                // On non-Apple platforms this will have no effect; OnDeferred will never be called.
                _appleExtensions.RegisterPurchaseDeferredListener(OnDeferred);
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                Debug.Log("Billing::Application.platform is android");
#if TTP_GOOGLE_PLAY_PLUGINS && UIAP_INSTALLED
                var gpExtenstion = extensions.GetExtension<Google.Play.Billing.IGooglePlayStoreExtensions>();
#else
                var gpExtenstion = extensions.GetExtension<TTPIGooglePlayStoreExtensions>();
#endif

                if (gpExtenstion != null)
                {
                    Debug.Log("Billing::Calling gpExtenstion.RestoreTransactions");
                    gpExtenstion.RestoreTransactions((success) =>
                    {
                        Debug.Log("Billing:: gpExtenstion.RestoreTransactions. success =  " + success);
                        _isRestoreInProgress = false;
                        DeterminePurchaseAd(_storeController.products.all);
                    });
                }
            }

            DeterminePurchaseAd(_storeController.products.all);

            _onBillingInitAction.Invoke(BillerErrors.NO_ERROR);
        }

        private void DeterminePurchaseAd(TTPIProduct[] allProducts)
        {
            bool purchaseAd = false;
            foreach (TTPIProduct product in _storeController.products.all)
            {
                Debug.Log("Billing::OnInitialized: id = " + product.definition.id + "reciept = " + product.receipt ??
                          "null");
                if (IsNoAdsItem(product.definition.id))
                {
                    if (product.hasReceipt)
                    {
                        if (product.definition.type == TTPIProductType.Subscription)
                        {
                            SubscriptionInfo itemInfo = GetSubscriptionInfo(product.definition.id);
                            if (itemInfo != null)
                            {
                                if (itemInfo.isSubscribed() == Result.True)
                                {
                                    Debug.Log("Billing::OnInitialized: subscribed to item " + product.definition.id +
                                              " which is defined as noAds. notifying TT Plugins.");
                                    purchaseAd = true;
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("Billing::OnInitialized: purchased item " + product.definition.id +
                                      " which is defined as noAds. notifying TT Plugins.");
                            purchaseAd = true;
                        }
                    }
                    else
                    {
                        if (product.definition.type != TTPIProductType.Subscription &&
                            IsSavedPurchased(product.definition.id))
                        {
                            Debug.Log("Billing::OnInitialized: purchased item " + product.definition.id +
                                      " which is defined as noAds. notifying TT Plugins. persistency check.");
                            purchaseAd = true;
                        }
                    }
                }
            }

            if (purchaseAd)
            {
                if (_reportNoAdsPurchasedAction != null)
                    _reportNoAdsPurchasedAction.Invoke(true);
            }
            else
            {
                Debug.Log(
                    "Billing::OnInitialized: did not find any noAds item subscribed or purchased. Notifying TT Plugins.");
                if (_reportNoAdsPurchasedAction != null)
                    _reportNoAdsPurchasedAction.Invoke(false);
            }
        }

        private void OnDeferred(TTPIProduct item)
        {
            Debug.Log("Billing::OnDeferred:Purchase deferred: " + item.definition.id);
        }

        public void OnInitializeFailed(TTPIInitializationFailureReason error)
        {
            _isInitialised = false;
            if (_reportNoAdsPurchasedAction != null)
                _reportNoAdsPurchasedAction.Invoke(false);
            Debug.Log("Billing::Unity billing failed to initialize!");
            switch (error)
            {
                case TTPIInitializationFailureReason.AppNotKnown:
                    Debug.Log(
                        "Billing::Failed to initialize - Is your App correctly uploaded on the relevant publisher console?");
                    _onBillingInitAction.Invoke(BillerErrors.APP_NOT_KNOWN);
                    break;
                case TTPIInitializationFailureReason.PurchasingUnavailable:
                    // Ask the user if billing is disabled in device settings.
                    Debug.Log("Billing::Failed to initialize - Billing disabled!");
                    _onBillingInitAction.Invoke(BillerErrors.PURCHASING_UNAVAILABLE);
                    break;
                case TTPIInitializationFailureReason.NoProductsAvailable:
                    // Developer configuration error; check product metadata.
                    Debug.Log("Billing::Failed to initialize - No products available for purchase!");
                    _onBillingInitAction.Invoke(BillerErrors.NO_PRODUCTS_AVAILABLE);
                    break;
            }
        }

        public void OnPurchaseFailed(TTPIProduct item, TTPIPurchaseFailureReason reason)
        {
            Debug.Log("Billing::OnPurchaseFailed:" + item.metadata.localizedTitle);

            PurchaseIAPResultCode resultCode = PurchaseIAPResultCode.Failed;
            BillerErrors error = BillerErrors.NO_ERROR;
            InAppPurchasableItem inApp = CreatePurchasableInAppItem(item);

            switch (reason)
            {
                case TTPIPurchaseFailureReason.ExistingPurchasePending:
                    error = BillerErrors.ATTEMPTING_TO_PURCHASE_PRODUCT_WITH_SAME_RECEIPT;
                    break;
                case TTPIPurchaseFailureReason.PaymentDeclined:
                    error = BillerErrors.PAYMENT_DECLINED;
                    break;
                case TTPIPurchaseFailureReason.ProductUnavailable:
                    error = BillerErrors.PRODUCT_UNAVAILABLE;
                    break;
                case TTPIPurchaseFailureReason.PurchasingUnavailable:
                    error = BillerErrors.PURCHASING_UNAVAILABLE;
                    break;
                case TTPIPurchaseFailureReason.SignatureInvalid:
                    error = BillerErrors.REMOTE_VALIDATION_FAILED;
                    break;
                case TTPIPurchaseFailureReason.Unknown:
                    error = BillerErrors.UNKNOWN;
                    break;
                case TTPIPurchaseFailureReason.UserCancelled:
                    resultCode = PurchaseIAPResultCode.Cancelled;
                    break;
            }

            _isPurchaseInProgress = false;

            PurchaseIAPResult result = new PurchaseIAPResult(inApp, resultCode, error);
            _onPurchasedAction.Invoke(result);
        }

        private string GetAndroidPurchaseToken(string payload)
        {
            if (payload != null) {
                Dictionary<string,object> tmp = TTPJson.Deserialize(payload) as Dictionary<string,object>;
                if (tmp != null) {
                    string tmpStr = tmp["json"] as string;
                    if (tmpStr != null) {
                        Dictionary<string,object> innerTmp = TTPJson.Deserialize(tmpStr) as Dictionary<string,object>;
                        if (innerTmp != null) {
                            return innerTmp["purchaseToken"] as string;
                        }
                        else {
                            Debug.Log ("ProcessPurchase failed to Deserialize json");
                        }
                    }
                    else {
                        Debug.Log ("ProcessPurchase failed to find json attribut in payload");
                    }
                }
                else {
                    Debug.Log ("ProcessPurchase failed to Deserialize payload");
                }
            } 
            else {
                Debug.Log ("ProcessPurchase failed to find payload attribute");
            }
            return null;
        }

        public TTPIPurchaseProcessingResult ProcessPurchase(TTPIPurchaseEventArgs e)
        {
            Debug.Log("Billing::Processing Purchase: " + e.purchasedProduct.definition.id);
            Debug.Log("Billing::Receipt: " + e.purchasedProduct.receipt);
            Debug.Log("Billing::Transaction Id: " + e.purchasedProduct.transactionID);

            _iapInValidation = CreatePurchasableInAppItem(e);
            _iapInValidationArgs = e;

            Dictionary<string, object> recieptJson =
                TTPJson.Deserialize(_iapInValidation.receipt) as Dictionary<string, object>;
            string payload = recieptJson["Payload"] as string;
            string purchaseToken = null;

#if UNITY_ANDROID
            purchaseToken = GetAndroidPurchaseToken(payload);
#endif

            // If restore is in progress - Skip receipt validation ! We have to do this because the server will not validate a receipt that has already been validated in the past
            if (_isRestoreInProgress)
            {
                // Indicate we have handled this purchase, we will not be informed of it again.
                Debug.Log("Billing::ProcessPurchase : Skipping receipt validation since restore is in progress");
                if (IsNoAdsItem(_iapInValidation.id))
                {
                    Debug.Log("Billing::ProcessPurchase : IsNoAdsItem=true calling PurchaseAd");
                    if (_reportNoAdsPurchasedAction != null)
                        _reportNoAdsPurchasedAction.Invoke(true);
                }
                else
                {
                    Debug.Log("Billing::ProcessPurchase : IsNoAdsItem=false");
                }

                PurchaseIAPResult result = new PurchaseIAPResult(_iapInValidation, PurchaseIAPResultCode.Success,
                    BillerErrors.NO_ERROR);
                SavePurchaseLocally(_iapInValidation.id);
                _onPurchasedAction.Invoke(result);
                _isPurchaseInProgress = false;
#if UNITY_ANDROID
                if (purchaseToken != null)
                {
                    if (e.purchasedProduct.definition.type == TTPIProductType.Subscription)
                    {
                        SubscriptionInfo subscriptionInfo = GetSubscriptionInfo(e.purchasedProduct.definition.id);
                        if (subscriptionInfo != null && subscriptionInfo.isSubscribed() == Result.True)
                        {
                            _validatePurchaseAction.Invoke(purchaseToken, _iapInValidation.localizedPrice,
                                _iapInValidation.currency, _iapInValidation.id, null, null, null);
                        }
                    }
                }
#endif
                return TTPIPurchaseProcessingResult.Complete;
            }
            
            string transactionId = e.purchasedProduct.transactionID;
            string originalTransactionId = null;
            bool? isFreeTrial = null;
#if UNITY_IOS && !UNITY_EDITOR
            purchaseToken = payload;
            
            bool isSubscription = e.purchasedProduct.definition.type == TTPIProductType.Subscription;
            bool isActiveSubscription = false;
            bool isFreeTrail = false;

            Debug.Log("ProcessPurchase: isSubscription: " + isSubscription);
            
            if (isSubscription)
            {
                SubscriptionInfo ttpSubscriptionInfo = GetSubscriptionInfo(e.purchasedProduct.definition.id);
                isActiveSubscription = ttpSubscriptionInfo != null && ttpSubscriptionInfo.isSubscribed() == Result.True;
                isFreeTrial = ttpSubscriptionInfo.isFreeTrial() == Result.True;
            }

            if (!isSubscription || (isSubscription && isActiveSubscription))
            {

                Debug.Log("ProcessPurchase: is not subscription or is active subscription");
                Debug.Log("ProcessPurchase: will try to get receipt data");

                TTPConfigurationBuilder builder =
                        TTPConfigurationBuilder.Instance(TTPStandardPurchasingModule.Instance());
                    TTPIAppleConfiguration appleConfig = builder.Configure<TTPIAppleConfiguration>();
                    byte[] receiptData = Convert.FromBase64String(appleConfig.appReceipt);

                    Debug.Log("ProcessPurchase : receipt data:" + receiptData);

                    if (receiptData != null)
                    {
                        Debug.Log("ProcessPurchase: receipt data is not null");

                        TTPAppleReceipt appleReceipt =
                            new TTPAppleValidator(TTPAppleTangle.Data()).Validate(receiptData);
                        if (appleReceipt != null)
                        {
                            Debug.Log("ProcessPurchase: succeeded to create apple receipt item for -" +
                                      e.purchasedProduct.definition.id);
                            foreach (TTPAppleInAppPurchaseReceipt inAppPurchaseReceipt in appleReceipt
                                         .inAppPurchaseReceipts)
                            {
                                Debug.Log("ProcessPurchaseDebug : productID=" + inAppPurchaseReceipt.productID +
                                          " originalTransactionIdentifier=" +
                                          inAppPurchaseReceipt.originalTransactionIdentifier +
                                          " transactionID=" + inAppPurchaseReceipt.transactionID);
                            }

                            TTPAppleInAppPurchaseReceipt currentIAPReceipt = null;
                            foreach (TTPAppleInAppPurchaseReceipt inAppPurchaseReceipt in appleReceipt
                                         .inAppPurchaseReceipts)
                            {
                                Debug.Log("ProcessPurchase : apple iap receipt found for - " +
                                          inAppPurchaseReceipt.productID);
                                if (inAppPurchaseReceipt.productID == e.purchasedProduct.definition.id
                                    && e.purchasedProduct.transactionID == inAppPurchaseReceipt.transactionID)
                                {
                                    Debug.Log("ProcessPurchase : found receipt for current product!");
                                    currentIAPReceipt = inAppPurchaseReceipt;
                                    break;
                                }
                            }

                            if (currentIAPReceipt != null)
                            {
                                originalTransactionId = currentIAPReceipt.originalTransactionIdentifier;
                                transactionId = currentIAPReceipt.transactionID;
                            }
                        }
                        else
                        {
                            Debug.Log("ProcessPurchase : could not parse apple receipt");
                        }
                    }
            }
            else {
                Debug.Log("ProcessPurchase: is subscription but not active; isSubscription: " + isSubscription 
                    + "; isActiveSubscription: " + isActiveSubscription);
            }
#endif
            if (purchaseToken != null)
            {
                Debug.Log("ProcessPurchase purchaseToken: " + purchaseToken);
                _validatePurchaseAction.Invoke(purchaseToken, _iapInValidation.localizedPrice,
                    _iapInValidation.currency, _iapInValidation.id, transactionId, originalTransactionId, isFreeTrial);
            }
            else
            {
                Debug.Log("ProcessPurchase purchaseToken was not found.");
            }

            OnValidPurchaseResponse(_iapInValidation.localizedPrice, _iapInValidation.currency, _iapInValidation.id,
                true);
            return TTPIPurchaseProcessingResult.Complete;
        }

#endregion

        private string NAIfNull(string val)
        {
            return val ?? "NA";
        }

        private void OnValidPurchaseResponse(float price, string currency, string itemId, bool isValid)
        {
            Debug.Log("Billing::OnValidPurchaseResponseEvent: Receipt validation result : " + isValid);
            PurchaseIAPResult result;
            if (isValid)
            {
                result = new PurchaseIAPResult(_iapInValidation, PurchaseIAPResultCode.Success, BillerErrors.NO_ERROR);
                SavePurchaseLocally(_iapInValidation.id);
                if (IsNoAdsItem(_iapInValidation.id))
                {
                    Debug.Log("Billing::OnValidPurchaseResponseEvent IsNoAdsItem=true calling PurchaseAd");
                    if (_reportNoAdsPurchasedAction != null)
                        _reportNoAdsPurchasedAction.Invoke(true);
                }
                else
                {
                    Debug.Log("Billing::OnValidPurchaseResponseEvent IsNoAdsItem=false");
                }
            }
            else
            {
                Debug.Log("Billing::OnValidPurchaseResponseEvent item is not valid");
                result = new PurchaseIAPResult(_iapInValidation, PurchaseIAPResultCode.Failed,
                    BillerErrors.REMOTE_VALIDATION_FAILED);
            }

            _onPurchasedAction.Invoke(result);
            _isPurchaseInProgress = false;
            _storeController.ConfirmPendingPurchase(_iapInValidationArgs.purchasedProduct);
            ResetPurchasableItem(_iapInValidation);
            _iapInValidationArgs = null;
        }

        private void SavePurchaseLocally(string iapId)
        {
            string iaps = PlayerPrefs.GetString(IAPS_PERSISTENCY_KEY, "");
            if (iaps.Length > 0)
                iaps += ";";
            iaps += iapId;
            PlayerPrefs.SetString(IAPS_PERSISTENCY_KEY, iaps);
        }

        private InAppPurchasableItem CreatePurchasableInAppItem(TTPIPurchaseEventArgs purchaseEventArgs)
        {
            TTPIProduct product = purchaseEventArgs.purchasedProduct;

            return CreatePurchasableInAppItem(product);
        }

        private InAppPurchasableItem CreatePurchasableInAppItem(TTPIProduct product)
        {
            InAppPurchasableItem purchasableInAppItem;
            purchasableInAppItem.id = product.definition.id;
            purchasableInAppItem.mainId = GetMainId(purchasableInAppItem.id);
            purchasableInAppItem.name = product.metadata.localizedTitle;
            purchasableInAppItem.description = product.metadata.localizedDescription;
            purchasableInAppItem.localizedPrice = (float)product.metadata.localizedPrice;
            purchasableInAppItem.receipt = product.receipt;
            purchasableInAppItem.transactionId = product.transactionID;
            purchasableInAppItem.currency = product.metadata.isoCurrencyCode;
            purchasableInAppItem.isSubscription = product.definition.type == TTPIProductType.Subscription;
            return purchasableInAppItem;
        }

        private void ResetPurchasableItem(InAppPurchasableItem purchasableInAppItem)
        {
            purchasableInAppItem.id = null;
            purchasableInAppItem.name = null;
            purchasableInAppItem.description = null;
            purchasableInAppItem.localizedPrice = -1f;
            purchasableInAppItem.receipt = null;
            purchasableInAppItem.transactionId = null;
            purchasableInAppItem.currency = null;
            purchasableInAppItem.isSubscription = false;
        }

        private string GetMainId(string storeId)
        {
            if (_iapMap != null)
            {
                foreach (KeyValuePair<string, string> kvp in _iapMap)
                {
                    if (kvp.Value == storeId)
                    {
                        return kvp.Key;
                    }
                }
            }

            return "";
        }

        private string GetString(Dictionary<string, object> dictionary, string str, string def)
        {
            if (dictionary != null && dictionary.ContainsKey(str) && dictionary[str] is string)
            {
                return dictionary[str] as string;
            }

            return def;
        }

        private bool GetBool(Dictionary<string, object> dictionary, string str)
        {
            if (dictionary != null && dictionary.ContainsKey(str) && dictionary[str] is bool)
            {
                return (bool)dictionary[str];
            }

            return false;
        }

        private void ReadProductInfo(TTPConfigurationBuilder builder)
        {
            _iapMap = new Dictionary<string, string>();
            List<object> productArr = _configuration != null ? _configuration["iaps"] as List<object> : null;
            if (productArr != null)
            {
                foreach (object productObj in productArr)
                {
                    if (productObj.GetType() == typeof(Dictionary<string, object>))
                    {
                        Dictionary<string, object> productDict = productObj as Dictionary<string, object>;
                        string productTypeStr = GetString(productDict, "type", "");
                        TTPIProductType productType;
                        if (TryParseProductType(productTypeStr, out productType))
                        {
                            string iapId = GetString(productDict, "iapId", "");
                            string itemId = GetString(productDict, "id", "");
                            bool isNoAds = GetBool(productDict, "noAds");
                            if (itemId.Length > 0 && iapId.Length > 0)
                            {
                                Debug.Log("Billing::ReadProductInfo:Adding Product - iapId = " + iapId + ", id = " +
                                          itemId + ", type = " + productTypeStr + ", noAds = " + isNoAds.ToString());
                                _iapMap.Add(itemId, iapId);
                                if (isNoAds)
                                {
                                    _noAdsIapIds += ";" + iapId;
                                }

                                string storeName = (Application.platform == RuntimePlatform.IPhonePlayer)
                                    ? TTPAppleAppStore.Name
                                    : TTPGooglePlay.Name;
                                Debug.Log("Billing::ReadProductInfo:" + storeName + " selected");
                                builder.AddProduct(iapId, productType, new TTPIDs
                                {
                                    { iapId, storeName }
                                });
                            }
                        }
                    }
                }
            }
        }

        private bool TryParseProductType(string productTypeStr, out TTPIProductType productType)
        {
            productType = TTPIProductType.Consumable;
            if (productTypeStr == "consumable")
            {
                productType = TTPIProductType.Consumable;
                return true;
            }
            else if (productTypeStr == "non-consumable")
            {
                productType = TTPIProductType.NonConsumable;
                return true;
            }
            else if (productTypeStr == "subscription")
            {
                productType = TTPIProductType.Subscription;
                return true;
            }
            else
            {
                Debug.LogError("Billing::ParseProductType: failed to parse product type - " + productTypeStr);
                return false;
            }
        }

        //copied from IAPDemo.cs
        private bool CheckIfProductIsAvailableForSubscriptionManager(string receipt)
        {
            if (receipt == null)
            {
                Debug.Log("Billing::CheckIfProductIsAvailableForSubscriptionManager:receipt is null");
                return false;
            }

            var receipt_wrapper = (Dictionary<string, object>)TTPJson.Deserialize(receipt);
            if (!receipt_wrapper.ContainsKey("Store") || !receipt_wrapper.ContainsKey("Payload"))
            {
                Debug.Log(
                    "Billing::CheckIfProductIsAvailableForSubscriptionManager:The product receipt does not contain enough information");
                return false;
            }

            var store = (string)receipt_wrapper["Store"];
            var payload = (string)receipt_wrapper["Payload"];
            Debug.Log("Billing::CheckIfProductIsAvailableForSubscriptionManager:Payload = " + payload);

            if (payload != null)
            {
                switch (store)
                {
                    case TTPGooglePlay.Name:
                    {
                        return true;
                    }
                    case TTPAppleAppStore.Name:
                    case TTPAmazonApps.Name:
                    case TTPMacAppStore.Name:
                    {
                        return true;
                    }
                    default:
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public SubscriptionInfo GetSubscriptionInfo(string itemId)
        {
            string finalId = GetFinalId(itemId);
            if (!IsInitialisedWithLogging())
                return null;
            TTPIProduct item = _storeController.products.WithStoreSpecificID(finalId ?? "");
            if (item == null)
            {
                Debug.Log("Billing::GetSubscriptionInfo:Error, Cannot find item - " + finalId);
                return null;
            }

            if (item.definition.type != TTPIProductType.Subscription)
            {
                Debug.Log("Billing::GetSubscriptionInfo:Error, item is not a subscription - " + finalId);
                return null;
            }

            if (!CheckIfProductIsAvailableForSubscriptionManager(item.receipt))
            {
                Debug.Log("Billing::GetSubscriptionInfo:Error, item is not available in sub manager - " + finalId);
                return null;
            }

            Dictionary<string, string> introductory_info_dict = null;
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                introductory_info_dict = _appleExtensions.GetIntroductoryPriceDictionary();
            }

            string intro_json =
                (introductory_info_dict == null || !introductory_info_dict.ContainsKey(item.definition.storeSpecificId))
                    ? null
                    : introductory_info_dict[item.definition.storeSpecificId];
            TTPSubscriptionManager p = new TTPSubscriptionManager(item, intro_json);
            return p.getSubscriptionInfo();
        }

        private void OnApplicationFocus()
        {
            if (_isInitialised && _storeController != null)
            {
                Debug.Log("Billing::OnResume: DeterminePurchaseAd called");
                DeterminePurchaseAd(_storeController.products.all);
            }
        }

        private void RegisterAppLifeCycle(bool register)
        {
            System.Reflection.MethodInfo method = typeof(TTPCore).GetMethod("GetTTPGameObject",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                GameObject gameObject = method.Invoke(null, null) as GameObject;
                if (gameObject != null)
                {
                    TTPCore.TTPGameObject ttpGo = gameObject.GetComponent<TTPCore.TTPGameObject>();
                    if (register)
                        ttpGo.OnApplicationFocusEvent += OnApplicationFocus;
                    else
                        ttpGo.OnApplicationFocusEvent -= OnApplicationFocus;
                }
            }
            else
            {
                Debug.LogWarning("Billing::RegisterAppLifeCycle:: could not find method GetTTPGameObject");
            }
        }

        private void LogEventByReflection(long targets, string eventName, Dictionary<string, object> parameters)
        {
            var analyticsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPAnalytics");
            if (analyticsClsType != null)
            {
                var method = analyticsClsType.GetMethod("LogEvent",
                    new Type[]
                    {
                        typeof(long), typeof(string), typeof(IDictionary<string, object>), typeof(bool), typeof(bool)
                    });
                if (method != null)
                {
                    method.Invoke(null, new object[]
                    {
                        targets,
                        eventName, parameters, false, true
                    });
                }
                else
                {
                    Debug.LogWarning("LogEventByReflection:: could not find method - LogEvent");
                }
            }
            else
            {
                Debug.LogWarning("LogEventByReflection:: could not find TTPAnalytics class");
            }
        }

        public void OnInitializeFailed(TTPIInitializationFailureReason error, string message)
        {
            OnInitializeFailed(error);
            if (message != null)
            {
                Debug.LogWarning("OnInitializeFailed:: " + message ?? "");
            }
        }
    }
}