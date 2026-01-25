using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsMenu : MonoBehaviour
{
    [Header("Music Icon")]
    public Image musicIcon;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;
    private bool musicEnabled = true;

    [Header("SFX Icon")]
    public Image sfxIcon;
    public Sprite sfxOnSprite;
    public Sprite sfxOffSprite;
    private bool sfxEnabled = true;

    void Start()
    {
        // Initialize with current AudioManager settings
        if (AudioManager.Instance != null)
        {
            musicEnabled = AudioManager.Instance.musicEnabled;
            sfxEnabled = AudioManager.Instance.sfxEnabled;
        }

        // Update visuals
        UpdateMusicVisual();
        UpdateSFXVisual();

        // Add click detection to images
        AddClickListener(musicIcon, ToggleMusicButton);
        AddClickListener(sfxIcon, ToggleSFXButton);
    }

    private void AddClickListener(Image image, UnityEngine.Events.UnityAction action)
    {
        if (image == null) return;

        // Add Button component if it doesn't exist
        Button btn = image.GetComponent<Button>();
        if (btn == null)
        {
            btn = image.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None; // No visual transition
        }
        btn.onClick.AddListener(action);
    }

    public void ToggleMusicButton()
    {
        musicEnabled = !musicEnabled;
        UpdateMusicVisual();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusic(musicEnabled);
        }
    }

    public void ToggleSFXButton()
    {
        sfxEnabled = !sfxEnabled;
        UpdateSFXVisual();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleSFX(sfxEnabled);
        }
    }

    private void UpdateMusicVisual()
    {
        if (musicIcon != null)
        {
            musicIcon.sprite = musicEnabled ? musicOnSprite : musicOffSprite;
        }
    }

    private void UpdateSFXVisual()
    {
        if (sfxIcon != null)
        {
            sfxIcon.sprite = sfxEnabled ? sfxOnSprite : sfxOffSprite;
        }
    }
}
