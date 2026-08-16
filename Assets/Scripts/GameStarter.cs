using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameStarter : MonoBehaviour
{
  [SerializeField] private GameObject nakliPlayerPrefab;
  [SerializeField] private GameObject playerPrefab;

  [SerializeField] private TMP_InputField nameInput;
  [SerializeField] private AnimatorOverrideController[] houseAnimators;
  private SceneChanger sc;

  public static Player PlayerInstance;


  void Start()
  {
    sc = GetComponent<SceneChanger>();
  }


  public void UpdatePlayerName(string value)
  {
    PlayerPrefs.SetString("PlayerName", value);
    PlayerPrefs.Save();
  }

  public void SelectHouse(int house)
  {
    PlayerPrefs.SetInt("PlayerHouse", house);
    PlayerPrefs.Save();
  }
  public void LoadNextScene()
  {
    if (sc)
      sc.changeScene();
    else
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
  }

  public void LoadNextScene(string sceneName)
  {
    if (sc)
      sc.changeScene();
    else
      SceneManager.LoadScene(sceneName);
  }

  public void CreatePlayer()
  {
    string playerName =
        PlayerPrefs.GetString("PlayerName", "Wizard");

    int house =
        PlayerPrefs.GetInt("PlayerHouse", 0);

    Destroy(nakliPlayerPrefab);

    GameObject playerObject =
        Instantiate(
            playerPrefab, new Vector3(0, 0, 0), Quaternion.identity
        );

    Player player = playerObject.GetComponent<Player>();
    PlayerInstance = player;

    Animator animator = player.GetComponentInChildren<Animator>();

    if (animator != null && house >= 0 && house < houseAnimators.Length)
    {
      animator.runtimeAnimatorController = houseAnimators[house];
    }

    player.GetComponent<Combatant>().combatantName = playerName;

    DontDestroyOnLoad(playerObject);

    Destroy(gameObject);
  }
}