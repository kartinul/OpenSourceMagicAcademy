using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Spell[] spells;

    public Spell ChooseSpell()
    {
        if (spells == null || spells.Length == 0)
        {
            Debug.LogError(
                $"{name} has no spells!"
            );

            return null;
        }

        return spells[Random.Range(0, spells.Length)];
    }
}