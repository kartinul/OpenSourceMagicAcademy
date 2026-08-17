using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
        if (sc != null)
            sc.changeScene();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadNextScene(string sceneName)
    {
        if (sc != null)
            sc.changeScene();
        else
            SceneManager.LoadScene(sceneName);
    }

    public void CreatePlayer()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Wizard");
        int house = PlayerPrefs.GetInt("PlayerHouse", 0);

        if (playerPrefab == null)
        {
            Debug.LogError("[GameStarter] Player prefab is not assigned!");
            return;
        }

        if (PlayerInstance != null)
        {
            Destroy(PlayerInstance.gameObject);
        }

        if (nakliPlayerPrefab != null)
            Destroy(nakliPlayerPrefab);

        GameObject playerObject = Instantiate(
            playerPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        playerObject.transform.SetParent(null);
        DontDestroyOnLoad(playerObject);

        Player player = playerObject.GetComponent<Player>();

        if (player == null)
        {
            Debug.LogError("[GameStarter] Player prefab has no Player component!");
            Destroy(playerObject);
            return;
        }

        PlayerInstance = player;

        Animator animator = player.GetComponentInChildren<Animator>();

        if (animator != null && house >= 0 && house < houseAnimators.Length)
        {
            animator.runtimeAnimatorController = houseAnimators[house];
        }

        Combatant combatant = player.GetComponent<Combatant>();

        if (combatant != null)
            combatant.combatantName = playerName;

        Debug.Log(player);
    }
}