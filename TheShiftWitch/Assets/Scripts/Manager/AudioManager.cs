using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioManager : MonoBehaviour
{

    public static event UnityAction saveSettings;
    [Space]
    [SerializeField] private AudioClip swing;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip bang;
    [SerializeField] private AudioClip rock;
    [SerializeField] private AudioClip slimeHit;
    [SerializeField] private AudioClip hit;
    [SerializeField] private AudioClip hitShield;
    [SerializeField] private AudioClip pause;
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip buttonClick;


    [Header("Zend Sounds")]
    [SerializeField] private AudioClip landingSound;
    [SerializeField] private AudioClip drawSword;
    [SerializeField] private AudioClip dash;
    [SerializeField] private AudioClip doubleJump;
    [Header("SoundTracks")]
    [SerializeField] private List<MusicThemeGroup> themeGroups;
    [SerializeField] private AudioClip soundTracks;
    [SerializeField] private AudioClip titleScreen;
    [SerializeField] private AudioClip houseMusic;
    [SerializeField] private AudioClip level1;
    [SerializeField] private AudioClip wormDiving;
    [SerializeField] private AudioClip death;
    [SerializeField] private AudioClip mysteriousHarmonies;
    [SerializeField] private AudioClip battleTheme;
    [Header("UI Sfx")]
    [SerializeField] private GameObject onClick;
    [SerializeField] private GameObject onSelect;

    [Header("Objects")]
    private AudioSource masterAudio;
    [SerializeField] private AudioSource sfxAudio;
    [SerializeField] private AudioSource musicAudio;
    private static AudioManager instance;

    private float masterVolume;
    private float sfxVolume;
    private float musicVolume;
    [Tooltip("The prefab that contains only an AudioSource component.")]
    [SerializeField] GameObject audioSourcePrefab;

    [Tooltip("How many audio sources to create initially.")]
    [SerializeField] int poolSize = 10;

    // The list that holds all our reusable audio sources
    private List<AudioSource> audioPool;
    public AudioClip Swing { get => swing; set => swing = value; }
    public AudioClip Bang { get => bang; set => bang = value; }
    public AudioClip Rock { get => rock; set => rock = value; }
    public AudioClip SlimeHit { get => slimeHit; set => slimeHit = value; }
    public AudioClip LandingSound { get => landingSound; set => landingSound = value; }
    public AudioClip Jump { get => jump; set => jump = value; }
    public AudioClip DrawSword { get => drawSword; set => drawSword = value; }
    public AudioClip Dash { get => dash; set => dash = value; }
    public AudioClip DoubleJump { get => doubleJump; set => doubleJump = value; }
    public AudioClip Hit { get => hit; set => hit = value; }
    public AudioClip HitShield { get => hitShield; set => hitShield = value; }
    public float MasterVolume { get => masterVolume; set { masterVolume = value; masterAudio.volume = masterVolume; saveSettings?.Invoke(); } }
    public float SfxVolume { get => sfxVolume; set { sfxVolume = value; sfxAudio.volume = sfxVolume; saveSettings?.Invoke(); } }
    public float MusicVolume { get => musicVolume; set { musicVolume = value; musicAudio.volume = value; saveSettings?.Invoke(); } }
    public AudioSource MusicAudio { get => musicAudio; set => musicAudio = value; }
    public AudioSource SfxAudio { get => sfxAudio; set => sfxAudio = value; }


    public static AudioManager GetAudio() => instance;
    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            DontDestroyOnLoad(gameObject);

        }
        else {
            instance = this;
        }
        masterAudio = GetComponent<AudioSource>();
        //GameController.onoLevelLoaded+=
        InitializePool();
    }
    private void OnEnable() {
       // Enemy.sendsfx += RetrieveSfx;
        UIButton.click += OnClick;

    }
    private void OnDisable() {
        //Enemy.sendsfx -= RetrieveSfx;
        UIButton.click -= OnClick;
    }
    // Start is called before the first frame update
    void Start() {



        MasterVolume = 0.3f;
    }
    private void InitializePool() {
        audioPool = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++) {
            // 1. Instantiate the prefab (which has the AudioSource)
            GameObject soundObject = Instantiate(audioSourcePrefab, transform);
            AudioSource source = soundObject.GetComponent<AudioSource>();

            // 2. Disable the GameObject (mark it as available)
            soundObject.SetActive(false);

            // 3. Add it to the list
            audioPool.Add(source);
        }
    }
    //readonly Vector3 pos = Vector3.zero;
    public void PlayOneShot(AudioClip clip, Vector3 position, float pitch = 1, float volume = 1f) {
        if (clip == null) return;

        // --- Step 1: Find the first available source ---
        AudioSource availableSource = FindAvailableSource();

        if (availableSource != null) {
            // --- Step 2: Configure and Activate ---

            // Set position (if you want positional 3D audio)
            availableSource.transform.position = position;

            //Set pitch
            availableSource.pitch = pitch;

            // Set clip and volume
            availableSource.clip = clip;
            availableSource.volume = volume;

            // Mark it as active/in-use
            availableSource.gameObject.SetActive(true);

            // --- Step 3: Play and Schedule Return to Pool ---
            availableSource.Play();

            // Start the coroutine to return the source to the pool after it's done playing
            StartCoroutine(ReturnToPoolAfterDelay(availableSource, clip.length));
        }
        else {
            // Optional: If pool is full, you can log a warning or dynamically expand the pool.
            Debug.LogWarning("Audio pool is exhausted. Consider increasing the pool size.");
        }
    }
    private IEnumerator ReturnToPoolAfterDelay(AudioSource source, float delay) {
        // Wait for the clip's duration to finish playing
        yield return new WaitForSeconds(delay);

        // --- Step 4: Return to Pool ---

        // Ensure it hasn't been reused in the interim (unlikely, but safe)
        if (source != null && source.gameObject.activeSelf) {
            source.gameObject.SetActive(false);
        }
    }
    private AudioSource FindAvailableSource() {
        // The core "find the first available" logic
        foreach (AudioSource source in audioPool) {
            if (!source.gameObject.activeSelf) {
                return source;
            }
        }
        // If no sources are available
        return null;
    }
    private void FadeOutVolume() {
        masterAudio.volume -= 0.1f;

    }

    private IEnumerator FadeOutCoroutine(int val) {
        YieldInstruction wait = new WaitForSeconds(0.3f);
        while (isActiveAndEnabled && masterAudio.volume > 0f) {
            yield return null;
            FadeOutVolume();
        }
        StartCoroutine(FadeOut(val));
    }
    private IEnumerator FadeOut(int val) {
        yield return new WaitUntil(() => masterAudio.volume == 0);
        BackGroundMusicManager(val);

    }
    private void Fade(int val) {
        StartCoroutine(FadeOutCoroutine(val));
        //BackGroundMusicManager(val);
    }

    // Call this method at the start of a level to play the right theme music
    public void PlayLevelMusic(int levelNumber) {
        // Determine the theme index (e.g., levels 1-10 = index 0, 11-20 = index 1)
        // Level 1-10: (1-1)/10 = 0. Level 10: (10-1)/10 = 0. Level 11: (11-1)/10 = 1.
        print(levelNumber);
        if (levelNumber <= 1) {
            return;
        }
        int themeIndex = (levelNumber - 1) / 10;

        // Safety check to ensure we don't go out of bounds of our defined themes
        if (themeGroups == null || themeGroups.Count == 0) return;

        if (themeIndex >= themeGroups.Count) {
            themeIndex = themeGroups.Count - 1; // Fallback to the last theme if player goes past your max themes
        }

        MusicThemeGroup currentTheme = themeGroups[themeIndex];

        if (currentTheme.tracks != null && currentTheme.tracks.Count > 0) {
            // Pick a random song from this theme's pool
            int randomTrackIndex = Random.Range(0, currentTheme.tracks.Count);
            AudioClip selectedTrack = currentTheme.tracks[randomTrackIndex];

            // Use your existing fade logic to transition smoothly
            StartCoroutine(FadeToNewTrack(selectedTrack));
        }
    }

    // A modified version of your fade logic to accept a specific AudioClip
    private IEnumerator FadeToNewTrack(AudioClip newClip) {
        // Fade out
        while (isActiveAndEnabled && musicAudio.volume > 0f) {
            musicAudio.volume -= 0.1f;
            yield return new WaitForSeconds(0.05f); // Faster fade than 0.3s for responsive level transitions
        }

        musicAudio.clip = newClip;
        musicAudio.volume = MasterVolume;

        if (newClip != null) {
            musicAudio.Play();
        }
        print("soundtrack played");
    }

    private void BackGroundMusicManager(int trackNumber) {

        switch (trackNumber) {
            case 0:
                masterAudio.clip = null;
                break;
            case 1:
                masterAudio.clip = titleScreen;
                break;
            case 2:
                masterAudio.clip = death;
                break;
            case 8:
                masterAudio.clip = battleTheme;
                break;
            default:
                masterAudio.clip = wormDiving;
                break;
        }
        masterAudio.volume = MasterVolume;
        masterAudio.Play();
    }
    private void RetrieveSfx(AudioClip sound) {
        SfxAudio.PlayOneShot(sound);
    }
    public void OnClick() {
        PlayOneShot(click, transform.position);
        //sfxAudio.PlayOneShot(buttonClick);
    }
    public void OnSelected() {
        Instantiate(onSelect);
        //sfxAudio.PlayOneShot(click);
    }
    private void Pause() {
        PlayOneShot(pause,transform.position);
    }

}
[System.Serializable]
public class MusicThemeGroup
{
    public string themeName; // e.g., "Levels 1-10: Floating Islands"
    public List<AudioClip> tracks; // The pool of songs for this theme
}

