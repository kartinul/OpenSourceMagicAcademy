using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
  [Header("Unlock")]
  [SerializeField] private DialogueData mapGrantDialogue;

  [Header("Level Maps")]
  [SerializeField] private Sprite[] levelMaps;

  [Header("UI")]
  [SerializeField] private GameObject mapOverlay;
  [SerializeField] private Image mapImage;
  [SerializeField] private GameObject lockedToast;
  [SerializeField] private TMP_Text lockedToastText;
  [SerializeField] private float lockedToastDuration = 1.5f;

  private Player player;
  private PlayerInteraction playerInteraction;
  private Coroutine lockedToastCoroutine;
  private int currentLevel = 1;
  private bool hasMap;
  private bool isOpen;
  private bool previousCanMove;
  private bool previousCanInteract;
  private float previousTimeScale = 1f;

  public bool HasMap => hasMap;
  public bool IsOpen => isOpen;

  private void Awake()
  {
    player = GetComponent<Player>();
    playerInteraction = GetComponent<PlayerInteraction>();

    if (mapOverlay != null)
      mapOverlay.SetActive(false);

    if (lockedToast != null)
      lockedToast.SetActive(false);

    if (lockedToastText != null)
      lockedToastText.text = "You haven't received the map yet.";

    UpdateLevelForScene(SceneManager.GetActiveScene().name);
    UpdateMapImage();
  }

  private void OnEnable()
  {
    DialogueManager.DialogueCompleted += OnDialogueCompleted;
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnDisable()
  {
    DialogueManager.DialogueCompleted -= OnDialogueCompleted;
    SceneManager.sceneLoaded -= OnSceneLoaded;

    if (lockedToastCoroutine != null)
    {
      StopCoroutine(lockedToastCoroutine);
      lockedToastCoroutine = null;
    }

    if (lockedToast != null)
      lockedToast.SetActive(false);

    if (isOpen)
      CloseMap();
  }

  private void Update()
  {
    if (Keyboard.current == null)
      return;

    bool mapPressed = Keyboard.current.mKey.wasPressedThisFrame;
    bool closePressed = Keyboard.current.escapeKey.wasPressedThisFrame;

    if (isOpen)
    {
      if (mapPressed || closePressed)
        CloseMap();

      return;
    }

    if (!mapPressed || !PlayerControlsAreAvailable())
      return;

    if (!hasMap)
    {
      ShowLockedToast();
      return;
    }

    OpenMap();
  }

  private bool PlayerControlsAreAvailable()
  {
    if (player == null)
      player = GetComponent<Player>();

    if (playerInteraction == null)
      playerInteraction = GetComponent<PlayerInteraction>();

    return player != null
        && player.canMove
        && (playerInteraction == null || playerInteraction.canInteract)
        && Time.timeScale > 0f;
  }

  private void OnDialogueCompleted(DialogueData dialogue)
  {
    if (dialogue != null && dialogue == mapGrantDialogue)
      hasMap = true;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    if (isOpen)
      CloseMap();

    UpdateLevelForScene(scene.name);
    UpdateMapImage();
  }

  private void UpdateLevelForScene(string sceneName)
  {
    int sceneLevel = ResolveLevel(sceneName);
    if (sceneLevel > currentLevel)
      currentLevel = sceneLevel;
  }

  private static int ResolveLevel(string sceneName)
  {
    switch (sceneName)
    {
      case "Level1":
      case "Level1_Floor1":
        return 1;
      case "Level2":
        return 2;
      case "Level3":
      case "Level3_Floor1":
        return 3;
      case "Level4":
        return 4;
      default:
        return 0;
    }
  }

  private void UpdateMapImage()
  {
    if (mapImage == null || levelMaps == null || levelMaps.Length == 0)
      return;

    int mapIndex = Mathf.Clamp(currentLevel - 1, 0, levelMaps.Length - 1);
    mapImage.sprite = levelMaps[mapIndex];
    mapImage.preserveAspect = true;
  }

  private void OpenMap()
  {
    if (mapOverlay == null || mapImage == null)
    {
      Debug.LogError("[MapController] Map UI references are not assigned.");
      return;
    }

    UpdateMapImage();

    previousTimeScale = Time.timeScale;
    previousCanMove = player != null && player.canMove;
    previousCanInteract = playerInteraction == null || playerInteraction.canInteract;

    if (player != null)
      player.canMove = false;

    if (playerInteraction != null)
      playerInteraction.canInteract = false;

    isOpen = true;
    mapOverlay.SetActive(true);
    Time.timeScale = 0f;
  }

  private void CloseMap()
  {
    if (!isOpen)
      return;

    isOpen = false;

    if (mapOverlay != null)
      mapOverlay.SetActive(false);

    Time.timeScale = previousTimeScale;

    if (player != null)
      player.canMove = previousCanMove;

    if (playerInteraction != null)
      playerInteraction.canInteract = previousCanInteract;
  }

  private void ShowLockedToast()
  {
    if (lockedToast == null)
      return;

    if (lockedToastCoroutine != null)
      StopCoroutine(lockedToastCoroutine);

    lockedToastCoroutine = StartCoroutine(ShowLockedToastRoutine());
  }

  private IEnumerator ShowLockedToastRoutine()
  {
    lockedToast.SetActive(true);
    yield return new WaitForSecondsRealtime(lockedToastDuration);
    lockedToast.SetActive(false);
    lockedToastCoroutine = null;
  }
}
