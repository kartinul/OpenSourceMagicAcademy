using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text spellText;

    private Spell spell;
    private BattleManager battleManager;

    public void Initialize(Spell spell, BattleManager battleManager)
    {
        this.spell = spell;
        this.battleManager = battleManager;

        spellText.text = spell.spellName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(UseSpell);
    }

    private void UseSpell()
    {
        battleManager.PlayerUseSpell(spell);
    }
}