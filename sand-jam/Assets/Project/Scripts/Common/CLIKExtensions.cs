using System;
using System.Collections.Generic;
using Tabtale.TTPlugins;
using UnityEngine;

public static class CLIKExtensions
{
    //static CLIKExtensions()
    //{
    //    ClikSetup();
    //}

    #region Analytics
    public static void ClikSetup()
    {
        Debug.Log("ClikSetup First");
        TTPCore.Setup();
    }

    public static void ClikMissionStart(int missionID)
    {
        var levelDict = new Dictionary<string, object> { { "event", "start" } };
        TTPGameProgression.FirebaseEvents.MissionStarted(missionID, levelDict);
    }

    public static void ClikMissionComplete()
    {
        var levelDict = new Dictionary<string, object> { { "event", "complete" } };
        TTPGameProgression.FirebaseEvents.MissionComplete(levelDict);
    }

    public static void ClikMissionFail()
    {
        var levelDict = new Dictionary<string, object> { { "event", "fail" } };
        TTPGameProgression.FirebaseEvents.MissionFailed(levelDict);
    }
    #endregion

    #region ADS
    public static bool RewardAdsIsReady()
    {
        return TTPRewardedAds.IsReady();
    }

    public static void OnRewardedAdsButtonClicked(string nameEvent, Action<bool> callback)
    {
        if (TTPRewardedAds.IsReady())
        {
            Debug.Log(nameEvent);
            TTPRewardedAds.Show(nameEvent, callback);
        }
        else
        {
            callback.Invoke(false);
        }
    }
    #endregion
}
