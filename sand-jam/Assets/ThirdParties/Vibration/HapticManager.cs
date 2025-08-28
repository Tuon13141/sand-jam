using UnityEngine;

public class HapticManager : MonoBehaviour
{
    enum VibrateType
    {
        Default,
        RepeatLimit,
        RepeatUnlimit
    }

    [SerializeField] float m_MinTime = 0.2f;

    int m_Count;
    int m_MaxCount;

    float m_Time;
    VibrateType m_VibrateType;

    public float MinTime
    {
        get
        {
            return m_MinTime;
        }

        set
        {
            m_MinTime = value;
        }
    }

    public static HapticManager instance
    {
        get;
        protected set;
    }

    public static bool vibrateValue
    {
        get
        {
            return PlayerPrefs.GetInt("VIBRATION", 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("VIBRATION", value ? 1 : 0);
        }
    }

    public static void VibrateOne()
    {
        if (instance != null && PlayerPrefs.GetInt("VIBRATION", 1) == 1)
        {
            instance.Vibrate();
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;

            m_Time = m_MinTime;

            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        m_Time += Time.deltaTime;

        switch (m_VibrateType)
        {
            case VibrateType.RepeatLimit:
                if (Vibrate())
                {
                    m_Count++;
                }

                if (m_Count >= m_MaxCount)
                {
                    m_VibrateType = VibrateType.Default;
                }
                break;

            case VibrateType.RepeatUnlimit:
                Vibrate();
                break;
        }
    }

    public bool Vibrate()
    {
        if (vibrateValue == false) return false;
        if (m_Time >= m_MinTime)
        {
            Vibration.VibratePeek();
            m_Time = 0;

            return true;
        }

        return false;
    }

    public void StartVibrateRepeat(int maxCount = 0)
    {
        if (maxCount < 0)
            return;

        if (maxCount == 0)
        {
            m_VibrateType = VibrateType.RepeatUnlimit;
        }
        else
        {
            m_VibrateType = VibrateType.RepeatLimit;
            m_MaxCount = maxCount;
            m_Count = 0;
        }
    }

    public void StopVibrate()
    {
        m_VibrateType = VibrateType.Default;
    }
}
