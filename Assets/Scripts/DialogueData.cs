using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(
    fileName = "NewDialogue",
    menuName = "Dialogue/Dialogue"
)]
public class DialogueData : ScriptableObject
{
    public string speakerName;

    [TextArea(2, 5)]
    public string[] lines;

    public UnityEvent onDialogueComplete;
}