using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject dialogueSpecificContainer;

    [Header("UI Text References")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerText;

    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Header("Player Control (Optional / Auto-Found)")]
    [SerializeField] private Player playerController;
    [SerializeField] private PlayerInteraction playerInteraction;

    private DialogueData currentDialogue;
    private int currentLine;

    private bool isTyping;
    private bool dialogueActive;

    private Coroutine typingCoroutine;
    private WaitForSeconds typingWait;

    private void Awake()
    {
        typingWait = new WaitForSeconds(typingSpeed);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueSpecificContainer != null)
            dialogueSpecificContainer.SetActive(false);

        AutoSetupPlayer();
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

    private void AutoSetupPlayer()
    {
        if (playerController == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<Player>();
                playerInteraction = playerObj.GetComponent<PlayerInteraction>();
            }
        }
    }

    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0)
        {
            Debug.LogError("[DialogueManager] Tried to start null or empty dialogue.");
            return;
        }

        SetPlayerControlsActive(false);

        currentDialogue = dialogue;
        currentLine = 0;
        dialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueSpecificContainer != null)
            dialogueSpecificContainer.SetActive(true);

        if (speakerText != null)
            speakerText.text = dialogue.speakerName;

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
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

        SetPlayerControlsActive(true);
    }

    private void SetPlayerControlsActive(bool active)
    {
        if (playerController != null)
            playerController.canMove = active;

        if (playerInteraction != null)
            playerInteraction.canInteract = active;
    }
}