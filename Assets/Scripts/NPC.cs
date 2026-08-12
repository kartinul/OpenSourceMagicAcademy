using UnityEngine;
using UnityEngine.Events;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string interactionPrompt = "Talk";

    [SerializeField] private UnityEvent onInteract;

    public string InteractionPrompt => interactionPrompt;

    public void Interact()
    {
        onInteract?.Invoke();
    }

}