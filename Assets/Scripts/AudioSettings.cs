using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer audioMixer;  // Drag MainAudioMixer here in Inspector
    public Slider volumeSlider;    // Drag the slider here

    void Start()
    {
        float volume = PlayerPrefs.GetFloat("MusicVolume", 0f);
        audioMixer.SetFloat("Volume", volume);
        volumeSlider.value = volume;

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume); // Save setting
    }
}
