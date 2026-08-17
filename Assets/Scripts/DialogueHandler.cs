using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DialogueHandler : MonoBehaviour
{

  [System.Serializable]
  public struct DialogueEvent
  {
    public DialogueData dialogueData;
    public UnityEvent unityEvent;
  }

  [SerializeField] private DialogueEvent[] dialogueEvents;

  private IEnumerator Start()
  {
    DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
    if (dialogueManager == null)
    {
      Debug.LogWarning("DialogueManager not found in the scene.");
      yield break;
    }

    foreach (var dEvent in dialogueEvents)
    {
      bool isDialogueFinished = false;

      dialogueManager.OnDialogueEnded.AddListener(() => isDialogueFinished = true);
      dialogueManager.StartDialogue(dEvent.dialogueData);

      yield return new WaitUntil(() => isDialogueFinished);

      dEvent.unityEvent?.Invoke();
    }
  }

}