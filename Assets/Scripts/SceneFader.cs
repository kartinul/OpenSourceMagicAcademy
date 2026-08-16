using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
  public static SceneFader Instance { get; private set; }

  [SerializeField] private Image fadeImage;
  [SerializeField] private float fadeDuration = 0.5f;

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

  public void FadeAndLoad(string sceneName)
  {
    StartCoroutine(FadeAndLoadRoutine(sceneName));
  }

  private IEnumerator FadeAndLoadRoutine(string sceneName)
  {
    yield return StartCoroutine(Fade(0f, 1f));

    AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
    op.allowSceneActivation = false;

    while (op.progress < 0.9f)
    {
      yield return null;
    }

    op.allowSceneActivation = true;

    while (!op.isDone)
    {
      yield return null;
    }

    yield return StartCoroutine(Fade(1f, 0f));
  }

  private IEnumerator Fade(float from, float to)
  {
    float t = 0f;
    Color c = fadeImage.color;

    while (t < fadeDuration)
    {
      t += Time.deltaTime;
      float a = Mathf.Lerp(from, to, t / fadeDuration);
      fadeImage.color = new Color(c.r, c.g, c.b, a);
      yield return null;
    }

    fadeImage.color = new Color(c.r, c.g, c.b, to);
  }
}