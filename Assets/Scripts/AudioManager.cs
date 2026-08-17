using UnityEngine;

public class AudioManager : MonoBehaviour
{

  public static AudioManager Instance { get; private set; }

  [SerializeField] AudioSource musicAudioSource;
  [SerializeField] AudioSource sfxAudioSource;

  [Header("common sounds")]
  [SerializeField] AudioClip talkingClip;
  [SerializeField] float minPitch = 0.9f;
  [SerializeField] float maxPitch = 1.1f;
  [SerializeField] float talkingFadeDuration = 0.2f;

  [Header("Music Settings")]
  [SerializeField] float musicFadeDuration = 1.0f;
  private float targetMusicVolume = 1f;

  private AudioSource talkingAudioSource;
  private bool isTalking = false;
  private bool isMusicStopping = false;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    if (musicAudioSource != null)
      targetMusicVolume = musicAudioSource.volume;

    talkingAudioSource = gameObject.AddComponent<AudioSource>();
    talkingAudioSource.clip = talkingClip;
    talkingAudioSource.loop = true;
    talkingAudioSource.playOnAwake = false;
    talkingAudioSource.volume = 0f;
  }

  void Update()
  {
    if (talkingAudioSource.isPlaying)
    {
      float targetSfxVolume = sfxAudioSource.volume;
      float talkingRate = (targetSfxVolume > 0.01f ? targetSfxVolume : 1f) / (talkingFadeDuration > 0f ? talkingFadeDuration : 0.01f);

      if (isTalking)
      {
        if (talkingFadeDuration > 0f)
        {
          talkingAudioSource.volume = Mathf.MoveTowards(talkingAudioSource.volume, targetSfxVolume, talkingRate * Time.deltaTime);
        }
        else
        {
          talkingAudioSource.volume = targetSfxVolume;
        }
      }
      else
      {
        if (talkingFadeDuration > 0f)
        {
          talkingAudioSource.volume = Mathf.MoveTowards(talkingAudioSource.volume, 0f, talkingRate * Time.deltaTime);
        }
        else
        {
          talkingAudioSource.volume = 0f;
        }

        if (talkingAudioSource.volume <= 0f)
        {
          talkingAudioSource.Stop();
        }
      }
    }

    if (musicAudioSource != null && musicAudioSource.isPlaying)
    {
      if (!isMusicStopping)
      {
        if (musicFadeDuration > 0f)
        {
          musicAudioSource.volume = Mathf.MoveTowards(musicAudioSource.volume, targetMusicVolume, (1f / musicFadeDuration) * Time.deltaTime);
        }
        else
        {
          musicAudioSource.volume = targetMusicVolume;
        }
      }
      else
      {
        if (musicFadeDuration > 0f)
        {
          musicAudioSource.volume = Mathf.MoveTowards(musicAudioSource.volume, 0f, (1f / musicFadeDuration) * Time.deltaTime);
        }
        else
        {
          musicAudioSource.volume = 0f;
        }

        if (musicAudioSource.volume <= 0f)
        {
          musicAudioSource.Stop();
          isMusicStopping = false;
        }
      }
    }
  }

  public void PlayTalkingAudio()
  {
    if (!isTalking)
    {
      isTalking = true;
      if (talkingClip != null)
      {
        talkingAudioSource.pitch = Random.Range(minPitch, maxPitch);
        if (talkingClip.loadState == AudioDataLoadState.Loaded)
        {
          talkingAudioSource.time = Random.Range(0f, talkingClip.length);
        }
        talkingAudioSource.volume = 0f;
        talkingAudioSource.Play();
      }
    }
  }

  public void StopTalkingAudio()
  {
    isTalking = false;
  }

  public void PlayMusic(AudioClip clip)
  {
    if (clip == null) return;

    musicAudioSource.Stop();
    isMusicStopping = false;
    musicAudioSource.clip = clip;
    musicAudioSource.loop = true;
    musicAudioSource.volume = 0f;
    musicAudioSource.Play();
  }

  public void StopMusic()
  {
    isMusicStopping = true;
  }

  public void PlaySFX(AudioClip clip)
  {
    if (clip == null) return;

    sfxAudioSource.pitch = 1f;
    sfxAudioSource.PlayOneShot(clip);
  }

  public void SetMusicVolume(float volume)
  {
    targetMusicVolume = volume;
  }

  public void SetSFXVolume(float volume)
  {
    sfxAudioSource.volume = volume;
  }

}
