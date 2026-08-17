using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
  public enum BattleState
  {
    Start,
    PlayerTurn,
    EnemyTurn,
    ShowingMessage,
    Victory,
    Defeat
  }

  [Header("Player")]
  [Tooltip("Leave empty to auto-find by 'Player' tag")]
  [SerializeField] private Combatant player;

  [Header("UI Panels")]
  [SerializeField] private GameObject dialoguePanel;
  [SerializeField] private GameObject dialogueSpecificContainer;
  [SerializeField] private GameObject hpPanel;
  [SerializeField] private GameObject spellPanel;

  [Header("Spell UI")]
  [SerializeField] private Transform spellButtonParent;
  [SerializeField] private GameObject spellButtonPrefab;
  [SerializeField] private SpellBook spellBook;

  [Header("UI Text References")]
  [SerializeField] private TMP_Text battleText;
  [SerializeField] private TMP_Text playerNameText;
  [SerializeField] private TMP_Text playerHPText;
  [SerializeField] private TMP_Text enemyNameText;
  [SerializeField] private TMP_Text enemyHPText;

  [Header("UI Health Bar Images")]
  [SerializeField] private Image playerHPBarImage;
  [SerializeField] private Image enemyHPBarImage;

  [Header("Settings")]
  [SerializeField] private float messageDuration = 1.5f;
  [SerializeField] private float enemyTurnDelay = 0.8f;
  [SerializeField] private float textTypingSpeed = 0.03f;
  [SerializeField] private float hpBarAnimDuration = 0.4f;

  public BattleState State { get; private set; }

  private Combatant enemy;
  private EnemyAI enemyAI;
  private Player playerController;
  private PlayerInteraction playerInteraction;

  private Coroutine playerHPAnimRoutine;
  private Coroutine enemyHPAnimRoutine;
  private Coroutine activeStateRoutine;

  private WaitForSeconds typingWait;
  private WaitForSeconds lineBreakWait;
  private WaitForSeconds endWait;
  private WaitForSeconds messageWait;
  private WaitForSeconds enemyDelayWait;

  public UnityEvent OnVictory;
  public UnityEvent OnDefeat;

  private AudioManager audioManagerInstance;
  private AudioClip previousMusicClip;

  private int currentSpellSelectionIndex = 0;
  private List<SpellButton> spellButtons = new List<SpellButton>();
  private bool isUsingKeyboard = false;

  private void Awake()
  {
    typingWait = new WaitForSeconds(textTypingSpeed);
    lineBreakWait = new WaitForSeconds(messageDuration * 0.7f);
    endWait = new WaitForSeconds(3.0f);
    messageWait = new WaitForSeconds(messageDuration);
    enemyDelayWait = new WaitForSeconds(enemyTurnDelay);

    AutoSetupPlayer();
  }

  void Start()
  {
    audioManagerInstance = AudioManager.Instance;
  }


  private void OnEnable()
  {
    DialogueManager.OnDialogueFinish += DialogueManager_OnDialogueFinish;
  }
  private void OnDisable()
  {
    DialogueManager.OnDialogueFinish -= DialogueManager_OnDialogueFinish;
  }

  private void DialogueManager_OnDialogueFinish(object sender, DialogueManager.DialogueFinishArgs e)
  {
    StartBattleAfterDialogue(e.enemy);
  }

  // private void Start()
  // {
  //     EndBattle();
  // }

  private void AutoSetupPlayer()
  {
    if (player == null)
    {
      GameObject playerObj = GameObject.FindWithTag("Player");
      if (playerObj != null)
      {
        player = playerObj.GetComponent<Combatant>();
      }
    }

    if (player != null)
    {
      playerController = player.GetComponent<Player>();
      playerInteraction = player.GetComponent<PlayerInteraction>();
    }
    else
    {
      Debug.LogWarning("[BattleManager] Player reference is missing! Please assign it or tag the player GameObject as 'Player'.");
    }
  }

  private void Update()
  {
    if (State == BattleState.PlayerTurn && spellButtons.Count > 0)
    {
      bool mouseMoved = Mouse.current != null && (Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f || Mouse.current.leftButton.wasPressedThisFrame);

      if (mouseMoved)
      {
        if (isUsingKeyboard)
        {
          isUsingKeyboard = false;
          if (currentSpellSelectionIndex >= 0 && currentSpellSelectionIndex < spellButtons.Count)
          {
            spellButtons[currentSpellSelectionIndex].SetVisualInactive();
          }
        }
      }

      bool upPressed = false, downPressed = false, leftPressed = false, rightPressed = false, confirmPressed = false;

      if (Keyboard.current != null)
      {
        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) upPressed = true;
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame) downPressed = true;
        else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) leftPressed = true;
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame) rightPressed = true;

        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame) confirmPressed = true;
      }

      if (Gamepad.current != null)
      {
        if (Gamepad.current.dpad.up.wasPressedThisFrame || Gamepad.current.leftStick.up.wasPressedThisFrame) upPressed = true;
        else if (Gamepad.current.dpad.down.wasPressedThisFrame || Gamepad.current.leftStick.down.wasPressedThisFrame) downPressed = true;
        else if (Gamepad.current.dpad.left.wasPressedThisFrame || Gamepad.current.leftStick.left.wasPressedThisFrame) leftPressed = true;
        else if (Gamepad.current.dpad.right.wasPressedThisFrame || Gamepad.current.leftStick.right.wasPressedThisFrame) rightPressed = true;

        if (Gamepad.current.buttonSouth.wasPressedThisFrame) confirmPressed = true;
      }

      if (upPressed || downPressed || leftPressed || rightPressed || confirmPressed)
      {
        int nextIndex = currentSpellSelectionIndex;
        int cols = spellButtons.Count <= 2 ? 1 : 2;
        int maxIndex = spellButtons.Count - 1;
        bool directionalPressedThisFrame = false;

        if (upPressed)
        {
          if (nextIndex - cols >= 0) nextIndex -= cols;
          directionalPressedThisFrame = true;
        }
        else if (downPressed)
        {
          if (nextIndex + cols <= maxIndex) nextIndex += cols;
          directionalPressedThisFrame = true;
        }
        else if (leftPressed)
        {
          if (nextIndex % cols > 0) nextIndex -= 1;
          directionalPressedThisFrame = true;
        }
        else if (rightPressed)
        {
          if (nextIndex % cols < cols - 1 && nextIndex + 1 <= maxIndex) nextIndex += 1;
          directionalPressedThisFrame = true;
        }

        if (directionalPressedThisFrame)
        {
          if (!isUsingKeyboard)
          {
            isUsingKeyboard = true; // Still flags that we aren't using the mouse
            if (currentSpellSelectionIndex >= 0 && currentSpellSelectionIndex < spellButtons.Count)
            {
              spellButtons[currentSpellSelectionIndex].SetVisualActive();
            }
          }
          if (nextIndex != currentSpellSelectionIndex)
          {
            SetSpellSelectionIndex(nextIndex);
          }
        }

        if (isUsingKeyboard && confirmPressed)
        {
          PlayerUseSpell(spellBook.UnlockedSpells[currentSpellSelectionIndex]);
        }
      }
    }
  }

  private void SetSpellSelectionIndex(int newIndex)
  {
    if (spellButtons.Count == 0 || newIndex < 0 || newIndex >= spellButtons.Count) return;

    spellButtons[currentSpellSelectionIndex].SetVisualInactive();
    currentSpellSelectionIndex = newIndex;
    spellButtons[currentSpellSelectionIndex].SetVisualActive();
  }

  public void StartBattle(Combatant opponent)
  {
    if (opponent == null)
    {
      Debug.LogError("[BattleManager] Cannot start battle with a null opponent!");
      return;
    }

    enemy = opponent;
    enemyAI = enemy.GetComponent<EnemyAI>();

    if (audioManagerInstance == null)
      audioManagerInstance = AudioManager.Instance;

    if (audioManagerInstance != null)
    {
      Debug.Log(audioManagerInstance.name);
      previousMusicClip = audioManagerInstance.GetCurrentMusicClip();
      if (enemy.musicAudioClip)
        audioManagerInstance.PlayMusic(enemy.musicAudioClip);
    }

    enemy.battleManager = this;
    enemy.RegisterVictoryReward();

    BuildSpellButtons();

    if (playerController != null)
      playerController.canMove = false;

    if (playerInteraction != null)
      playerInteraction.canInteract = false;

    InitializeUI();

    Debug.Log($"[BattleManager] A battle has begun! {enemy.combatantName} challenges you!");

    TransitionToState(BattleState.PlayerTurn);
  }

  private void InitializeUI()
  {
    if (player != null && playerNameText != null)
      playerNameText.text = player.combatantName;

    if (enemy != null && enemyNameText != null)
      enemyNameText.text = enemy.combatantName;

    UpdateHealthUI(animate: false);
  }

  private void UpdateHealthUI(bool animate = true)
  {
    if (player != null)
    {
      float targetPct = player.maxHP > 0 ? (float)player.currentHP / player.maxHP : 0f;
      if (playerHPText != null) playerHPText.text = $"{player.currentHP} / {player.maxHP}";

      if (playerHPBarImage != null)
      {
        if (animate && gameObject.activeInHierarchy)
        {
          if (playerHPAnimRoutine != null) StopCoroutine(playerHPAnimRoutine);
          playerHPAnimRoutine = StartCoroutine(AnimateHealthBarRoutine(playerHPBarImage, targetPct));
        }
        else
        {
          playerHPBarImage.fillAmount = targetPct;
          playerHPBarImage.color = GetHPColor(targetPct);
        }
      }
    }

    if (enemy != null)
    {
      float targetPct = enemy.maxHP > 0 ? (float)enemy.currentHP / enemy.maxHP : 0f;
      if (enemyHPText != null) enemyHPText.text = $"{enemy.currentHP} / {enemy.maxHP}";

      if (enemyHPBarImage != null)
      {
        if (animate && gameObject.activeInHierarchy)
        {
          if (enemyHPAnimRoutine != null) StopCoroutine(enemyHPAnimRoutine);
          enemyHPAnimRoutine = StartCoroutine(AnimateHealthBarRoutine(enemyHPBarImage, targetPct));
        }
        else
        {
          enemyHPBarImage.fillAmount = targetPct;
          enemyHPBarImage.color = GetHPColor(targetPct);
        }
      }
    }
  }

  private IEnumerator AnimateHealthBarRoutine(Image barImage, float targetFill)
  {
    float startFill = barImage.fillAmount;
    Color startColor = barImage.color;
    Color targetColor = GetHPColor(targetFill);

    float elapsed = 0f;

    while (elapsed < hpBarAnimDuration)
    {
      elapsed += Time.deltaTime;
      float t = Mathf.Clamp01(elapsed / hpBarAnimDuration);
      float smoothT = Mathf.SmoothStep(0f, 1f, t);

      barImage.fillAmount = Mathf.Lerp(startFill, targetFill, smoothT);
      barImage.color = Color.Lerp(startColor, targetColor, smoothT);

      yield return null;
    }

    barImage.fillAmount = targetFill;
    barImage.color = targetColor;
  }

  private Color GetHPColor(float fillFraction)
  {
    fillFraction = Mathf.Clamp01(fillFraction);

    if (fillFraction > 0.5f)
    {
      float t = (fillFraction - 0.5f) * 2f;
      return Color.Lerp(Color.yellow, Color.green, t);
    }
    else
    {
      float t = fillFraction * 2f;
      return Color.Lerp(Color.red, Color.yellow, t);
    }
  }

  public void PlayerUseSpell(Spell spell)
  {
    if (State != BattleState.PlayerTurn || spell == null) return;

    if (spellButtons.Count > 0 && currentSpellSelectionIndex >= 0 && currentSpellSelectionIndex < spellButtons.Count)
    {
      spellButtons[currentSpellSelectionIndex].SetVisualInactive();
    }

    StartCoroutine(ExecuteTurnRoutine(player, enemy, spell, nextState: BattleState.EnemyTurn));
  }

  private void TransitionToState(BattleState newState)
  {
    State = newState;

    if (activeStateRoutine != null)
    {
      StopCoroutine(activeStateRoutine);
    }

    switch (State)
    {
      case BattleState.PlayerTurn:
        if (CheckBattleEnd()) return;
        SetUIVisibility(showSpells: true, showDialogue: false, showHP: true);
        if (spellButtons.Count > 0)
        {
          if (isUsingKeyboard)
          {
            SetSpellSelectionIndex(0);
          }
          else
          {
            currentSpellSelectionIndex = 0;
          }
        }
        break;

      case BattleState.EnemyTurn:
        if (CheckBattleEnd()) return;
        SetUIVisibility(showSpells: false, showDialogue: false, showHP: true);
        activeStateRoutine = StartCoroutine(ProcessEnemyTurnRoutine());
        break;

      case BattleState.ShowingMessage:
        SetUIVisibility(showSpells: false, showDialogue: true, showHP: true);
        break;

      case BattleState.Victory:
        OnVictory?.Invoke();
        SetUIVisibility(showSpells: false, showDialogue: true, showHP: true);
        activeStateRoutine = StartCoroutine(ShowEndMessageRoutine("VICTORY!"));
        break;

      case BattleState.Defeat:
        OnDefeat?.Invoke();
        SetUIVisibility(showSpells: false, showDialogue: true, showHP: true);
        activeStateRoutine = StartCoroutine(ShowEndMessageRoutine("You were defeated..."));
        break;
    }
  }

  private IEnumerator ProcessEnemyTurnRoutine()
  {
    yield return enemyDelayWait;

    Spell selectedSpell = enemyAI != null ? enemyAI.ChooseSpell() : null;

    if (selectedSpell != null)
    {
      yield return ExecuteTurnRoutine(enemy, player, selectedSpell, nextState: BattleState.PlayerTurn);
    }
    else
    {
      Debug.LogWarning($"[BattleManager] {enemy.combatantName} skipped turn (no valid spell selected).");
      TransitionToState(BattleState.PlayerTurn);
    }
  }

  private IEnumerator ExecuteTurnRoutine(Combatant caster, Combatant target, Spell spell, BattleState nextState)
  {
    TransitionToState(BattleState.ShowingMessage);

    SpellResult result = ResolveSpell(caster, target, spell);
    if (result.IsHit)
      audioManagerInstance?.PlaySFX(spell.spellAudio);

    UpdateHealthUI(animate: true);

    string message = BuildSpellMessage(caster, spell, result);

    yield return DisplayMessageRoutine(message);

    if (CheckBattleEnd()) yield break;

    TransitionToState(nextState);
  }

  private readonly struct SpellResult
  {
    public bool IsHit { get; }
    public bool IsEffective { get; }

    public SpellResult(bool isHit, bool isEffective)
    {
      IsHit = isHit;
      IsEffective = isEffective;
    }
  }

  private SpellResult ResolveSpell(Combatant caster, Combatant target, Spell spell)
  {
    Debug.Log($"{caster.combatantName} cast {spell.spellName.ToUpper()}!");

    if (UnityEngine.Random.value > spell.accuracy)
    {
      return new SpellResult(isHit: false, isEffective: false);
    }

    bool isEffective = spell.type switch
    {
      SpellType.Damage => ApplyDamageSpell(target, spell.power, caster.transform),
      SpellType.Heal => ApplyHealSpell(caster, spell.power),
      _ => HandleUnsupportedSpell(spell.type)
    };

    return new SpellResult(isHit: true, isEffective: isEffective);
  }

  private bool ApplyDamageSpell(Combatant target, int power, Transform casterTransform)
  {
    target.TakeDamage(power, casterTransform);
    return power > 0;
  }

  private bool ApplyHealSpell(Combatant caster, int power)
  {
    caster.Heal(power);
    return power > 0;
  }

  private bool HandleUnsupportedSpell(SpellType type)
  {
    Debug.LogWarning($"[BattleManager] {type} spell effect logic is not implemented.");
    return false;
  }

  private string BuildSpellMessage(Combatant caster, Spell spell, SpellResult result)
  {
    string spellName = spell.spellName.ToUpper();

    if (!result.IsHit)
    {
      return $"{caster.combatantName} cast {spellName}!\nIt missed!";
    }

    return spell.type switch
    {
      SpellType.Damage => result.IsEffective
          ? $"{caster.combatantName} cast {spellName}!\nIt was effective!"
          : $"{caster.combatantName} cast {spellName}!\nIt wasn't effective...",

      SpellType.Heal => $"{caster.combatantName} cast {spellName}!\n{caster.combatantName} recovered {spell.power} HP!",

      _ => $"{caster.combatantName} cast {spellName}!"
    };
  }

  private IEnumerator DisplayMessageRoutine(string text)
  {
    if (battleText == null) yield break;

    battleText.text = text;
    battleText.maxVisibleCharacters = 0;
    battleText.ForceMeshUpdate();

    TMP_TextInfo textInfo = battleText.textInfo;
    int totalVisibleCharacters = textInfo.characterCount;

    for (int i = 0; i < totalVisibleCharacters; i++)
    {
      battleText.maxVisibleCharacters = i + 1;

      char character = textInfo.characterInfo[i].character;
      yield return (character == '\n') ? lineBreakWait : typingWait;
    }

    yield return messageWait;
  }

  private IEnumerator ShowEndMessageRoutine(string message)
  {
    if (battleText != null)
    {
      battleText.text = message;
      battleText.maxVisibleCharacters = message.Length;
    }

    yield return endWait;

    EndBattle();
  }

  private void EndBattle()
  {
    SetUIVisibility(
        showSpells: false,
        showDialogue: false,
        showHP: false
    );

    if (playerController != null)
      playerController.canMove = true;

    if (playerInteraction != null)
      playerInteraction.canInteract = true;

    if (audioManagerInstance != null)
    {
        if (previousMusicClip != null)
        {
            if (enemy != null && enemy.musicAudioClip != null)
            {
                audioManagerInstance.PlayMusic(previousMusicClip);
            }
        }
        else
        {
            audioManagerInstance.StopMusic();
        }
    }
    
    enemy.Revive();

    if (State == BattleState.Victory && player != null)
    {
        player.Revive();
    }

    Debug.Log("[BattleManager] Battle ended.");
  }

  private bool CheckBattleEnd()
  {
    if (enemy != null && enemy.IsDead)
    {
      TransitionToState(BattleState.Victory);
      return true;
    }

    if (player != null && player.IsDead)
    {
      TransitionToState(BattleState.Defeat);
      return true;
    }

    return false;
  }

  private void SetUIVisibility(bool showSpells, bool showDialogue, bool showHP)
  {
    if (spellPanel != null) spellPanel.SetActive(showSpells);
    if (dialoguePanel != null) dialoguePanel.SetActive(showDialogue);
    if (dialogueSpecificContainer != null) dialogueSpecificContainer.SetActive(false);
    if (hpPanel != null) hpPanel.SetActive(showHP);
  }

  public void StartBattleAfterDialogue(Combatant enemy)
  {
    DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();


    if (dialogueManager != null)
    {
      dialogueManager.OnDialogueEnded.AddListener(() =>
      {
        StartBattle(enemy);
      });
    }
    else
    {
      StartBattle(enemy);
    }
  }

  private void BuildSpellButtons()
  {
    foreach (Transform child in spellButtonParent)
    {
      Destroy(child.gameObject);
    }

    spellButtons.Clear();

    foreach (Spell spell in spellBook.UnlockedSpells)
    {
      GameObject buttonObject =
          Instantiate(
              spellButtonPrefab,
              spellButtonParent
          );

      SpellButton button =
          buttonObject.GetComponent<SpellButton>();

      button.Initialize(spell, this);
      spellButtons.Add(button);
    }
  }
}