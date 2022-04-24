using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField]
    Color unlitColor = Color.gray;

    [SerializeField]
    float thrownSpeed = 5f;

    [SerializeField]
    float recallSpeed = 8f;

    [SerializeField, Range(0.01f, 1f), Tooltip("When recalled, how close it needs to be to the player before attaching")]
    float minAttachDistance = 0.25f;

    [SerializeField, Range(0.01f, 1f), Tooltip("Whe NOT recalled, how close it needs to be to the player before initiaing a recall")]
    float pickupShieldDistance = 0.10f;

    [SerializeField]
    ShieldState state;

    [SerializeField]
    GameObject particleObject;

    public ShieldState State { get { return state; } protected set { state = value; } }
    public bool IsBouncing 
    { 
        get { 
            return state == ShieldState.Throwing 
                || state == ShieldState.Thrown 
                || state == ShieldState.Recalled; 
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

    SpriteRenderer spriteRenderer;
    SpriteRenderer SpriteRenderer
    {
        get
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            return spriteRenderer;
        }
    }

    new Collider2D collider2D;
    Collider2D Collider2D
    {
        get
        {
            if (collider2D == null)
                collider2D = GetComponent<Collider2D>();
            return collider2D;
        }
    }

    Player player;
    Player Player
    {
        get
        {
            if (player == null)
                player = FindObjectOfType<Player>();
            return player;
        }
    }

    [SerializeField]
    Transform targetXForm;
    Transform TargetXForm
    {
        get
        {
            if (targetXForm == null)
                targetXForm = Player.ShieldAttachedXForm;
            return targetXForm;
        }
    }

    [SerializeField, Tooltip("Time, in seconds, the shield remains ON")]
    float totalTimeOn = 3f;
    float lighOnTimer = 0f;

    [SerializeField, Tooltip("The shield is ON")]
    Torch torch;
    Torch Torch
    {
        get
        {
            if (torch == null)
                torch = GetComponentInChildren<Torch>();
            return torch;
        }
    }

    SFXAudioSource shieldSpinSrc;

    bool previouslyOn;
    public bool IsOn 
    { 
        get { return Torch.IsOn; } 
        set 
        { 
            Torch.IsOn = value;
            
            // Reset the timer
            if (value)
            {
                lighOnTimer = totalTimeOn;

                // Since the shield was NOT ON previosly but is ON now 
                // we can play the turning ON sound
                if(!previouslyOn)
                    AudioManager.instance.Play(SFXLibrary.instance.shieldFire);
            }

            // Save it for next time this is changed
            previouslyOn = value;
        }
    }

    public Vector2 Velocity { get { return RB.velocity; } }

    private void Start()
    {
        if(state == ShieldState.Attached)
        {
            RB.velocity = Vector2.zero;
            transform.SetParent(Player.ShieldAttachedXForm);
            transform.localPosition = Vector3.zero;
        }

        // Making sure the shield is not controlled by gravity
        RB.gravityScale = 0f;
    }    

    void FixedUpdate()
    {
        if (GameManager.instance.IsGameOver || GameManager.instance.IsGamePaused)
            return;

        switch (state)
        {
            case ShieldState.Thrown:
                Move(thrownSpeed);
                break;

            case ShieldState.Recalled:
                if (TargetXForm == null)
                    return;

                MoveToTargetXForm();
                // Reached the distination
                if (Vector2.Distance(transform.position, TargetXForm.position) < minAttachDistance)
                    AttachToPlayer();
                break;
        }

        if (state != ShieldState.Attached)
            Rotate();
    }

    private void LateUpdate()
    {
        // Stop the shield sound if it is attached
        if (!IsBouncing && shieldSpinSrc != null)
            shieldSpinSrc.Stop();

        if (GameManager.instance.IsGameOver || GameManager.instance.IsGamePaused)
            return;

        // The shield is not attached but also was not thrown/recall
        // This means it needs to be picked up by the player touching
        if (state == ShieldState.Detached) 
        {
            var distance = Vector2.Distance(transform.position, Player.transform.position);
            var isWithinRange = distance <= pickupShieldDistance;

            if(isWithinRange)
                RecallShield(true);
            return;
        }

        LighTimer();
        Animator.SetFloat("IsOn", IsOn ? 1f : 0f);
    }

    /// <summary>
    /// While the torch is ON it will run down a timer
    /// When it reaches zero, it turns OFF the torch
    /// </summary>
    void LighTimer()
    {
        // Already ran out
        if (!IsOn)
            return;

        lighOnTimer -= Time.deltaTime;
        if (lighOnTimer <= 0f)
        {
            IsOn = false;
            AudioManager.instance.Play(SFXLibrary.instance.shieldFireOut);
        }
    }

    void Rotate()
    {
        var moveDirection = RB.velocity;
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public bool CanThrowShield { get { return state == ShieldState.Attached; } }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        AudioManager.instance.Play(SFXLibrary.instance.shieldBounce);
        // var particles = Instantiate(particleObject, transform);
    }
    
    public void ThrowShield(float direction)
    {
        // Can only throw the shield when attached
        if (!CanThrowShield)
            return;

        StartCoroutine(ThrowShieldRoutine(direction));
    }

    IEnumerator ThrowShieldRoutine(float direction)
    {
        state = ShieldState.Throwing;

        // Re-enable collisions
        RB.isKinematic = false;
        Collider2D.isTrigger = false;

        // Detach from player
        transform.SetParent(null);

        // We want to make sure our scaling is positive or else the rotations won't work
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            Mathf.Abs(transform.localScale.y),
            Mathf.Abs(transform.localScale.z)
        );

        Animator.Play("Thrown");
        AudioManager.instance.Play(SFXLibrary.instance.shieldThrown);
        shieldSpinSrc = AudioManager.instance.Play(SFXLibrary.instance.shieldSpin);
        

        // Trigger the player's animation
        // Player.PlayThrowAnimation();

        // Launch it and wait a frame so the shield as a "direction" set 
        RB.velocity = Vector2.right * direction * thrownSpeed;
        yield return new WaitForEndOfFrame();

        state = ShieldState.Thrown;
    }

    public void RecallShield(bool isAutoRecall = false)
    {
        // Can only recall when the shield was thrown
        if (!isAutoRecall && state != ShieldState.Thrown)
            return;

        // Play sound
        AudioManager.instance.Play(SFXLibrary.instance.shieldRecall);

        // Stop on a dime so that we can change direction
        RB.velocity = Vector2.zero;

        // Making it kinematic so that it can go through walls
        RB.isKinematic = true;
        Collider2D.isTrigger = true;

        // We need to re-allow movement on the Y axis to follow the player
        RB.constraints = RigidbodyConstraints2D.FreezeRotation;

        state = ShieldState.Recalled;
    }

    void Move(float speed)
    {
        var direction = RB.velocity.normalized;
        RB.velocity = direction * speed;
    }

    void MoveToTargetXForm()
    {
        var position = Vector2.MoveTowards(transform.position, TargetXForm.position, recallSpeed * Time.deltaTime);
        RB.MovePosition(position);
    }

    void AttachToPlayer()
    {
        // Stop Moving
        RB.velocity = Vector2.zero;

        // Attach
        transform.SetParent(TargetXForm);
        transform.localPosition = Vector3.zero;

        // Just making sure
        RB.isKinematic = true;
        Collider2D.isTrigger = true;

        // Update the visuals/sound
        Animator.Play("Attached");
        if (shieldSpinSrc != null)
            shieldSpinSrc.Stop();

        AudioManager.instance.Play(SFXLibrary.instance.shieldCaught);
        state = ShieldState.Attached;
    }

    public void GameOver()
    {
        RB.velocity = Vector2.zero;
        transform.SetParent(null);
        IsOn = false;
        lighOnTimer = 0f;
    }

    public void Resapwn()
    {
        if(state != ShieldState.Detached)
            AttachToPlayer();
    }

    public void Poof()
    {
        // Maybe play some particles to show poof
        AttachToPlayer();
    }
}
