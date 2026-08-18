using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
  [SerializeField] private string sceneName;
  [SerializeField] private int spawnId = 0;
  [SerializeField] private AudioClip sceneMusic;

  [Header("Boss Transition")]
  [SerializeField] private bool goToBoss = false;
  [SerializeField] private int minLevelForBoss = 5;


  private AudioManager audioManager;

  void Start()
  {
    audioManager = AudioManager.Instance;
  }
  public void changeScene()
  {
    audioManager.PlayMusic(sceneMusic);
    LoadWithTransition();
  }
  public static void changeScene(int spawnId, string sceneName)
  {
    LoadWithTransition(spawnId, sceneName);
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
    if (Player.Instance != null)
    {
        Player.Instance.canMove = false;
        if (Player.Instance.rb != null)
        {
            Player.Instance.rb.linearVelocity = Vector2.zero;
        }
    }

    string targetSceneName = sceneName;

    if (goToBoss && Player.Instance != null && Player.Instance.level >= minLevelForBoss)
    {
      targetSceneName = "CollegeExteriorEnding";
    }

    PlayerPrefs.SetInt("spawnId", spawnId);

    if (SceneFader.Instance != null)
    {
      SceneFader.Instance.FadeAndLoad(targetSceneName);
    }
    else
    {
      SceneManager.LoadScene(targetSceneName);
    }
  }
  public static void LoadWithTransition(int spawnId, string sceneName)
  {
    if (Player.Instance != null)
    {
        Player.Instance.canMove = false;
        if (Player.Instance.rb != null)
        {
            Player.Instance.rb.linearVelocity = Vector2.zero;
        }
    }

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