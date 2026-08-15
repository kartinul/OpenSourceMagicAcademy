using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public float interactionRange = 1.5f;
    public LayerMask interactableLayer;
    public bool canInteract = true;

    [Header("Interaction UI")]
    public GameObject interactionPrompt;
    public TMP_Text interactionText;

    public IInteractable currentInteractable { get; private set; }

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        FindInteractable();

        if (canInteract && currentInteractable != null)
        {
            if (interactionPrompt != null && !interactionPrompt.activeSelf)
                interactionPrompt.SetActive(true);

            if (interactionText != null)
                interactionText.text = currentInteractable.InteractionPrompt;

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentInteractable.Interact();
            }
        }
        else
        {
            if (interactionPrompt != null && interactionPrompt.activeSelf)
                interactionPrompt.SetActive(false);
        }
    }

    void FindInteractable()
    {
        if (!canInteract)
        {
            currentInteractable = null;
            return;
        }

        currentInteractable = null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            interactionRange,
            interactableLayer
        );

        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentInteractable = interactable;
            }
        }
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