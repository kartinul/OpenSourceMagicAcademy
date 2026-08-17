using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellButton : MonoBehaviour
{
  [SerializeField] private Button button;
  [SerializeField] private TMP_Text spellText;
  [SerializeField] private float colorMultiplier = 1.3f;


  private Button btn;

  private Spell spell;
  private BattleManager battleManager;


  void Awake()
  {
    btn = GetComponent<Button>();
  }

  public void SetVisualActive()
  {
    ColorBlock clr = btn.colors;
    clr.colorMultiplier = colorMultiplier;
    btn.colors = clr;
  }
  public void SetVisualInactive()
  {
    ColorBlock clr = btn.colors;
    clr.colorMultiplier = 1;
    btn.colors = clr;
  }
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