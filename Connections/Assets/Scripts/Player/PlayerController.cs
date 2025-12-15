using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerID { Player1, Player2 }

    [Header("Player Settings")]
    [SerializeField] private PlayerID playerID = PlayerID.Player1;

    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 13f;
    [SerializeField] private float velPower = 0.96f; 
    
    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 8.5f;
    [SerializeField] private float fallGravityMultiplier = 6f; 
    [SerializeField] private float jumpCutGravityMultiplier = 10f; 
    [Range(0, 0.5f)] [SerializeField] private float coyoteTime = 0.15f; 
    [Range(0, 0.5f)] [SerializeField] private float jumpBufferTime = 0.1f; 

    [Header("Checks")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    // State Variables
    private Rigidbody2D _rb;
    private bool _isFacingRight = true;
    
    // Input Variables
    private DualPlayerControls _controls; 
    private Vector2 _moveInput; 
    private bool _isJumpHeld;
    
    // Timers
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

        // 1. Defined Actions
        void OnJumpPressed(InputAction.CallbackContext ctx)
        {
            _lastJumpTime = jumpBufferTime; // Start Buffer Timer
            _isJumpHeld = true;
        }

        void OnJumpReleased(InputAction.CallbackContext ctx)
        {
            _isJumpHeld = false;
        }

        // 2. Bindings
        if (playerID == PlayerID.Player1)
        {
            _controls.Gameplay.MoveP1.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _controls.Gameplay.MoveP1.canceled += ctx => _moveInput = Vector2.zero;
            
            _controls.Gameplay.JumpP1.performed += OnJumpPressed;
            _controls.Gameplay.JumpP1.canceled += OnJumpReleased;
        }
        else // Player 2
        {
            _controls.Gameplay.MoveP2.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _controls.Gameplay.MoveP2.canceled += ctx => _moveInput = Vector2.zero;
            
            _controls.Gameplay.JumpP2.performed += OnJumpPressed;
            _controls.Gameplay.JumpP2.canceled += OnJumpReleased;
        }
    }

    private void OnDisable()
    {
        _controls.Gameplay.Disable();
    }

    private void Update()
    {
        // Decrement timers
        _lastGroundedTime -= Time.deltaTime;
        _lastJumpTime -= Time.deltaTime;

        // Jump Request Check
        if (_lastJumpTime > 0 && _lastGroundedTime > 0 && !_isJumping)
        {
            PerformJump();
        }

        // Sprite Flipping
        if (_moveInput.x != 0)
        {
            CheckDirectionToFace(_moveInput.x > 0);
        }
    }

    private void FixedUpdate()
    {
        // 1. Physics Movement
        Run();

        // 2. Ground Check
        bool isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer);
        
        if (isGrounded)
        {
            _lastGroundedTime = coyoteTime; // Reset Coyote timer
            
            if (_rb.linearVelocity.y <= 0) 
            {
                _isJumping = false;
            }
        }

        HandleGravity();
    }

    private void HandleGravity()
    {
        // Case 1: Falling
        if (_rb.linearVelocity.y < 0)
        {
            _rb.gravityScale = fallGravityMultiplier;
        }
        // Case 2: Jumping UP, but button released (Variable Jump Height)
        else if (_rb.linearVelocity.y > 0 && !_isJumpHeld)
        {
            _rb.gravityScale = jumpCutGravityMultiplier;
        }
        // Case 3: Default (Standing or Jumping UP with button held)
        else
        {
            _rb.gravityScale = 1f;
        }
    }

    private void Run()
    {
        float targetSpeed = _moveInput.x * moveSpeed;
        float speedDif = targetSpeed - _rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x + movement * Time.fixedDeltaTime, _rb.linearVelocity.y);
    }

    private void PerformJump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0); 
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        _lastGroundedTime = 0; // Consume Coyote Time
        _lastJumpTime = 0;     // Consume Jump Buffer
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
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
    }
}