using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;

public class AudioManager : MonoBehaviour
{
    public static float DefaultBgmVolume = .5f;
    public static float DefaultSfxVolume = 1;
    public static float DefaultVoiceVolume = 1;
    public static float DefaultBgmVolumeScale = .5f;
    public static float DefaultSfxVolumeScale = .5f;
    public static float DefaultVoiceVolumeScale = 1;

    static float bgmVolume = -1;
    static float sfxVolume = -1;
    static float voiceVolume = -1;

    static float bgmVolumeScale = 1;

    public static float BgmVolume
    {
        get
        {
            if (bgmVolume == -1)
            {
                bgmVolume = PlayerPrefs.GetFloat("BGM_VOLUME", DefaultBgmVolume);
            }

            return bgmVolume;
        }

        set
        {
            bgmVolume = value;
            instance.m_BgmSource.volume = bgmVolume * DefaultBgmVolumeScale * bgmVolumeScale;
        }
    }

    public static float SfxVolume
    {
        get
        {
            if (sfxVolume == -1)
            {
                sfxVolume = PlayerPrefs.GetFloat("SFX_VOLUME", DefaultSfxVolume);
            }

            return sfxVolume;
        }

        set
        {
            sfxVolume = value;
        }
    }

    public static float VoiceVolume
    {
        get
        {
            if (voiceVolume == -1)
            {
                voiceVolume = PlayerPrefs.GetFloat("VOICE_VOLUME", DefaultVoiceVolume);
            }

            return voiceVolume;
        }

        set
        {
            voiceVolume = value;
        }
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat("BGM_VOLUME", bgmVolume);
        PlayerPrefs.SetFloat("SFX_VOLUME", sfxVolume);
        PlayerPrefs.SetFloat("VOICE_VOLUME", voiceVolume);
        PlayerPrefs.Save();
    }

    [SerializeField] string m_BgmPath = "Sounds/Bgm";
    [SerializeField] string m_SfxPath = "Sounds/Sfx";
    [SerializeField] string m_VoicePath = "Sounds/Voice";
    [SerializeField] string m_ButtonTapSfx = "Button";
    [SerializeField] AudioSource m_BgmSource;
    [SerializeField] AudioSource m_SfxSource;
    [SerializeField] AudioSource m_VoiceSource;

    public static AudioManager instance { get; protected set; }

    Dictionary<string, AudioClip> m_AudioDict = new Dictionary<string, AudioClip>();
    bool m_PauseBgm;

    public string sfxPath
    {
        get { return m_SfxPath; }
    }

    public string bgmPath
    {
        get { return m_BgmPath; }
    }

    public string voicePath
    {
        get { return m_VoicePath; }
    }

    public AudioSource voiceSource
    {
        get { return m_VoiceSource; }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void PlayButtonTapSfx()
    {
        PlaySfx(m_ButtonTapSfx);
    }

    public void PlaySfx(AudioClip audioClip, float volumeScale = 1f)
    {
        m_SfxSource.PlayOneShot(audioClip, SfxVolume * DefaultSfxVolumeScale * volumeScale);
    }

    public void PlaySfx(string audioName, float volumeScale = 1f, bool oneShot = true)
    {
        CacheClip(m_SfxPath, audioName);

        if (oneShot)
        {
            m_SfxSource.PlayOneShot(m_AudioDict[audioName], SfxVolume * DefaultSfxVolumeScale * volumeScale);
        }
        else
        {
            m_BgmSource.clip = m_AudioDict[audioName];
            m_SfxSource.Play();
        }
    }

    public float GetLength(string audioName)
    {
        return (m_AudioDict.ContainsKey(audioName) ? m_AudioDict[audioName].length : 0);
    }

    public float PlayVoice(AudioClip audioClip, float volumeScale = 1f)
    {
        m_VoiceSource.PlayOneShot(audioClip, VoiceVolume * DefaultVoiceVolumeScale * volumeScale);
        return audioClip.length;
    }

    //public async UniTask<float> PlayVoice(string audioPath, float volumeScale = 1f)
    //{
    //    var audioClip = await BuchaPresenter.instance.LoadAudioLocal(audioPath);

    //    PlayVoice(audioClip, volumeScale);
    //    return audioClip.length;
    //}

    public void SetVoicePitch(float pitch)
    {
        m_VoiceSource.pitch = pitch;
    }

    public void PlayBgm(string audioName, float volumeScale = 1f)
    {
        bgmVolumeScale = volumeScale;
        m_BgmSource.volume = BgmVolume * DefaultBgmVolumeScale * bgmVolumeScale;

        bool currentClipNull = (m_BgmSource.clip == null);
        bool sameCurrentClip = (!currentClipNull && m_BgmSource.clip.name == audioName);

        if (sameCurrentClip)
        {
            if (!m_BgmSource.isPlaying)
            {
                m_BgmSource.Play();
            }
        }
        else
        {
            if (!currentClipNull)
            {
                StopBgm(true);
            }
            m_BgmSource.clip = Resources.Load<AudioClip>(System.IO.Path.Combine(m_BgmPath, audioName));
            m_BgmSource.Play();
        }
    }

    public void StopBgm(bool clearClip = false)
    {
        if (m_BgmSource.isPlaying)
        {
            m_BgmSource.Stop();
        }

        if (clearClip && m_BgmSource.clip != null)
        {
            Resources.UnloadAsset(m_BgmSource.clip);
            m_BgmSource.clip = null;
        }
    }

    public bool pauseBgm
    {
        set
        {
            m_PauseBgm = value;

            if (m_PauseBgm)
            {
                m_BgmSource.Pause();
            }
            else
            {
                m_BgmSource.UnPause();
            }
        }
    }

    public void CacheClip(AudioClip clip, string audioName)
    {
        if (!m_AudioDict.ContainsKey(audioName))
        {
            m_AudioDict.Add(audioName, clip);
        }
    }

    public void CacheClip(string path, string audioName)
    {
        if (!m_AudioDict.ContainsKey(audioName))
        {
            AudioClip clip = Resources.Load<AudioClip>(System.IO.Path.Combine(path, audioName));
            m_AudioDict.Add(audioName, clip);
        }
    }

    public void CacheSfx(string audioName)
    {
        CacheClip(m_SfxPath, audioName);
    }

    public void ClearCacheClip(string audioName, bool unloadClip)
    {
        if (m_AudioDict.ContainsKey(audioName))
        {
            if (unloadClip)
            {
                Resources.UnloadAsset(m_AudioDict[audioName]);
            }
            m_AudioDict.Remove(audioName);
        }
    }

    public void ClearCacheClip()
    {
        foreach (var item in m_AudioDict)
        {
            Resources.UnloadAsset(item.Value);
        }
        m_AudioDict.Clear();
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
    }
}