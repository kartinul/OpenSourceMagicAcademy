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
    [SerializeField] private float textTypingSpeed = 0.03f; // Delay per character (in seconds)

    public BattleState State { get; private set; }

    private EnemyAI enemyAI;

    

    private void Awake()
    {
        if (enemy != null)
        {
            enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI == null)
            {
                Debug.LogError($"[BattleManager] {enemy.name} is missing an EnemyAI component!", this);
            }
        }
    }

    private void Start()
    {
        StartBattle();
    }

    public void StartBattle()
    {
        SetState(BattleState.Start);
        Debug.Log($"A battle has begun! {enemy.combatantName} challenges you!");
        
        SetState(BattleState.PlayerTurn);
    }

    private void SetState(BattleState newState)
    {
        State = newState;

        switch (State)
        {
            case BattleState.PlayerTurn:
                if (CheckBattleEnd()) return;
                SetUIVisibility(showPlayerUI: true, showTextUI: false);
                break;

            case BattleState.EnemyTurn:
                if (CheckBattleEnd()) return;
                SetUIVisibility(showPlayerUI: false, showTextUI: false);
                StartCoroutine(ProcessEnemyTurnRoutine());
                break;

            case BattleState.ShowingMessage:
                SetUIVisibility(showPlayerUI: false, showTextUI: true);
                break;

            case BattleState.Victory:
                SetUIVisibility(showPlayerUI: false, showTextUI: true);
                StartCoroutine(ShowEndMessageRoutine("VICTORY!"));
                break;

            case BattleState.Defeat:
                SetUIVisibility(showPlayerUI: false, showTextUI: true);
                StartCoroutine(ShowEndMessageRoutine("DEFEAT..."));
                break;
        }
    }


    public void PlayerUseSpell(Spell spell)
    {
        if (State != BattleState.PlayerTurn || spell == null) return;

        StartCoroutine(ExecuteTurnRoutine(player, enemy, spell, NextState: BattleState.EnemyTurn));
    }

    private IEnumerator ProcessEnemyTurnRoutine()
    {
        yield return new WaitForSeconds(enemyTurnDelay);

        Spell selectedSpell = enemyAI != null ? enemyAI.ChooseSpell() : null;

        if (selectedSpell != null)
        {
            yield return ExecuteTurnRoutine(enemy, player, selectedSpell, NextState: BattleState.PlayerTurn);
        }
        else
        {
            Debug.LogWarning($"[BattleManager] {enemy.combatantName} skipped turn (no spell selected).");
            SetState(BattleState.PlayerTurn);
        }
    }

    private IEnumerator ExecuteTurnRoutine(Combatant caster, Combatant target, Spell spell, BattleState NextState)
    {
        SetState(BattleState.ShowingMessage);

        SpellResult result = ResolveSpell(caster, target, spell);
        string message = BuildSpellMessage(caster, spell, result);

        yield return DisplayMessageRoutine(message);

        if (CheckBattleEnd()) yield break;

        SetState(NextState);
    }

    private struct SpellResult
    {
        public bool IsHit;
        public bool IsEffective;
    }

    private SpellResult ResolveSpell(Combatant caster, Combatant target, Spell spell)
    {
        Debug.Log($"{caster.combatantName} cast {spell.spellName.ToUpper()}!");

        // Accuracy Check
        if (UnityEngine.Random.value > spell.accuracy)
        {
            Debug.Log("The spell missed!");
            return new SpellResult { IsHit = false, IsEffective = false };
        }

        bool effective = false;

        switch (spell.type)
        {
            case SpellType.Damage:
                target.TakeDamage(spell.power);
                effective = spell.power > 0;
                break;

            case SpellType.Heal:
                caster.Heal(spell.power);
                effective = spell.power > 0;
                break;

            case SpellType.Disable:
            case SpellType.Special:
                Debug.LogWarning($"[BattleManager] {spell.type} spell effect is not implemented yet.");
                effective = false;
                break;
        }

        return new SpellResult { IsHit = true, IsEffective = effective };
    }


    private void SetUIVisibility(bool showPlayerUI, bool showTextUI)
    {
        if (leftPanel != null) leftPanel.SetActive(showPlayerUI);
        if (rightPanel != null) rightPanel.SetActive(showPlayerUI);
        if (textPanel != null) textPanel.SetActive(showTextUI);
    }

    private string BuildSpellMessage(Combatant caster, Spell spell, SpellResult result)
    {
        string spellName = spell.spellName.ToUpper();
        if (!result.IsHit)
        {
            return $"{caster.combatantName} cast {spellName}!\nIt missed!";
        }

        switch (spell.type)
        {
            case SpellType.Damage:
                return result.IsEffective
                    ? $"{caster.combatantName} cast {spellName}!\nIt was effective!"
                    : $"{caster.combatantName} cast {spellName}!\nIt wasn't effective...";

            case SpellType.Heal:
                return $"{caster.combatantName} cast {spellName}!\n{caster.combatantName} recovered {spell.power} HP!";

            default:
                return $"{caster.combatantName} cast {spellName}!";
        }
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
            if (character == '\n')
            {
                yield return new WaitForSeconds(messageDuration * 0.7f);
            }
            else
            {
                yield return new WaitForSeconds(textTypingSpeed);
            }
        }

        yield return new WaitForSeconds(messageDuration);
    }
    private IEnumerator ShowEndMessageRoutine(string message)
    {
        if (battleText != null) battleText.text = message;
        yield return new WaitForSeconds(3.0f);
    }

    private bool CheckBattleEnd()
    {
        if (enemy != null && enemy.IsDead)
        {
            SetState(BattleState.Victory);
            return true;
        }

        if (player != null && player.IsDead)
        {
            SetState(BattleState.Defeat);
            return true;
        }

        return false;
    }
}