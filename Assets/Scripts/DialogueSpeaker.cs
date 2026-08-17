using System.Collections;
using UnityEngine;

public class DialogueSpeaker : MonoBehaviour
{
  [Header("Speaking Animation")]
  [SerializeField] private Transform visual;
  [SerializeField] private float bobHeight = 0.07f;
  [SerializeField] private float bobSpeed = 15f;
  [SerializeField] private float squashAmount = 0.02f;

  private Vector3 originalPosition;
  private Vector3 originalScale;
  private Coroutine speakingCoroutine;

  private AudioManager audioManagerInstance;

  private void Awake()
  {
    if (visual == null)
      visual = GetComponentInChildren<SpriteRenderer>()?.transform;

    if (visual != null)
    {
      originalPosition = visual.localPosition;
      originalScale = visual.localScale;
    }
  }

  void Start()
  {
    audioManagerInstance = AudioManager.Instance;
  }

  public void StartSpeaking()
  {
    if (visual == null)
      return;

    if (speakingCoroutine != null)
      StopCoroutine(speakingCoroutine);

    audioManagerInstance.PlayTalkingAudio();
    speakingCoroutine = StartCoroutine(SpeakingRoutine());
  }

  public void StopSpeaking()
  {
    audioManagerInstance.StopTalkingAudio();
    if (speakingCoroutine != null)
    {
      StopCoroutine(speakingCoroutine);
      speakingCoroutine = null;
    }

    if (visual != null)
    {
      visual.localPosition = originalPosition;
      visual.localScale = originalScale;
    }
  }

  private IEnumerator SpeakingRoutine()
  {
    float time = 0f;

    while (true)
    {
      time += Time.deltaTime * bobSpeed;

      float wave = Mathf.Sin(time);

      visual.localPosition =
          originalPosition +
          Vector3.up * (wave * bobHeight);

      float squash = 1f - Mathf.Abs(wave) * squashAmount;

      visual.localScale = new Vector3(
          originalScale.x / squash,
          originalScale.y * squash,
          originalScale.z
      );

      yield return null;
    }
  }

  private void OnDisable()
  {
    StopSpeaking();
  }
}