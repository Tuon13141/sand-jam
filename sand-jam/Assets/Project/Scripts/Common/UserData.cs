using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UserData
{
    public List<int> collectionProcess;

    public int GetCollectionProcess(int index) //index start 0
    {
        if (index < 0) return 0;

        if (collectionProcess == null || collectionProcess.Count == 0)
        {
            collectionProcess = new List<int>(new int[index + 1]);
            return 0;
        }
        else if (index < collectionProcess.Count)
        {
            return collectionProcess[index];
        }
        else
        {
            var tempList = new List<int>(new int[index - collectionProcess.Count + 1]);
            collectionProcess.AddRange(tempList);
            return 0;
        }
    }
    public void SetCollectionProcess(int index, int value)
    {
        if (index < 0 || value < 0) return;

        var currentValue = GetCollectionProcess(index);
        if (currentValue != value)
        {
            collectionProcess[index] = value;
            User.Save();
        }
    }
}

public static class User
{
    public static UserData data;

    static User()
    {
        Load();
    }

    public static void Load()
    {
        var textData = PlayerPrefs.GetString("USER_DATA", string.Empty);

        if (!string.IsNullOrEmpty(textData))
        {
            data = JsonConvert.DeserializeObject<UserData>(textData);
        }
        else
        {
            data = new UserData();
            User.Save();
        }
    }

    public static void Save()
    {
        var textData = JsonConvert.SerializeObject(data);
        PlayerPrefs.SetString("USER_DATA", textData);
        PlayerPrefs.Save();
    }
}