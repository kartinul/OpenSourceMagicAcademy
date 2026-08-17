using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollegeExteriorSequence : MonoBehaviour
{
  [Header("Core Dependencies")]
  [SerializeField] private DialogueManager dialogueManager;

  [Header("Scene Objects to Focus On")]
  [SerializeField] private GameObject dumbledore;
  [SerializeField] private GameObject coldemort;

  private void Start()
  {
    dialogueManager = Player.Instance.GetComponentInChildren<DialogueManager>();

    StartCoroutine(RunSequence());
  }

  private IEnumerator RunSequence()
  {
    // --- STEP 1: Dumbledore Welcome ---
    EventHelpers.FocusCameraOn(dumbledore);
    yield return PlayDialogueAndWait(CreateDialogue("Humbledore",
        "Ah! There you are.",
        "Welcome to the Open Source Magic Academy.",
        "Have you settled into your house and made friends yet? I assure you, the-"
    ));

    // Big shake interrupting the welcome
    EventHelpers.ShakeScreenWithForce(1.5f, 0.8f);
    EventHelpers.ShakeScreenWithForce(1.5f, 0.8f);
    yield return new WaitForSeconds(0.5f); // Pause for dramatic effect

    // --- STEP 2: Dumbledore Reacts ---
    yield return PlayDialogueAndWait(CreateDialogue("Humbledore",
        "Hmm?\nThat is... unusual.",
        "Everyone, stay back!"
    ));

    // --- STEP 3: Coldemort Appears ---
    EventHelpers.FocusCameraOnWithZoom(coldemort, 12f);
    EventHelpers.ShakeScreen(0.3f); // Slight shake as he appears
    yield return PlayDialogueAndWait(CreateDialogue("Coldemort",
        "So this is where you've been hiding it, Humbledore."
    ));

    // --- STEP 4: Dumbledore Defends ---
    EventHelpers.FocusCameraOn(dumbledore);
    yield return PlayDialogueAndWait(CreateDialogue("Humbledore",
        "You are not invited here, Coldemort."
    ));

    // --- STEP 5: Coldemort Threatens ---
    EventHelpers.FocusCameraOn(coldemort);
    yield return PlayDialogueAndWait(CreateDialogue("Coldemort",
        "For far too long, you have allowed your students to share what should have belonged to its creators.",
        "Code. Knowledge. Magic.",
        "All of it will belong to me now."
    ));

    // --- STEP 6: Dumbledore Warns ---
    EventHelpers.FocusCameraOn(dumbledore);
    yield return PlayDialogueAndWait(CreateDialogue("Humbledore",
        "You cannot simply take the work of an entire world and lock it away!"
    ));

    EventHelpers.FocusCameraOnWithZoom(coldemort, 15f);
    // --- STEP 7: Coldemort Casts Spell ---
    yield return PlayDialogueAndWait(CreateDialogue("Coldemort",
        "Watch me.",
        "CLOSED SOURCEIMUS CONVERTIMUS!"
    ));
    EventHelpers.FlashScreenRed();
    EventHelpers.ShakeScreenWithForce(5f, 1.5f);

    // --- STEP 8: Dumbledore Screams & Scene Ends ---
    EventHelpers.FocusCameraOn(dumbledore);
    yield return PlayDialogueAndWait(CreateDialogue("Humbledore",
        "NOOOOOOOOOOOO!!",
        "Young wizard, we must act at once. Meet me inside the college."
    ));

    // --- SEQUENCE END ---
    EventHelpers.FlashScreenWhite(); // Fade to white or black to end
    EventHelpers.ClearCameraFocus();

    Debug.Log("College Exterior Sequence Finished! Loading next scene...");

    // Give the screen flash a tiny bit of time before cutting the scene instantly
    yield return new WaitForSeconds(0.5f);

    SceneChanger.changeScene(0, "CollegeExterior");
  }

  /// <summary>
  /// Instantiates a transient DialogueData scriptable object entirely in code.
  /// </summary>
  private DialogueData CreateDialogue(string speaker, params string[] lines)
  {
    DialogueData data = ScriptableObject.CreateInstance<DialogueData>();
    data.speakerName = speaker;
    data.lines = lines;
    return data;
  }

  /// <summary>
  /// Helper method that triggers a dialogue and pauses the coroutine until the player finishes it.
  /// </summary>
  private IEnumerator PlayDialogueAndWait(DialogueData data)
  {
    if (data == null)
    {
      Debug.LogWarning("Missing dialogue data in CollegeExteriorSequence!");
      yield break;
    }

    bool isFinished = false;

    UnityEngine.Events.UnityAction onEnd = null;
    onEnd = () =>
    {
      isFinished = true;
      dialogueManager.OnDialogueEnded.RemoveListener(onEnd);
    };

    dialogueManager.OnDialogueEnded.AddListener(onEnd);
    dialogueManager.StartDialogue(data);


    yield return new WaitUntil(() => isFinished);
  }
}
