using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Talk";

    public void Interact()
    {
        Debug.Log("Talking to NPC!");
    }
}