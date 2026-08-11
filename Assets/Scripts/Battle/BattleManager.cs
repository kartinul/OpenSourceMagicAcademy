using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        Start,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }

    [Header("Combatants")]
    public Combatant player;
    public Combatant enemy;

    [Header("Battle State")]
    public BattleState state;

    void Start()
    {
        StartBattle();
    }

    void StartBattle()
    {
        state = BattleState.Start;

        Debug.Log($"A battle has begun! {enemy.combatantName} challenges you!");

        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        if (CheckBattleEnd())
            return;

        state = BattleState.PlayerTurn;

        // Debug.Log("Your turn");
    }

    public void PlayerUseSpell(Spell spell)
    {
        if (state != BattleState.PlayerTurn)
            return;

        StartCoroutine(PlayerSpellRoutine(spell));
    }

    IEnumerator PlayerSpellRoutine(Spell spell)
    {
        state = BattleState.EnemyTurn;
        ResolveSpell(player, enemy, spell);

        yield return new WaitForSeconds(1f);

        if (CheckBattleEnd())
            yield break;

        StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        state = BattleState.EnemyTurn;

        Debug.Log("Enemy's turn.");

        // temporary enemy AI
        Spell enemySpell = enemy.GetComponent<EnemyAI>().ChooseSpell();

        if (enemySpell != null) ResolveSpell(enemy, player, enemySpell);


        if (CheckBattleEnd())
            return;

        StartPlayerTurn();
    }

    void ResolveSpell(
        Combatant caster,
        Combatant target,
        Spell spell
    )
    {
        Debug.Log(
            $"{caster.combatantName} used {spell.spellName}!"
        );

        if (Random.value > spell.accuracy)
        {
            Debug.Log("The spell missed!");
            return;
        }

        switch (spell.type)
        {
            case SpellType.Damage:
                target.TakeDamage(spell.power);
                break;

            case SpellType.Heal:
                caster.Heal(spell.power);
                break;

            case SpellType.Disable:
                Debug.Log("disable effect not implemented yet.");
                break;

            case SpellType.Special:
                Debug.Log("special spell not implemented yet.");
                break;
        }
    }

    void StartEnemyTurnDummy()
    {
        StartEnemyTurn();
    }

    bool CheckBattleEnd()
    {
        if (enemy.IsDead) {
            state = BattleState.Victory;
            Debug.Log("VICTORY!");

            return true;
        }

        if (player.IsDead) {
            state = BattleState.Defeat;
            Debug.Log("DEFEAT!");

            return true;
        }

        return false;
    }
}