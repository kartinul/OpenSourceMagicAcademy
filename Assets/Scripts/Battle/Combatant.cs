using UnityEngine;
using TMPro;

public class Combatant : MonoBehaviour, IInteractable
{
    [Header("Stats")]
    public string combatantName = "Combatant";
    public int maxHP = 100;
     public TextMeshProUGUI hpText;

    [HideInInspector]
    public int currentHP;

    [Header("Battle")]
    public BattleManager battleManager;

    public bool IsDead => currentHP <= 0;

    public string InteractionPrompt => "Fight";


    void Awake()
    {
        currentHP = maxHP;
    }

    public void UpdateHPText()
    {
        hpText.text = $"{currentHP} HP";
    }


    public void TakeDamage(int amount)
    {
        amount = Mathf.Max(0, amount);

        currentHP -= amount;
        currentHP = Mathf.Max(0, currentHP);

        Debug.Log($"{combatantName} took {amount} damage!");
        UpdateHPText();

    }


    public void Heal(int amount)
    {
        amount = Mathf.Max(0, amount);

        currentHP += amount;
        currentHP = Mathf.Min(maxHP, currentHP);

        Debug.Log($"{combatantName} healed {amount} HP!");
        UpdateHPText();

    }


    public void Interact()
    {
        if (battleManager == null)
        {
            Debug.LogError(
                $"{combatantName} has no BattleManager assigned!"
            );

            return;
        }

        battleManager.StartBattle(this);
    }
}