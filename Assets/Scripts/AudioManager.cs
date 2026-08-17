using UnityEngine;

public class AudioManager : MonoBehaviour
{

  public static AudioManager Instance { get; private set; }

  [SerializeField] AudioSource musicAudioSource;
  [SerializeField] AudioSource sfxAudioSource;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
  }


  public void PlayMusic(AudioClip clip)
  {
    if (clip == null) return;

    StopMusic();
    musicAudioSource.clip = clip;
    musicAudioSource.loop = true;
    musicAudioSource.Play();
  }

  public void StopMusic()
  {
    musicAudioSource.Stop();
  }

  public void PlaySFX(AudioClip clip)
  {
    if (clip == null) return;

    sfxAudioSource.PlayOneShot(clip);
  }

  public void SetMusicVolume(float volume)
  {
    musicAudioSource.volume = volume;
  }

  public void SetSFXVolume(float volume)
  {
    sfxAudioSource.volume = volume;
  }

}
