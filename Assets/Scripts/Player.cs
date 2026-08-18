using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
  [Header("Movement")]
  public float moveSpeed = 5f;
  public Rigidbody2D rb;

  public int level = 1;

  public bool canMove = true;
  [Header("Animation")]
  public Animator animator;

  private Vector2 movement;
  private Vector2 facingDirection = Vector2.down;

  public static Player Instance { get; private set; }

  void Awake()
  {
      Instance = this;
    // else
    // {
    //   Destroy(gameObject);
    //   return;
    // }

    if (rb == null)
      rb = GetComponent<Rigidbody2D>();

    if (UnityEngine.EventSystems.EventSystem.current == null)
    {
      GameObject esObj = new GameObject("EventSystem");
      esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
      esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
      DontDestroyOnLoad(esObj);
    }

    if (animator == null)
      animator = GetComponentInChildren<Animator>();

  }

  void OnEnable()
  {
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  void OnDisable()
  {
    SceneManager.sceneLoaded -= OnSceneLoaded;
  }

  void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    canMove = true;
    PlayerInteraction interact = GetComponent<PlayerInteraction>();
    if (interact != null) interact.canInteract = true;

    GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawn");

    if (spawnPoints.Length > 0)
    {
      foreach (GameObject spawnPoint in spawnPoints)
      {
        if (spawnPoint.GetComponent<PlayerSpawn>().spawnId == PlayerPrefs.GetInt("spawnId"))
          transform.position = spawnPoint.transform.position;
      }
    }
  }
  void Update()
  {
    ReadInput();
    UpdateAnimation();
  }

  void FixedUpdate()
  {
    if (rb != null)
    {
      rb.MovePosition(
          rb.position +
          movement * moveSpeed * Time.fixedDeltaTime
      );
    }
  }

  void ReadInput()
  {
    movement = Vector2.zero;

    if (!canMove)
      return;

    if (Keyboard.current != null)
    {
      if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) movement.y += 1;
      if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) movement.y -= 1;
      if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) movement.x -= 1;
      if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) movement.x += 1;
    }

    if (Gamepad.current != null)
    {
      Vector2 stick = Gamepad.current.leftStick.ReadValue();
      if (stick.sqrMagnitude > 0.01f) movement += stick;

      if (Gamepad.current.dpad.up.isPressed) movement.y += 1;
      if (Gamepad.current.dpad.down.isPressed) movement.y -= 1;
      if (Gamepad.current.dpad.left.isPressed) movement.x -= 1;
      if (Gamepad.current.dpad.right.isPressed) movement.x += 1;
    }

    movement = movement.normalized;
  }

  void UpdateAnimation()
  {
    bool isMoving = movement.sqrMagnitude > 0.001f;

    if (isMoving)
    {
      facingDirection = GetAnimationDirection(movement);
    }

    animator.SetBool("isMoving", isMoving);

    animator.SetFloat("moveX", facingDirection.x);
    animator.SetFloat("moveY", facingDirection.y);
  }

  Vector2 GetAnimationDirection(Vector2 direction)
  {

    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
    {
      return new Vector2(
          Mathf.Sign(direction.x),
          0f
      );
    }

    return new Vector2(
        0f,
        Mathf.Sign(direction.y)
    );
  }
}