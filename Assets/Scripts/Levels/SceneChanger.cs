using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
  [SerializeField] private string sceneName;
#if UNITY_EDITOR
  [SerializeField] private Object sceneToLoad;
#endif
  [SerializeField] private int spawnId = 0;

  public void changeScene()
  {
    LoadWithTransition();
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.gameObject.GetComponent<Player>() != null)
    {
      LoadWithTransition();
    }
  }

  private void LoadWithTransition()
  {
    PlayerPrefs.SetInt("spawnId", spawnId);

    if (SceneFader.Instance != null)
    {
      SceneFader.Instance.FadeAndLoad(sceneName);
    }
    else
    {
      SceneManager.LoadScene(sceneName);
    }
  }
}