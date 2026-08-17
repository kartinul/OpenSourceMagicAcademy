using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private SpellBook spellBook;

    [Header("Interaction")]
    public float interactionRange = 1.5f;
    public LayerMask interactableLayer;
    public bool canInteract = true;

    [Header("Interaction UI")]
    public GameObject interactionPrompt;
    public TMP_Text interactionText;

    public IInteractable currentInteractable { get; private set; }

    private IInteractable blockedInteractable;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        bool interactPressed = false;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) interactPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) interactPressed = true;

        if (interactPressed)
        {
            if (spellBook != null && spellBook.IsUnlockPanelOpen)
            {
                spellBook.CloseSpellUnlock();
                return;
            }
        }

        FindInteractable();

        if (canInteract && currentInteractable != null)
        {
            if (interactionPrompt != null && !interactionPrompt.activeSelf)
                interactionPrompt.SetActive(true);

            if (interactionText != null)
                interactionText.text = currentInteractable.InteractionPrompt;

            if (interactPressed)
            {
                blockedInteractable = currentInteractable;
                currentInteractable.Interact();
            }
        }
        else
        {
            if (interactionPrompt != null && interactionPrompt.activeSelf)
                interactionPrompt.SetActive(false);
        }
    }

        private Collider2D[] interactionHits = new Collider2D[10];

        void FindInteractable()
        {
            if (!canInteract)
            {
                currentInteractable = null;
                return;
            }

            currentInteractable = null;

            int hitCount = Physics2D.OverlapCircleNonAlloc(
                transform.position,
                interactionRange,
                interactionHits,
                interactableLayer
            );

            float closestDistance = Mathf.Infinity;
            bool blockedStillInRange = false;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = interactionHits[i];
            IInteractable interactable =
                hit.GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            if (interactable == blockedInteractable)
            {
                blockedStillInRange = true;
                continue;
            }

            float distance = Vector2.Distance(
                transform.position,
                hit.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentInteractable = interactable;
            }
        }

        if (!blockedStillInRange)
            blockedInteractable = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = canInteract ? Color.green : Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionRange
        );
    }
}