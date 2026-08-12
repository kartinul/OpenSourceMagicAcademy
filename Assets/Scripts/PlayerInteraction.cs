using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public float interactionRange = 1.5f;
    public LayerMask interactableLayer;

    [Header("Interaction UI")]
    public GameObject interactionPrompt;
    public TMP_Text interactionText;

    private IInteractable currentInteractable;

    void Start()
    {
        interactionPrompt.SetActive(false);
    }

    void Update()
    {
        FindInteractable();

        if (currentInteractable != null)
        {
            interactionPrompt.SetActive(true);

            interactionText.text = currentInteractable.InteractionPrompt ;

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentInteractable.Interact();
            }
        }
        else
        {
            interactionPrompt.SetActive(false);
        }
    }

    void FindInteractable()
    {
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
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            interactionRange
        );
    }
}