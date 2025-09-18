using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    public AudioMixer audioMixer;   // Assign your MainAudioMixer here
    private AudioSource audioSource;

    void Awake()
    {
        // Singleton pattern (only 1 music manager ever exists)
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load saved volume setting when game starts
        float volume = PlayerPrefs.GetFloat("MusicVolume", 0f);
        audioMixer.SetFloat("Volume", volume);
    }
}
