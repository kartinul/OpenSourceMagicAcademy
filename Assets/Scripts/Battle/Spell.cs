using UnityEngine;

public enum SpellType
{
    Damage,
    Heal,
    Disable,
    Special
}

[CreateAssetMenu(
    fileName = "NewSpell",
    menuName = "Battle/Spell"
)]
public class Spell : ScriptableObject
{
    [Header("Identity")]
    public string spellName;
    [TextArea]
    public string description;

    [Header("Combat")]
    public SpellType type;
    public int power;

    [Header("Accuracy")]
    [Range(0f, 1f)]
    public float accuracy = 1f;

    [Header("Turn Cost")]
    public int turnCost = 1;
}