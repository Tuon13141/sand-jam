using UnityEngine;
using UnityEngine.UI;

public class AudioInteraction : MonoBehaviour
{
    [SerializeField] string m_AudioName = "sfx_tap";
    [SerializeField] bool m_ToggleSoundOff;
    Button m_Button;
    Toggle m_Toggle;

    void Awake()
    {
        m_Button = GetComponent<Button>();
        if (m_Button != null)
        {
            m_Button.onClick.AddListener(OnButtonTap);
        }

        m_Toggle = GetComponent<Toggle>();
        if (m_Toggle != null)
        {
            m_Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    void OnButtonTap()
    {
        if (AudioManager.instance != null && !string.IsNullOrEmpty(m_AudioName))
        {
            AudioManager.instance.PlaySfx(m_AudioName);
        }
    }

    void OnToggleValueChanged(bool isOn)
    {
        if (AudioManager.instance != null && !string.IsNullOrEmpty(m_AudioName) && (isOn || m_ToggleSoundOff))
        {
            AudioManager.instance.PlaySfx(m_AudioName);
        }
    }
}
