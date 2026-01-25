using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Toggle musicToggle;
    public Toggle sfxToggle;

    void Start()
    {
        // Initialize toggles with current settings
        if (AudioManager.Instance != null)
        {
            musicToggle.isOn = AudioManager.Instance.musicEnabled;
            sfxToggle.isOn = AudioManager.Instance.sfxEnabled;
        }

        // Add listeners
        musicToggle.onValueChanged.AddListener(OnMusicToggle);
        sfxToggle.onValueChanged.AddListener(OnSFXToggle);
    }

    void OnMusicToggle(bool value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusic(value);
        }
    }

    void OnSFXToggle(bool value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleSFX(value);
        }
    }
}
