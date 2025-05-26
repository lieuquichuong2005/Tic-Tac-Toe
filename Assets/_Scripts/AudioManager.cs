using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // Singleton instance

    public AudioSource musicAudioSource; // Reference to the AudioSource component
    public AudioSource sfxAudioSource; // Reference to the AudioSource component for sound effects

    public AudioClip backgroundMusic_01;
    public AudioClip backgroundMusic_02;
    public AudioClip clickSound;
    public AudioClip winSound;

    private void Start()
    {
        PlayerPrefs.SetFloat("MusicVolume", 0.5f); // Set default music volume
        PlayerPrefs.SetFloat("SFXVolume", 0.5f); // Set default SFX volume

        // Check if instance already exists and destroy duplicate
        if (instance == null)
        {
            instance = this; // Set the instance to this object
            DontDestroyOnLoad(gameObject); // Don't destroy this object when loading new scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instance
        }
        // Play the background music at the start of the game
        PlayMusic(backgroundMusic_01);
    }
    public void PlayMusic(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            musicAudioSource.clip = audioClip;
            musicAudioSource.loop = true; // Loop the music
            musicAudioSource.Play();
        }
    }    
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            sfxAudioSource.PlayOneShot(clickSound);
        }
    }
    public void PlayWinSound()
    {
        if (winSound != null)
        {
            sfxAudioSource.PlayOneShot(winSound);
        }
    }
    public void SetMusicVolume(float volume)
    {
        musicAudioSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume); // Save the music volume
    }
    public void SetSFXVolume(float volume)
    {
        sfxAudioSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume); // Save the SFX volume
    }
}
