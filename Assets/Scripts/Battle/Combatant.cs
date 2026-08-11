using UnityEngine;
using TMPro;

public class Combatant : MonoBehaviour
{
    [Header("Stats")]
    public string combatantName = "Combatant";
    public int maxHP = 100;
    public TextMeshProUGUI hpText;
    [HideInInspector]
    public int currentHP;

    public bool IsDead => currentHP <= 0;


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
}