using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    public bool canMove = true;
    [Header("Animation")]
    public Animator animator;

    private Vector2 movement;
    private Vector2 facingDirection = Vector2.down;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
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

        if (!canMove || Keyboard.current == null)
            return;
            
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
        {
            movement.y += 1;
        }

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
        {
            movement.y -= 1;
        }

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            movement.x -= 1;
        }

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            movement.x += 1;
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