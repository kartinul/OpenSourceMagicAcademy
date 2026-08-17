using System;
using UnityEngine;


public class NPC : MonoBehaviour, IInteractable
{
  [Header("Interaction")]
  [SerializeField] private string interactionPrompt = "Talk";
  [SerializeField] private DialogueData dialogueData;
  [SerializeField] private bool playTalkingSound = false;
  [SerializeField] private float pitch = 1f;


  [SerializeField] private bool onlyTalkOnce = false;

  public static event EventHandler<InteractEventArgs> OnNPCInteract;
  public class InteractEventArgs : EventArgs { public DialogueData dialogueData; public Combatant enemy; public bool playTalkingSound; public float pitch; };

  private bool hasTalked = false;

  public string InteractionPrompt => (onlyTalkOnce && hasTalked) ? "" : interactionPrompt;
  private Combatant enemy;

  public void Start()
  {
    enemy = gameObject.GetComponent<Combatant>();
  }

  public void Interact()
  {
    if (onlyTalkOnce && hasTalked) return;
    
    DialogueData dataToPlay = dialogueData;

    if (enemy != null)
    {
      if (Player.Instance.level < enemy.level)
      {
        dataToPlay = ScriptableObject.CreateInstance<DialogueData>();
        dataToPlay.speakerName = "System";
        dataToPlay.lines = new string[] { "Your level is too low." };
      }
      else
      {
        hasTalked = true;
      }
    }
    else
    {
      hasTalked = true;
    }

    OnNPCInteract?.Invoke(this, new InteractEventArgs
    {
      dialogueData = dataToPlay,
      enemy = enemy,
      playTalkingSound = playTalkingSound,
      pitch = pitch
    });
  }

}