using System;
using Unity.AppUI.Editor;
using Unity.InferenceEngine;
using UnityEngine;


public class NPC : MonoBehaviour, IInteractable
{
  [Header("Interaction")]
  [SerializeField] private string interactionPrompt = "Talk";
  [SerializeField] private DialogueData dialogueData;
  [SerializeField] private bool playTalkingSound = false;



  public static event EventHandler<InteractEventArgs> OnNPCInteract;
  public class InteractEventArgs : EventArgs { public DialogueData dialogueData; public Combatant enemy; public bool playTalkingSound; };

  public string InteractionPrompt => interactionPrompt;
  private Combatant enemy;

  public void Start()
  {
    enemy = gameObject.GetComponent<Combatant>();
  }

  public void Interact()
  {
    OnNPCInteract?.Invoke(this, new InteractEventArgs
    {
      dialogueData = dialogueData,
      enemy = enemy,
      playTalkingSound = playTalkingSound
    });
  }

}