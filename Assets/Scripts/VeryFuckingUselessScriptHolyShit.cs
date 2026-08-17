using UnityEngine;

public class VeryFuckingUselessScriptHolyShit : MonoBehaviour
{
  [SerializeField] private DialogueManager dialogueManager;
  [SerializeField] private DialogueData dialogue;

  private void Start()
  {
    dialogueManager.StartDialogue(dialogue, true);
  }
}
