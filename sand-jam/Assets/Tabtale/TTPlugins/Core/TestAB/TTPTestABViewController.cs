using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Tabtale.TTPlugins
{
    public class TTPTestABViewController : MonoBehaviour
    {
        private string _selectedExperiment;
        private string _selectedVariant;
        private readonly Dictionary<string, GameObject> _buttons = new Dictionary<string, GameObject>();
        
        private GameObject _panelConsole;
        private GameObject _buttonBack;
        private GameObject _buttonHide;
        private GameObject _buttonSet;

        private void Start()
        {
            _panelConsole = gameObject.transform.Find("Panel").gameObject;
            
            _buttonBack = gameObject.transform.Find("Panel/BackButton").gameObject;
            _buttonBack.GetComponent<Button>().onClick.AddListener(OnBackClicked);
            _buttonBack.SetActive(false);
            
            _buttonHide = gameObject.transform.Find("Panel/MinimazeButton").gameObject;
            _buttonHide.GetComponent<Button>().onClick.AddListener(OnMinimizeConsoleClicked);
            
            _buttonSet = gameObject.transform.Find("Panel/SetButton").gameObject;
            _buttonSet.SetActive(false);
            
            gameObject.SetActive(true);
        }

        public void ReloadData()
        {
            Debug.Log("TTPTestABViewController::ReloadData");
           ShowAllExperiments();
        }

        public void Show()
        {
            Debug.Log("TTPTestABViewController::Show");
            _panelConsole.SetActive(true);
            ReloadData();
        }

        /// Actions

        private void OnBackClicked()
        {
            Debug.Log("TTPTestABViewController::OnBackClicked");
            ShowAllExperiments();
        }
        
        private void OnMinimizeConsoleClicked()
        {
            Debug.Log("TTPTestABViewController::OnMinimizeConsoleClicked");
            
            _selectedExperiment = "";
            _selectedVariant = "";
            _buttonBack.SetActive(false);
            _panelConsole.SetActive(false);
            if (TTPTestAB.OnHideConsole != null)
            {
                TTPTestAB.OnHideConsole();
            }
        }
        
        /// Private

        private void ShowAllExperiments()
        {
            Debug.Log("TTPTestABViewController::ShowAllExperiments");

            if (string.IsNullOrEmpty(_selectedExperiment))
            {
                _selectedExperiment = TTPTestAB.LoadedExperiment;
            }
            _buttonBack.SetActive(false);
            ShowButtonsList(TTPTestAB.GetExperiments(), experimentName =>
            {
                _selectedExperiment = experimentName;
                TTPTestAB.LoadedExperiment = experimentName;
                ShowAllVariants(TTPTestAB.GetVariantNamesForExperiment(experimentName));
            });
            SetActive(_selectedExperiment);
        }

        private void ShowAllVariants(string[] variantNames)
        {
            Debug.Log("TTPTestABViewController::ShowAllVariants");
            
            if (string.IsNullOrEmpty(_selectedVariant))
            {
                _selectedVariant = TTPTestAB.LoadedVariant;
            }
            _buttonBack.SetActive(true);
            ShowButtonsList(variantNames, variantName =>
            {
                _selectedVariant = variantName;
                TTPTestAB.LoadedVariant = variantName;
                SetActive(_selectedVariant);
                TTPTestAB.ApplyExperimentWithVariant(_selectedExperiment, _selectedVariant);
                Debug.Log("TTPTestABViewController::" + variantName + "=" +
                          TTPTestAB.GetVariantForExperiment(_selectedExperiment, variantName));
            });
            SetActive(_selectedVariant);
        }

        private void ShowButtonsList(string[] configurationNames, Action<string> action)
        {
            Debug.Log("TTPTestABViewController::ShowButtonsList - " + string.Join(", ", configurationNames));

            var buttons = _buttons.Values;
            foreach (var button in buttons)
            {
                DestroyImmediate(button);
            }
            
            _buttons.Clear();
            _buttonHide.transform.parent = null;
            foreach (var configurationName in configurationNames)
            {
                var button = Instantiate(_buttonSet, _panelConsole.transform, true);
                button.transform.Find("Text").GetComponent<Text>().text = configurationName;
                
                var rectTransform = button.GetComponent<RectTransform>();
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.offsetMin = Vector2.zero;
                
                button.GetComponent<Button>().onClick.AddListener(delegate
                {
                    action(configurationName);
                });
                
                button.SetActive(true);
                _buttons[configurationName] = button;
            }
            _buttonHide.transform.parent = _panelConsole.transform;
        }
        
        private void SetActive(string experimentName)
        {
            Debug.Log("TTPTestABViewController::SetActive - " + experimentName);
            
            foreach (var key in _buttons.Keys)
            {
                var img = _buttons[key].GetComponent<Image>();
                img.color = experimentName == key ? Color.green : Color.white;
            }
        }
    }
}
