//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class AudioCacheManager
//{
//    static string s_AudioPath = string.Empty;
//    static AudioClip s_AudioClip = null;

//    public static AudioClip clip
//    {
//        get
//        {
//            return s_AudioClip;
//        }
//    }

//    public static IEnumerator Cache(string gameMode, string audio)
//    {
//        string audioPath = string.Format("{0}_sound/{1}.mp3", gameMode, audio);

//        #if UNITY_ANDROID
//        if (s_AudioPath != audioPath || s_AudioClip == null)
//        {
//        #endif
//            if (s_AudioClip != null)
//            {
//                GameObject.Destroy(s_AudioClip);
//                s_AudioClip = null;
//            }

//            s_AudioPath = audioPath;

//            WWW www = new WWW("file://" + System.IO.Path.Combine(Application.persistentDataPath, audioPath));
//            yield return www;

//            s_AudioClip = www.GetAudioClip();
//        #if UNITY_ANDROID
//        }
//        #endif
//    }

//    public static void Load(string gameMode, string audio, MonoBehaviour mono, System.Action<AudioClip> callback)
//    {
//        string audioPath = string.Format("{0}_sound/{1}.mp3", gameMode, audio);

//        if (s_AudioPath != audioPath || s_AudioClip == null)
//        {
//            s_AudioPath = audioPath;
//            mono.StartCoroutine(CoLoad(audioPath, callback));
//        }
//        else
//        {
//            if (callback != null)
//            {
//                callback(s_AudioClip);
//            }
//        }
//    }

//    static IEnumerator CoLoad(string audioPath, System.Action<AudioClip> callback)
//    {
//        if (s_AudioClip != null)
//        {
//            GameObject.Destroy(s_AudioClip);
//            s_AudioClip = null;
//        }

//        WWW www = new WWW("file://" + System.IO.Path.Combine(Application.persistentDataPath, audioPath));
//        yield return www;
//        s_AudioClip = www.GetAudioClip();

//        if (callback != null)
//        {
//            callback(s_AudioClip);
//        }
//    }

//    public static void Clear()
//    {
//        s_AudioPath = string.Empty;

//        if (s_AudioClip != null)
//        {
//            GameObject.Destroy(s_AudioClip);
//            s_AudioClip = null;
//        }
//    }
//}