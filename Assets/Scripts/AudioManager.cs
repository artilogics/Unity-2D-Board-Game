using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioClip musicIntro;
    public AudioClip[] musicLoops; // 5 loop files
    private int currentLoopIndex = 0;
    private bool hasPlayedIntro = false;

    [Header("Sound Effects")]
    public AudioClip jumpSFX;
    public AudioClip[] diceBounce; // Array of bounce sounds for variation

    [Header("Audio Sources")]
    private AudioSource musicSource;
    private AudioSource sfxSource;

    [Header("Settings")]
    public bool musicEnabled = true;
    public bool sfxEnabled = true;

    void Awake()
    {
        // Singleton pattern - persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAudioSettings(); // Load saved settings
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void LoadAudioSettings()
    {
        // Load saved settings (defaults to 1 = true if not set)
        musicEnabled = PlayerPrefs.GetInt("AudioMusicEnabled", 1) == 1;
        sfxEnabled = PlayerPrefs.GetInt("AudioSFXEnabled", 1) == 1;
        Debug.Log($"Loaded audio settings: Music={musicEnabled}, SFX={sfxEnabled}");
    }

    void SaveAudioSettings()
    {
        PlayerPrefs.SetInt("AudioMusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("AudioSFXEnabled", sfxEnabled ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Saved audio settings: Music={musicEnabled}, SFX={sfxEnabled}");
    }

    void SetupAudioSources()
    {
        // Create music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = false; // We'll handle looping manually
        musicSource.playOnAwake = false;

        // Create SFX source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    void Start()
    {
        if (musicEnabled)
        {
            PlayMusic();
        }
    }

    void Update()
    {
        // Check if music finished and start next track
        if (musicEnabled && !musicSource.isPlaying && (hasPlayedIntro || musicIntro == null))
        {
            PlayNextLoop();
        }
    }

    public void PlayMusic()
    {
        if (!musicEnabled) return;

        // Play intro first if we haven't yet
        if (!hasPlayedIntro && musicIntro != null)
        {
            musicSource.clip = musicIntro;
            musicSource.Play();
            hasPlayedIntro = true;
            Debug.Log("Playing intro music");
        }
        else
        {
            PlayNextLoop();
        }
    }

    private void PlayNextLoop()
    {
        if (musicLoops == null || musicLoops.Length == 0) return;

        // Pick random loop (never the current one if we have more than 1)
        if (musicLoops.Length > 1)
        {
            int newIndex;
            do
            {
                newIndex = Random.Range(0, musicLoops.Length);
            } while (newIndex == currentLoopIndex);
            currentLoopIndex = newIndex;
        }
        else
        {
            currentLoopIndex = 0;
        }

        musicSource.clip = musicLoops[currentLoopIndex];
        musicSource.Play();
        Debug.Log($"Playing music loop {currentLoopIndex + 1}");
    }

    public void PlayJumpSound()
    {
        if (sfxEnabled && jumpSFX != null)
        {
            sfxSource.PlayOneShot(jumpSFX);
        }
    }

    public void ToggleMusic(bool enabled)
    {
        musicEnabled = enabled;
        SaveAudioSettings(); // Persist setting
        if (enabled)
        {
            if (!musicSource.isPlaying)
            {
                PlayMusic();
            }
        }
        else
        {
            musicSource.Stop();
        }
        Debug.Log($"Music {(enabled ? "enabled" : "disabled")}");
    }

    public void ToggleSFX(bool enabled)
    {
        sfxEnabled = enabled;
        SaveAudioSettings(); // Persist setting
        Debug.Log($"SFX {(enabled ? "enabled" : "disabled")}");
    }

    public void PlayDiceBounce()
    {
        if (sfxEnabled && diceBounce != null && diceBounce.Length > 0)
        {
            // Pick random bounce sound
            int randomIndex = Random.Range(0, diceBounce.Length);
            sfxSource.PlayOneShot(diceBounce[randomIndex]);
        }
    }

    // --- New Methods ---

    [Header("New SFX")]
    public AudioClip winSFX;
    public AudioClip turnSwitchSFX;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    public void PlayWinSound()
    {
        if (sfxEnabled && winSFX != null) sfxSource.PlayOneShot(winSFX);
    }

    public void PlayTurnSwitchSound()
    {
        if (sfxEnabled && turnSwitchSFX != null) sfxSource.PlayOneShot(turnSwitchSFX);
    }

    public void PlayCorrectAnswer()
    {
        if (sfxEnabled && correctSFX != null) sfxSource.PlayOneShot(correctSFX);
    }

    public void PlayWrongAnswer()
    {
        if (sfxEnabled && wrongSFX != null) sfxSource.PlayOneShot(wrongSFX);
    }
}
