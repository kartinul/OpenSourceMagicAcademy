using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollegeBossSequence : MonoBehaviour
{
  [Header("Core Dependencies")]
  private DialogueManager dialogueManager;
  private BattleManager battleManager;

  [Header("Scene Objects")]
  [SerializeField] private GameObject dumbledore;
  [SerializeField] private GameObject coldemort;
  [SerializeField] private GameObject statueNPC;

  [Header("Player Transforms")]
  [SerializeField] private Transform statuePlayerPosition;
  [SerializeField] private Transform fightPlayerPosition;

  [Header("Dialogue Assets")]
  [SerializeField] private DialogueData dumbledoreDeathAsset;
  [SerializeField] private DialogueData spiritOfTorvaldsAsset;

  [Header("Spells")]
  [SerializeField] private Spell finalSpellAsset;

  [Header("Scene Transition")]
  [SerializeField] private string nextSceneName = "WinScene";

  private float bossCameraZoom = 17.5f;
  private float bossCameraXOffset = 2f;
  private float bossCameraYOffset = -4f;

  private float secondFightZoom = 21.6f;
  private float secondFightXOffset = 2f;
  private float secondFightYOffset = -6.26f;

  private bool battleEnded = false;
  private bool battleWon = false;
  private bool isSecondBattle = false;
  private GameObject bossCameraTarget;

  private void Start()
  {
    dialogueManager = Player.Instance.GetComponentInChildren<DialogueManager>(true);
    battleManager = Player.Instance.GetComponentInChildren<BattleManager>(true);

    if (battleManager != null)
    {
      battleManager.OnVictory.AddListener(OnBattleVictory);
      battleManager.OnDefeat.AddListener(OnBattleDefeat);
      battleManager.bypassSceneReloadOnDefeat = true;
    }

    bossCameraTarget = new GameObject("BossCameraTarget");
    bossCameraTarget.transform.SetParent(coldemort.transform);
    bossCameraTarget.transform.localPosition = new Vector3(bossCameraXOffset, bossCameraYOffset, 0);

    StartCoroutine(RunSequence());
  }

  private void OnDestroy()
  {
    if (battleManager != null)
    {
      battleManager.OnVictory.RemoveListener(OnBattleVictory);
      battleManager.OnDefeat.RemoveListener(OnBattleDefeat);
      battleManager.bypassSceneReloadOnDefeat = false;
    }
    if (bossCameraTarget != null) Destroy(bossCameraTarget);
  }

  private void OnBattleVictory()
  {
    battleEnded = true;
    battleWon = true;
  }

  private void OnBattleDefeat()
  {
    battleEnded = true;
    battleWon = false;
  }

  private IEnumerator RunSequence()
  {
    // 1. Coldemort Dialog
    bossCameraTarget.transform.localPosition = new Vector3(bossCameraXOffset, bossCameraYOffset, 0);
    EventHelpers.FocusCameraOnWithZoom(bossCameraTarget, bossCameraZoom);
    yield return PlayDialogueAndWait(CreateDialogue("Coldemort",
        "This is the end for you. I have defeated your master."
    ));

    // 2. Pan to Dumbledore
    EventHelpers.FocusCameraOn(dumbledore);
    if (dumbledoreDeathAsset != null)
    {
      yield return PlayDialogueAndWait(dumbledoreDeathAsset);
    }
    else
    {
      Debug.LogWarning("DumbledoreDeath asset is missing!");
    }

    // 3. Back to boss & enter first fight
    bossCameraTarget.transform.localPosition = new Vector3(bossCameraXOffset, bossCameraYOffset, 0);
    EventHelpers.FocusCameraOnWithZoom(bossCameraTarget, bossCameraZoom);
    yield return new WaitForSeconds(0.5f);

    Combatant coldemortCombatant = coldemort.GetComponentInChildren<Combatant>(true);
    Combatant playerCombatant = Player.Instance.GetComponentInChildren<Combatant>(true);
    EnemyAI coldemortAI = coldemort.GetComponentInChildren<EnemyAI>(true);

    if (coldemortAI != null)
    {
      for (int i = 0; i < coldemortAI.spells.Length; i++)
      {
        if (coldemortAI.spells[i] != null)
        {
          coldemortAI.spells[i] = Instantiate(coldemortAI.spells[i]);
          coldemortAI.spells[i].power = Mathf.RoundToInt(coldemortAI.spells[i].power * 1.5f);
        }
      }
    }

    if (coldemortCombatant != null && battleManager != null && playerCombatant != null)
    {
      battleEnded = false;
      isSecondBattle = false;
      battleManager.StartBattle(coldemortCombatant);

      // Wait for battle to finish
      yield return new WaitUntil(() => battleEnded);

      // Wait an additional 3.5 seconds because BattleManager takes 3 seconds in ShowEndMessageRoutine before EndBattle()
      yield return new WaitForSeconds(3.5f);

      // The user assumes you lose the first time.
      if (!battleWon)
      {
        // 4. After you lose, go to FIGURE (statue), play SpiritOfTorvalds.asset
        EventHelpers.FocusCameraOn(statueNPC);

        if (statuePlayerPosition != null && Player.Instance != null)
        {
          Player.Instance.transform.position = statuePlayerPosition.position;
        }

        if (spiritOfTorvaldsAsset != null)
        {
          yield return PlayDialogueAndWait(spiritOfTorvaldsAsset);
        }
        else
        {
          Debug.LogWarning("SpiritOfTorvalds asset is missing!");
        }

        // Unlock final spell (rm -rf) after receiving it from Torvalds
        UnlockFinalSpell();

        SpellBook spellBook = FindFirstObjectByType<SpellBook>();
        if (spellBook != null && finalSpellAsset != null)
        {
          yield return null; // wait a frame for it to open
          yield return new WaitUntil(() => !spellBook.IsUnlockPanelOpen);
        }

        // 5. Go back to fight scene (second battle)
        if (fightPlayerPosition != null && Player.Instance != null)
        {
          Player.Instance.transform.position = fightPlayerPosition.position;
        }

        bossCameraTarget.transform.localPosition = new Vector3(secondFightXOffset, secondFightYOffset, 0);
        EventHelpers.FocusCameraOnWithZoom(bossCameraTarget, secondFightZoom);

        yield return new WaitForSeconds(0.5f);

        battleEnded = false;
        isSecondBattle = true;

        // Revive player manually since they lost the last one
        playerCombatant.Revive();

        battleManager.StartBattle(coldemortCombatant);

        yield return new WaitUntil(() => battleEnded);

        if (battleWon)
        {
          // 6. If you win, pan back to statue and YAP
          EventHelpers.FocusCameraOn(statueNPC);
          yield return PlayDialogueAndWait(CreateDialogue("Spirit of Torvalds",
              "You see, young wizard? This is the power of open source.",
              "When we share our code, our knowledge, our magic...",
              "We become stronger than any single proprietary force.",
              "The community stands with you."
          ));

          // Sequence end
          EventHelpers.ClearCameraFocus();
          Debug.Log("College Boss Sequence Completed.");

          yield return new WaitForSeconds(0.5f);

          SceneChanger.changeScene(0, nextSceneName);
        }
        else
        {
          // 7. If you lose again, reload the entire scene
          Debug.Log("Player lost the second battle, reloading scene...");
          SceneChanger.changeScene(0, SceneManager.GetActiveScene().name);
        }
      }
      else
      {
        // Edge case: if they somehow won the unwinnable fight
        Debug.LogWarning("Player won the unwinnable first fight! Continuing sequence anyway?");
        EventHelpers.ClearCameraFocus();
      }
    }
    else
    {
      Debug.LogError("Missing Combatant script or BattleManager!");
    }
  }

  private DialogueData CreateDialogue(string speaker, params string[] lines)
  {
    DialogueData data = ScriptableObject.CreateInstance<DialogueData>();
    data.speakerName = speaker;
    data.lines = lines;
    return data;
  }

  private IEnumerator PlayDialogueAndWait(DialogueData data)
  {
    if (data == null)
    {
      yield break;
    }

    bool isFinished = false;

    UnityEngine.Events.UnityAction onEnd = null;
    onEnd = () =>
    {
      isFinished = true;
      dialogueManager.OnDialogueEnded.RemoveListener(onEnd);
    };

    dialogueManager.OnDialogueEnded.AddListener(onEnd);
    dialogueManager.StartDialogue(data);

    yield return new WaitUntil(() => isFinished);
  }

  public void UnlockFinalSpell()
  {
    if (finalSpellAsset == null)
    {
      Debug.LogWarning("Final spell asset is missing!");
      return;
    }

    SpellBook spellBook = FindFirstObjectByType<SpellBook>();

    if (spellBook != null)
      spellBook.UnlockSpell(finalSpellAsset, true);
  }

  [ContextMenu("Skip To Next Scene")]
  public void SkipToNextScene()
  {
    if (!string.IsNullOrEmpty(nextSceneName))
    {
      SceneChanger.changeScene(0, nextSceneName);
    }
  }
}
