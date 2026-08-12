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
    [Header("Spell")]
    public string spellName;

    [TextArea(2, 4)]
    public string description;

    [Header("What you'll learn")]
    [TextArea(3, 6)]
    public string educationalDescription;

    [Header("Battle")]
    public SpellType type;

    public int power = 20;

    [Range(0f, 1f)]
    public float accuracy = 1f;
}