
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField, Tooltip("How fast to move")]
    float moveSpeed = 7f;

    [SerializeField, Tooltip("How high the jump is. Holding down the jump button keeps adding this speed")]
    float jumpForce = 12f;

    [SerializeField, Tooltip("How many seconds of the player pressing the JUMP button a bit to soon to allow and still register as soon as they land")]
    float earlyJumpTime = .10f;

    [SerializeField, Tooltip("How many seconds after leaping off the is the player allow to still jump")]
    float coyoteTime = .1f;

    [SerializeField, Tooltip("How long to allow the JUMP button to add more jump height")]
    float jumpHeightMaxTime = 3f;

    [SerializeField]
    Transform feetXForm;

    [SerializeField, Tooltip("How big is the radious used to test for collision with the ground")]
    float groundCollisionRadious;

    [SerializeField]
    LayerMask groundLayerMask;

    [SerializeField]
    Transform shieldAttachedXForm;
    public Transform ShieldAttachedXForm { get { return shieldAttachedXForm; } }

    [SerializeField]
    Slider deathMeter;

    PlayerInputActions inputActions;

    [Header("[Debugger]")]
    [SerializeField, Tooltip("Failsafe, recording the starting point in case we don't have a checkpoint to respawn at")]
    Vector3 startingPos;
    public Vector3 StartingPos { get { return startingPos; } }

    [SerializeField]
    Vector2 movementVector;

    public bool WasGrounded { get; protected set; }
    public bool IsGrounded { get { return IsCurrentlyGrounded(); } }

    [SerializeField]
    bool isJumping;
    public bool IsJumping { get { return isJumping; } }

    /// <summary>
    /// RB is always "falling" to touch the ground so we need to both test 
    /// that we are both not grounded and falling
    /// </summary>
    [SerializeField]
    bool WasFalling;
    public bool IsFalling { get { return WasFalling; } }

    [SerializeField]
    bool jumpButtonPressed;
    public bool JumpButtonPressed { get { return jumpButtonPressed; } }

    public bool IsPressingLeft { get { return movementVector.x < 0f; } }
    public bool IsPressingRight { get { return movementVector.x > 0f; } }
    public bool IsPressingDown { get { return movementVector.y < 0f; } }
    public bool JumpedDown { get; set; }
    public bool CanJumpDown 
    { 
        get {
            return IsGrounded
                && IsPressingDown 
                && JumpButtonPressed
                && !JumpedDown;
        } 
    }

    Rigidbody2D rb;
    Rigidbody2D RB
    {
        get
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
            return rb;
        }
    }

    Animator animator;
    Animator Animator
    {
        get
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            return animator;
        }
    }

    new Collider2D collider2D;
    public Collider2D Collider2D
    {
        get
        {
            if (collider2D == null)
                collider2D = GetComponent<Collider2D>();
            return collider2D;
        }
    }

    Shield shield;
    Shield Shield
    {
        get
        {
            if (shield == null)
                shield = FindObjectOfType<Shield>();
            return shield;
        }
    }

    Parallax parallax;
    Parallax Parallax
    {
        get
        {
            if (parallax == null)
                parallax = FindObjectOfType<Parallax>();
            return parallax;
        }
    }

    public int FacingDirection { get { return transform.localScale.x > 0f ? 1 : -1; } }
    public bool IsInLight { get { return lightSources.Count > 0; } }

    [SerializeField]
    List<LightBall> lightSources;

    public Vector2 Velocity { get { return RB.velocity; } }

    IEnumerator delayJumpRoutine;
    IEnumerator coyoteTimeRoutine;

    private void Awake()
    {
        lightSources = new List<LightBall>();
        inputActions = new PlayerInputActions();
        
        inputActions.Player.Enable();

        inputActions.Player.Attack.started += OnAttackButtonPressed;
        inputActions.Player.Jump.started += OnJumpButtonPressed;
        inputActions.Player.Jump.canceled += OnJumpButtonReleased;        

        startingPos = transform.position;
        deathMeter.value = 0f;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(feetXForm.position, groundCollisionRadious);
    }

    /// <summary>
    /// Read/Execute player inputs
    /// </summary>
    private void Update()
    {
        if (GameManager.instance.IsGameOver || GameManager.instance.IsGamePaused)
            return;

        movementVector = inputActions.Player.Movement.ReadValue<Vector2>();
        LookAtMovingDirection();
    }

    /// <summary>
    /// Execute all physics based logic
    /// </summary>
    private void FixedUpdate()
    {
        if (GameManager.instance.IsGameOver || GameManager.instance.IsGamePaused)
            return;

        Move();
    }

    /// <summary>
    /// Now that both player input and physics have been processed
    /// we can update the player according to their final state
    /// </summary>
    private void LateUpdate()
    {
        Parallax.Speed = Vector2.zero;

        if (GameManager.instance.IsGameOver || GameManager.instance.IsGamePaused)
            return;

        Parallax.Speed = RB.velocity;

        // Whether to show them moving or not
        var speed = movementVector.x != 0f ? 1f : 0f;
        Animator.SetFloat("MoveSpeed", speed);

        // Last frame the player was grounded but now they are falling
        // Start coyote time
        if(WasGrounded && IsCurrentFalling())
            StartCoyoteTimeRoutine();

        // Last frame we were falling but now we are grounded
        else if (WasFalling && IsCurrentlyGrounded())
        {
            // We've landed
            AudioManager.instance.Play(SFXLibrary.instance.playerLand);
            isJumping = false;
            JumpedDown = false;
        }

        // Save the current states
        WasGrounded = IsCurrentlyGrounded();
        WasFalling = IsCurrentFalling();

        // Looks like we are grounded this frame so make sure we can jump again
        if (IsCurrentlyGrounded())
        {
            isJumping = false;
            JumpedDown = false;
        }   
    }

    bool IsCurrentFalling() => !IsCurrentlyGrounded() && RB.velocity.y <= 0f;

    /// <summary>
    /// Light boxes need to be ON to validated as solid ground so we must first check them
    /// when we encounter them
    /// </summary>
    /// <returns></returns>
    bool IsCurrentlyGrounded()
    {
        var collider = Physics2D.OverlapCircle(feetXForm.position, groundCollisionRadious, groundLayerMask);
        if(collider != null)
        {
            var lightBox = collider.GetComponent<LightBox>();
            if (lightBox != null)
                return lightBox.IsOn;
        }

        return collider != null;
    }

    void LookAtMovingDirection()
    {
        if (IsPressingLeft && FacingDirection != -1)
            Flip();

        if (IsPressingRight && FacingDirection != 1)
            Flip();
    }

    void Flip()
    {
        var scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }    

    private void Move()
    {
        // We want snappy movement so we change the velocity of the player to match their input
        // We don't want to override the "Y" since we might be falling/jumping
        RB.velocity = new Vector2(movementVector.x * moveSpeed, RB.velocity.y);
    }

    public void WalkSound()
    {
        if (IsGrounded) 
            AudioManager.instance.Play(SFXLibrary.instance.playerStep);
    }

    public void OnJumpButtonPressed(InputAction.CallbackContext context)
    {
        if (GameManager.instance.IsGameOver || GameManager.instance.IsGamePaused)
            return;

        jumpButtonPressed = true;

        // Currently grounded so jump!
        if (IsGrounded)
            Jump();

        // Looks like we are still on the way down but hit the jump button a bit too early
        // So we will trigger a routine to try the jump again in a few frames
        else if (!WasGrounded && IsCurrentFalling())
            StartDelayJumpRoutine();
    }

    public void OnJumpButtonReleased(InputAction.CallbackContext context)
    {
        jumpButtonPressed = false;
    }

    void Jump()
    {
        isJumping = true;

        // Since we are potentially falling, we might not have enough force 
        // to launch us into the air so we want to stop all veritcal movement
        // before we jump again
        RB.velocity = Vector2.right * RB.velocity;

        // Now we can add the first which should start from ZERO an overcome its mass
        RB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        AudioManager.instance.Play(SFXLibrary.instance.playerJump);
    }

    void StartDelayJumpRoutine()
    {
        // Prevent running multiple
        StopDelayJumpRoutine();

        // Trigger it
        delayJumpRoutine = DelayJumpRoutine();
        StartCoroutine(delayJumpRoutine);
    }

    void StopDelayJumpRoutine()
    {
        if (delayJumpRoutine != null)
            StopCoroutine(delayJumpRoutine);
    }

    /// <summary>
    /// Checks each "frame" we are allowing the player to trigger a jump
    /// until a jump is available to trigger it. Otherwise, no jump is trigged
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayJumpRoutine()
    {
        var startTime = earlyJumpTime;
        while (startTime > 0f) 
        {
            yield return new WaitForEndOfFrame();
            startTime -= Time.deltaTime;
            if (IsGrounded && !isJumping)
            {
                Jump();
                break;
            }
        }        

        yield return new WaitForEndOfFrame();
    }

    void StartCoyoteTimeRoutine()
    {
        // Prevent running multiple
        StopCoyoteTimeRoutine();

        // The player cannot already be holding the jump button
        // To avoid the scenario if the player jumping onto a platform
        // keep holding the jump button, run off the platform, and then jump again
        jumpButtonPressed = false;

        // Trigger it
        coyoteTimeRoutine = CoyoteTimeRoutine();
        StartCoroutine(coyoteTimeRoutine);
    }

    void StopCoyoteTimeRoutine()
    {
        if (coyoteTimeRoutine != null)
            StopCoroutine(coyoteTimeRoutine);
    }

    IEnumerator CoyoteTimeRoutine()
    {
        var startTime = coyoteTime;
        while (startTime > 0f)
        {
            yield return new WaitForEndOfFrame();
            startTime -= Time.deltaTime;

            // The player jumped a few frame after begining to fall
            if (IsCurrentFalling() && jumpButtonPressed)
            {
                Jump();
                break;
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public void OnAttackButtonPressed(InputAction.CallbackContext context)
    {
        if (GameManager.instance.IsGameOver || GameManager.instance.IsGamePaused)
            return;

        switch (Shield.State)
        {
            case ShieldState.Attached:
                if (Shield.CanThrowShield)
                    PlayThrowAnimation();
                break;

            case ShieldState.Thrown:
                Shield.RecallShield();
                break;
        }
    }

    public void ThrowShield() => Shield.ThrowShield(FacingDirection);

    public void PlayThrowAnimation() => Animator.Play("Throw");
    public void PlayDeathAnimation() => Animator.Play("Death");
    public void AddLightSource(LightBall source)
    {
        if (!lightSources.Contains(source))
            lightSources.Add(source);
    }
    public void RemoveLightSource(LightBall source)
    {
        if (lightSources.Contains(source))
            lightSources.Remove(source);
    }

    public void GameOver()
    {
        RB.isKinematic = true;
        RB.velocity = Vector2.zero;
        lightSources.Clear();
        Animator.SetFloat("MoveSpeed", 0f);
    }

    public void Respawn()
    {
        RB.isKinematic = false;
        RB.velocity = Vector2.zero;
        deathMeter.value = 0f;
        Animator.Play("Locomotion");
    }
}
