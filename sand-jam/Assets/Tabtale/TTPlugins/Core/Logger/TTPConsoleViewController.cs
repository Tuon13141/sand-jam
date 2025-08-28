using UnityEngine;
using UnityEngine.UI;

namespace Tabtale.TTPlugins
{
    public class TTPConsoleViewController : MonoBehaviour
    {
        GameObject panelConsole;
        GameObject buttonShow;
        GameObject buttonHide;
        GameObject buttonShare;
        GameObject buttonTestAb;
        GameObject buttonFilterEvents;
            
        void Start()
        {
            panelConsole = gameObject.transform.Find("Panel").gameObject;
            buttonShow = gameObject.transform.Find("ShowButton").gameObject;
            buttonShow.GetComponent<Button>().onClick.AddListener(OnShowConsoleClicked);
            buttonHide = gameObject.transform.Find("Panel/ButtonsRow/MinimazeButton").gameObject;
            buttonHide.GetComponent<Button>().onClick.AddListener(OnMinimizeConsoleClicked);
            buttonShare = gameObject.transform.Find("Panel/ButtonsRow/ShareButton").gameObject;
            buttonShare.GetComponent<Button>().onClick.AddListener(OnShareClicked);
            buttonTestAb = gameObject.transform.Find("Panel/ButtonsRow/TestAbButton").gameObject;
            buttonTestAb.GetComponent<Button>().onClick.AddListener(OnTestAbClicked);
            buttonFilterEvents = gameObject.transform.Find("Panel/ButtonsRow/FilterEventsButton").gameObject;
            buttonFilterEvents.GetComponent<Button>().onClick.AddListener(OnFilterEventsClicked);
            gameObject.SetActive(true);
            TTPTestAB.OnHideConsole = () =>
            {
                panelConsole.SetActive(true);
            };
        }

        void OnTestAbClicked()
        {
            panelConsole.SetActive(false);
            TTPTestAB.Show();
        }
        
        void OnShowConsoleClicked()
        {
            panelConsole.SetActive(true);
            buttonShow.SetActive(false);
            Debug.Log("TTPLog::OnShowConsoleClicked");
        }

        void OnMinimizeConsoleClicked()
        {
            panelConsole.SetActive(false);
            buttonShow.SetActive(true);
            Debug.Log("TTPLog::OnMinimizeConsoleClicked");
        }

        void OnShareClicked()
        {
            Debug.Log("TTPLog::OnShareClicked");
#if !UNITY_EDITOR && TTP_DEV_MODE
            TTPLogger.FlushBuffer();
            string path = TTPLogger.GetFilePath();
            if (path.Length != 0)
            {
                NativeShare.Share("", path, "", "logfile");
            }
#endif
        }

        void OnFilterEventsClicked()
        {
            var listViewObject = gameObject.transform.Find("Panel/VerticalListView").gameObject;
            var listView = listViewObject.GetComponent<TTPLogLoggerListView>();
            listView.ToggleEventFilter();
        }
    }
}