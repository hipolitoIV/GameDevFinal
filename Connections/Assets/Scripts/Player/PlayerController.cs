using UnityEngine;
using UnityEngine.InputSystem; // Required for New Input System

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    // Still need to tweak parameters.
    public enum PlayerID { Player1, Player2 }

    [Header("Player Settings")]
    [SerializeField] private PlayerID playerID = PlayerID.Player1;

    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 13f;
    [SerializeField] private float velPower = 0.96f; 
    
    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float fallGravityMultiplier = 4f; 
    [SerializeField] private float jumpCutGravityMultiplier = 8f; 
    [Range(0, 0.5f)] [SerializeField] private float coyoteTime = 0.15f; 
    [Range(0, 0.5f)] [SerializeField] private float jumpBufferTime = 0.1f; 

    [Header("Checks")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1.0f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    // State Variables
    private Rigidbody2D _rb;
    private bool _isFacingRight = true;
    
    // New Input System Variables
    private DualPlayerControls _controls; 
    private Vector2 _moveInput; // Renamed to be generic for both players
    private bool _isJumpHeld;
    
    // Jump Timing
    private float _lastGroundedTime;
    private float _lastJumpTime;
    private bool _isJumping;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _controls = new DualPlayerControls();
    }

    private void OnEnable()
    {
        _controls.Gameplay.Enable();

        // Helper actions to avoid code duplication between P1 and P2
        void onJumpPerformed(InputAction.CallbackContext ctx)
        {
            _lastJumpTime = jumpBufferTime;
            _isJumpHeld = true;
        }

        void onJumpCanceled(InputAction.CallbackContext ctx)
        {
            _isJumpHeld = false;
            if (_rb.linearVelocity.y > 0)
            {
                _rb.gravityScale = jumpCutGravityMultiplier;
            }
        }

        // Subscribe based on which player this script is attached to
        if (playerID == PlayerID.Player1)
        {
            _controls.Gameplay.MoveP1.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _controls.Gameplay.MoveP1.canceled += ctx => _moveInput = Vector2.zero;
            
            _controls.Gameplay.JumpP1.performed += onJumpPerformed;
            _controls.Gameplay.JumpP1.canceled += onJumpCanceled;
        }
        else if (playerID == PlayerID.Player2)
        {
            // IMPORTANT: Ensure "MoveP2" and "JumpP2" exist in your Input Actions file!
            _controls.Gameplay.MoveP2.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _controls.Gameplay.MoveP2.canceled += ctx => _moveInput = Vector2.zero;
            
            _controls.Gameplay.JumpP2.performed += onJumpPerformed;
            _controls.Gameplay.JumpP2.canceled += onJumpCanceled;
        }
    }

    private void OnDisable()
    {
        _controls.Gameplay.Disable();
    }

    private void Update()
    {
        // 1. Timers
        _lastGroundedTime -= Time.deltaTime;
        _lastJumpTime -= Time.deltaTime;

        // 2. Jump Logic
        if (_lastJumpTime > 0 && _lastGroundedTime > 0 && !_isJumping)
        {
            Jump();
        }

        // 3. Sprite Flipping
        if (_moveInput.x != 0)
        {
            CheckDirectionToFace(_moveInput.x > 0);
        }
    }

    private void FixedUpdate()
    {
        // 1. Run Movement
        Run();

        // 2. Ground Check
        if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer))
        {
            _lastGroundedTime = coyoteTime;
            _isJumping = false;
            
            if(_rb.linearVelocity.y <= 0) _rb.gravityScale = 1; 
        }
        else
        {
            // 3. Gravity Modifications
            if (_rb.linearVelocity.y < 0)
            {
                _rb.gravityScale = fallGravityMultiplier;
            }
            else if (_rb.linearVelocity.y > 0 && !_isJumpHeld)
            {
                _rb.gravityScale = jumpCutGravityMultiplier;
            }
            else
            {
                _rb.gravityScale = 1f;
            }
        }
    }

    private void Run()
    {
        float targetSpeed = _moveInput.x * moveSpeed;
        float speedDif = targetSpeed - _rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        _rb.AddForce(movement * Vector2.right);
    }

    private void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0); 
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        _lastGroundedTime = 0;
        _lastJumpTime = 0;
        _isJumping = true;
    }

    private void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != _isFacingRight)
        {
            _isFacingRight = !_isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
    }
}