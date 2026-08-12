using System;
using System.Collections;
using UnityEngine;
using TMPro;

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

    [Header("Combatants")]
    [SerializeField] private Combatant player;
    [SerializeField] private Combatant enemy;

    [Header("UI Panels")]
    [SerializeField] private GameObject textPanel;
    [SerializeField] private GameObject leftPanel;
    [SerializeField] private GameObject rightPanel;
    [SerializeField] private TMP_Text battleText;

    [Header("Settings")]
    [SerializeField] private float messageDuration = 1.5f;
    [SerializeField] private float enemyTurnDelay = 0.8f;
    [SerializeField] private float textTypingSpeed = 0.03f;

    public BattleState State { get; private set; }

    private EnemyAI enemyAI;
    private Coroutine activeStateRoutine;

    private WaitForSeconds typingWait;
    private WaitForSeconds lineBreakWait;
    private WaitForSeconds endWait;
    private WaitForSeconds messageWait;
    private WaitForSeconds enemyDelayWait;

    private void Awake()
    {
        typingWait = new WaitForSeconds(textTypingSpeed);
        lineBreakWait = new WaitForSeconds(messageDuration * 0.7f);
        endWait = new WaitForSeconds(3.0f);
        messageWait = new WaitForSeconds(messageDuration);
        enemyDelayWait = new WaitForSeconds(enemyTurnDelay);
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

        Debug.Log($"[BattleManager] A battle has begun! {enemy.combatantName} challenges you!");

        TransitionToState(BattleState.PlayerTurn);
    }

    public void PlayerUseSpell(Spell spell)
    {
        if (State != BattleState.PlayerTurn || spell == null) return;

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
                SetUIVisibility(showPlayerUI: true, showTextUI: false);
                break;

            case BattleState.EnemyTurn:
                if (CheckBattleEnd()) return;
                SetUIVisibility(showPlayerUI: false, showTextUI: false);
                activeStateRoutine = StartCoroutine(ProcessEnemyTurnRoutine());
                break;

            case BattleState.ShowingMessage:
                SetUIVisibility(showPlayerUI: false, showTextUI: true);
                break;

            case BattleState.Victory:
                SetUIVisibility(showPlayerUI: false, showTextUI: true);
                activeStateRoutine = StartCoroutine(ShowEndMessageRoutine("VICTORY!"));
                break;

            case BattleState.Defeat:
                SetUIVisibility(showPlayerUI: false, showTextUI: true);
                activeStateRoutine = StartCoroutine(ShowEndMessageRoutine("DEFEAT..."));
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
            SpellType.Damage => ApplyDamageSpell(target, spell.power),
            SpellType.Heal   => ApplyHealSpell(caster, spell.power),
            _ => HandleUnsupportedSpell(spell.type)
        };

        return new SpellResult(isHit: true, isEffective: isEffective);
    }

    private bool ApplyDamageSpell(Combatant target, int power)
    {
        target.TakeDamage(power);
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

    private void SetUIVisibility(bool showPlayerUI, bool showTextUI)
    {
        if (leftPanel != null) leftPanel.SetActive(showPlayerUI);
        if (rightPanel != null) rightPanel.SetActive(showPlayerUI);
        if (textPanel != null) textPanel.SetActive(showTextUI);
    }
}