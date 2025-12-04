using UnityEngine;
using UnityEngine.InputSystem;

public class DualPlayerController : MonoBehaviour
{
    // Input system
    private DualPlayerControls _controls;

    [Header("Player 1 Settings (WASD)")]
    public Rigidbody2D rb1;
    public float speed1 = 5f;
    public float jumpForce1 = 7f;
    public float jumpCutMultiplier1 = 0.5f;

    [Header("Player 2 Settings (Arrows)")]
    public Rigidbody2D rb2;
    public float speed2 = 5f;
    public float jumpForce2 = 7f;
    public float jumpCutMultiplier2 = 0.5f;
    
    [Header("Global Settings")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.05f; // distance to check below the box

    private Vector2 moveInput1;
    private Vector2 moveInput2;
    
    private bool isJumping1 = false;
    private bool isJumping2 = false;

    private void Awake()
    {
        _controls = new DualPlayerControls();
    }

    private void OnEnable()
    {
        _controls.Gameplay.Enable();

        // Player 1
        _controls.Gameplay.MoveP1.performed += ctx => moveInput1 = ctx.ReadValue<Vector2>();
        _controls.Gameplay.MoveP1.canceled += ctx => moveInput1 = Vector2.zero;
        
        // Updated: Jump P1 now uses both performed and canceled
        _controls.Gameplay.JumpP1.performed += OnJumpP1Performed;
        _controls.Gameplay.JumpP1.canceled += OnJumpP1Canceled;

        // Player 2
        _controls.Gameplay.MoveP2.performed += ctx => moveInput2 = ctx.ReadValue<Vector2>();
        _controls.Gameplay.MoveP2.canceled += ctx => moveInput2 = Vector2.zero;
        
        // Updated: Jump P2 now uses both performed and canceled
        _controls.Gameplay.JumpP2.performed += OnJumpP2Performed;
        _controls.Gameplay.JumpP2.canceled += OnJumpP2Canceled;
    }

    private void OnDisable()
    {
        _controls.Gameplay.Disable();
    }

    // --- Player 1 Jump Handlers ---
    private void OnJumpP1Performed(InputAction.CallbackContext context)
    {
        if (IsGrounded(rb1))
        {
            rb1.linearVelocity = new Vector2(rb1.linearVelocity.x, jumpForce1);
            isJumping1 = true; // Start of the jump button press
        }
    }

    private void OnJumpP1Canceled(InputAction.CallbackContext context)
    {
        // Variable jump height logic
        if (isJumping1 && rb1.linearVelocity.y > 0)
        {
            // Reduce upward velocity when the jump button is released early
            rb1.linearVelocity = new Vector2(rb1.linearVelocity.x, rb1.linearVelocity.y * jumpCutMultiplier1);
        }
        isJumping1 = false; // End of the jump button press
    }

    // --- Player 2 Jump Handlers ---
    private void OnJumpP2Performed(InputAction.CallbackContext context)
    {
        if (IsGrounded(rb2))
        {
            rb2.linearVelocity = new Vector2(rb2.linearVelocity.x, jumpForce2);
            isJumping2 = true; // Start of the jump button press
        }
    }

    private void OnJumpP2Canceled(InputAction.CallbackContext context)
    {
        // Variable jump height logic
        if (isJumping2 && rb2.linearVelocity.y > 0)
        {
            // Reduce upward velocity when the jump button is released early
            rb2.linearVelocity = new Vector2(rb2.linearVelocity.x, rb2.linearVelocity.y * jumpCutMultiplier2);
        }
        isJumping2 = false; // End of the jump button press
    }

    private void FixedUpdate()
    {   
        // Player 1
        float targetVelocityX1 = moveInput1.x * speed1;
        rb1.linearVelocity = new Vector2(targetVelocityX1, rb1.linearVelocity.y);

        // Player 2
        float targetVelocityX2 = moveInput2.x * speed2;
        rb2.linearVelocity = new Vector2(targetVelocityX2, rb2.linearVelocity.y);
    }

    /// <summary>
    /// Uses BoxCast to check if the player's Rigidbody2D is grounded.
    /// </summary>
    private bool IsGrounded(Rigidbody2D rb)
    {
        if (!rb.TryGetComponent<BoxCollider2D>(out var box)) return false;

        // Cast a box downward slightly below the player
        RaycastHit2D hit = Physics2D.BoxCast(
            box.bounds.center,
            box.bounds.size,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    // Optional: visualize the ground check in the editor
    private void OnDrawGizmosSelected()
    {
        if (rb1 != null)
        {
            DrawGroundCheckGizmo(rb1);
        }
        if (rb2 != null)
        {
            DrawGroundCheckGizmo(rb2);
        }
    }

    private void DrawGroundCheckGizmo(Rigidbody2D rb)
    {
        BoxCollider2D box = rb.GetComponent<BoxCollider2D>();
        if (box == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(box.bounds.center + Vector3.down * groundCheckDistance, box.bounds.size);
    }
}