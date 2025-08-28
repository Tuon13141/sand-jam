using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasTest : Kit.Common.Singleton<CanvasTest>
{
    [SerializeField] GameObject m_WinPanel;
    [SerializeField] GameObject m_LostPanel;
    [SerializeField] GameObject m_SelectPanel;
    [SerializeField] GameObject m_panelClearBlock, m_panelColorRemove;
    [SerializeField] GameObject m_GroupBooster;

    public void ShowWinPanel()
    {
        m_WinPanel.SetActive(true);
    }

    public void ShowLostPanel()
    {
        m_LostPanel.SetActive(true);
    }

    public void ShowSelectPanel(bool isColorRemoveBooster)
    {
        m_SelectPanel.SetActive(true);
        //Percas.GameController.OnActiveUI?.Invoke(false);

        m_panelColorRemove.SetActive(isColorRemoveBooster);
        m_panelClearBlock.SetActive(!isColorRemoveBooster);
    }

    public void HideSelectPanel()
    {
        m_SelectPanel.SetActive(false);
        //Percas.GameController.OnActiveUI?.Invoke(true);
    }

    public void RetryLevel()
    {
        Debug.Log("RetryLevel");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void NextLevel()
    {
        Debug.Log("RetryLevel");
        //PlayerPrefs.SetInt("CURRENT_LEVEL", LevelController.instance.levelIndex + 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void ResetScene()
    {
        Debug.Log("ResetScene");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextScene()
    {
        Debug.Log("NextScene");
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void PrevScene()
    {
        Debug.Log("PrevScene");
        int prevSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (prevSceneIndex >= 0)
        {
            SceneManager.LoadScene(prevSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
        }
    }
}
