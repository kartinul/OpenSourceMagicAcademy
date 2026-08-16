using UnityEngine;
using TMPro;
using System.Collections;

public class SpellBook : MonoBehaviour
{
    [SerializeField] private float unlockUIDelay = 3f;
    [Header("Spells")]
    [SerializeField] private Spell[] unlockedSpells;

    [Header("Unlock UI")]
    [SerializeField] private GameObject spellUnlockPanel;
    [SerializeField] private TMP_Text spellNameText;
    [SerializeField] private TMP_Text educationalDescriptionText;
    [SerializeField] private TMP_Text battleDescriptionText;

    public Spell[] UnlockedSpells => unlockedSpells;

    public bool IsUnlockPanelOpen =>
        spellUnlockPanel != null && spellUnlockPanel.activeSelf;


    private void Awake()
    {
        if (spellUnlockPanel != null)
            spellUnlockPanel.SetActive(false);
    }


    public void UnlockSpell(Spell spell)
    {
        if (spell == null)
            return;

        foreach (Spell unlocked in unlockedSpells)
        {
            if (unlocked == spell)
                return;
        }

        Spell[] newSpells =
            new Spell[unlockedSpells.Length + 1];

        unlockedSpells.CopyTo(newSpells, 0);
        newSpells[^1] = spell;

        unlockedSpells = newSpells;

        Debug.Log($"Unlocked spell: {spell.spellName}");

        StartCoroutine(ShowSpellUnlockDelayed(spell));
    }

    private IEnumerator ShowSpellUnlockDelayed(Spell spell)
    {
        yield return new WaitForSeconds(unlockUIDelay);

        ShowSpellUnlock(spell);
    }


    private void ShowSpellUnlock(Spell spell)
    {
        if (spellNameText != null)
            spellNameText.text = spell.spellName;

        if (educationalDescriptionText != null)
            educationalDescriptionText.text =
                spell.educationalDescription;

        if (battleDescriptionText != null)
            battleDescriptionText.text =
                spell.description;

        if (spellUnlockPanel != null)
            spellUnlockPanel.SetActive(true);
    }


    public void CloseSpellUnlock()
    {
        if (spellUnlockPanel != null)
            spellUnlockPanel.SetActive(false);
    }
}