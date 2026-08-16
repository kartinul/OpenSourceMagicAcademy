using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.Events;
using System;

public class DialogueManager : MonoBehaviour
{
  [Header("UI Panels")]
  [Tooltip("Outer shared panel container")]
  [SerializeField] private GameObject dialoguePanel;
  [Tooltip("Inner dialogue-only panel container (Speaker Name, [SPACE] Prompt)")]
  [SerializeField] private GameObject dialogueSpecificContainer;

  [Header("UI Text References")]
  [SerializeField] private TMP_Text dialogueText;
  [SerializeField] private TMP_Text speakerText;

  [Header("Typing Settings")]
  [SerializeField] private float typingSpeed = 0.03f;
  private Player playerController;
  private PlayerInteraction playerInteraction;
  private CinemachineTargetGroup targetGroup;
  private Transform playerRootTransform;

  private DialogueData currentDialogue;
  private int currentLine;

  private bool isTyping;
  private bool dialogueActive;

  private Coroutine typingCoroutine;
  private WaitForSeconds typingWait;
  public UnityEvent OnDialogueEnded = new UnityEvent();
  [SerializeField] private DialogueSpeaker currentSpeaker;

  [SerializeField] private DialogueData startingDialogue;
  public static event EventHandler<DialogueFinishArgs> OnDialogueFinish;
  public class DialogueFinishArgs : EventArgs { public Combatant enemy; };


  private void OnEnable()
  {
    NPC.OnNPCInteract += NPC_OnNPCInteract;
  }
  private void NPC_OnNPCInteract(object sender, NPC.InteractEventArgs e)
  {
    StartDialogue(e.dialogueData);

    if (e.enemy is not null)
    {
      OnDialogueFinish?.Invoke(this, new DialogueFinishArgs
      {
        enemy = e.enemy
      });
    }
  }


  private void OnDisable()
  {
    NPC.OnNPCInteract -= NPC_OnNPCInteract;
  }



  private void Awake()
  {
    typingWait = new WaitForSeconds(typingSpeed);

    playerRootTransform = transform.root;
    playerController = playerRootTransform.GetComponent<Player>();
    playerInteraction = playerRootTransform.GetComponent<PlayerInteraction>();
    targetGroup = playerRootTransform.GetComponentInChildren<CinemachineTargetGroup>();

    if (dialoguePanel != null)
      dialoguePanel.SetActive(false);

    if (dialogueSpecificContainer != null)
      dialogueSpecificContainer.SetActive(false);
    targetGroup.Targets.Clear();
    targetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = playerRootTransform, Weight = 1f, Radius = 1f });

    if (startingDialogue != null) StartDialogue(startingDialogue);
  }

  private void Update()
  {
    if (!dialogueActive || Keyboard.current == null)
      return;

    if (Keyboard.current.spaceKey.wasPressedThisFrame)
    {
      AdvanceDialogue();
    }
  }

  // this is really shitty architecture but i honestly dont know what to do at this point
  // im exhausted
  public void StartDialogue(DialogueData dialogue)
  {
    Transform npcTarget = null;

    if (playerInteraction == null)
      playerInteraction = transform.root.GetComponent<PlayerInteraction>();

    if (playerInteraction != null && playerInteraction.currentInteractable != null)
    {
      if (playerInteraction.currentInteractable is Component interactableComponent)
      {
        npcTarget = interactableComponent.transform;
      }
    }

    StartDialogue(dialogue, npcTarget);
  }

  public void StartDialogue(DialogueData dialogue, Transform npcTransform)
  {
    if (dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0)
    {
      Debug.LogError("[DialogueManager] Tried to start null or empty dialogue.");
      return;
    }


    SetPlayerControlsActive(false);

    if (targetGroup != null)
    {
      targetGroup.Targets.Clear();

      targetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = playerRootTransform, Weight = 1f, Radius = 1f });

      if (npcTransform != null)
      {
        targetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = npcTransform, Weight = 1f, Radius = 1f });
      }
    }

    currentDialogue = dialogue;
    currentLine = 0;
    dialogueActive = true;

    if (dialoguePanel != null)
      dialoguePanel.SetActive(true);

    if (dialogueSpecificContainer != null)
      dialogueSpecificContainer.SetActive(true);

    if (speakerText != null)
      speakerText.text = dialogue.speakerName;

    currentSpeaker = npcTransform != null
        ? npcTransform.GetComponentInParent<DialogueSpeaker>()
        : null;

    if (currentSpeaker != null)
      currentSpeaker.StartSpeaking();

    ShowCurrentLine();
  }

  private void ShowCurrentLine()
  {
    if (currentSpeaker != null)
      currentSpeaker.StartSpeaking();

    if (typingCoroutine != null)
      StopCoroutine(typingCoroutine);

    typingCoroutine = StartCoroutine(TypeLine(currentDialogue.lines[currentLine]));
  }

  private IEnumerator TypeLine(string line)
  {
    isTyping = true;

    dialogueText.text = line;
    dialogueText.maxVisibleCharacters = 0;
    dialogueText.ForceMeshUpdate();

    int characterCount = dialogueText.textInfo.characterCount;

    for (int i = 0; i < characterCount; i++)
    {
      dialogueText.maxVisibleCharacters = i + 1;
      yield return typingWait;
    }

    isTyping = false;
    if (currentSpeaker != null) currentSpeaker.StopSpeaking();
  }

  private void AdvanceDialogue()
  {
    if (isTyping)
    {
      if (typingCoroutine != null)
        StopCoroutine(typingCoroutine);

      dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
      isTyping = false;
      return;
    }

    currentLine++;

    if (currentLine >= currentDialogue.lines.Length)
    {
      EndDialogue();
      return;
    }

    ShowCurrentLine();
  }

  private void EndDialogue()
  {
    dialogueActive = false;
    isTyping = false;

    if (typingCoroutine != null)
    {
      StopCoroutine(typingCoroutine);
      typingCoroutine = null;
    }

    if (dialogueText != null) dialogueText.text = string.Empty;
    if (speakerText != null) speakerText.text = string.Empty;

    if (dialogueSpecificContainer != null)
      dialogueSpecificContainer.SetActive(false);

    if (dialoguePanel != null)
      dialoguePanel.SetActive(false);

    if (targetGroup != null)
    {
      targetGroup.Targets.Clear();
      targetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = playerRootTransform, Weight = 1f, Radius = 1f });
    }

    if (currentSpeaker != null)
    {
      currentSpeaker.StopSpeaking();
      currentSpeaker = null;
    }

    SetPlayerControlsActive(true);

    OnDialogueEnded?.Invoke();
    OnDialogueEnded.RemoveAllListeners();
  }

  private void SetPlayerControlsActive(bool active)
  {
    if (playerController == null || playerInteraction == null)
    {
      playerRootTransform = transform.root;
      if (playerController == null) playerController = playerRootTransform.GetComponent<Player>();
      if (playerInteraction == null) playerInteraction = playerRootTransform.GetComponent<PlayerInteraction>();
    }

    if (playerController != null)
      playerController.canMove = active;

    if (playerInteraction != null)
      playerInteraction.canInteract = active;
  }
}