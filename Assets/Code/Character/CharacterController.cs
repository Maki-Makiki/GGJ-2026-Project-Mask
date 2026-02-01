using Code.Character;
using UnityEngine;

public class CharacterController : MonoBehaviour, IPausable
{
    [Space]
    [Header("Internal Values")]
    [SerializeField] BoxCollider2D boxCollider2D;
    [SerializeField] Rigidbody2D rigidBody2D;

    [Space]
    [Header("Move variables")]
    [SerializeField] float deadzone = 0.15f;
    [SerializeField] bool forceDigital = false; // por si querés modo retro
    [SerializeField] float maxSpeedX = 8f;
    [SerializeField] float acceleration = 40f;
    [SerializeField] float deceleration = 60f;
    [SerializeField] float turnMultiplier = 1.5f;

    [Space]
    [Header("Jump variables")]
    [SerializeField] float JumpForce = 10f;

    [Space]
    [Header("Input")]
    [SerializeField] string MaskToggleAction = "Mask Toggle";
    [SerializeField] string AttackAction = "Action";
    [SerializeField] string JumpAction = "Jump";
    [SerializeField] string MoveAction = "Move";
    [SerializeField] Vector2 inputVector = Vector2.zero;

    [Space]
    [Header("Jump Assist")]
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBufferTime = 0.12f;
    float coyoteTimer = 0f;
    float jumpBufferTimer = 0f;

    [Space]
    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheckDistance = 0.05f;
    [SerializeField] float groundHorizontalTolerance = 0.9f;

    [Space]
    [Header("States")]
    [SerializeField] bool inAir = false;
    [SerializeField] bool grounded = false;
    [SerializeField] bool paused = false;
    [SerializeField] bool AttackMaskEnable = false;
    [SerializeField] bool flip = false;

    [Space]
    [Header("Animations")]
    [SerializeField] bool animate = false;
    [SerializeField] Animator CharacterAnimator;
    [SerializeField] string JumpStartTrigger = "T_JumpStart";
    [SerializeField] string JumpEndTrigger = "T_JumpEnd";
    [SerializeField] string WalkBool = "B_Walk";
    [SerializeField] string FlipBool = "B_Flip";
    [SerializeField] string GroundedBool = "B_Ground";
    [SerializeField] string MaskBool = "B_Mask";
    [SerializeField] string BrilloTrigger = "T_Brillo";
    [SerializeField] string AttackTrigger = "T_Attack";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(boxCollider2D == null)
            boxCollider2D = this.gameObject.GetComponent<BoxCollider2D>();

        if (rigidBody2D == null)
            rigidBody2D = this.gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (paused)
            return;

        CheckMonkeyInput();
        CheckMaskToggleInput();
        CheckAttackInput();
        CheckGrounded();
        CheckHorizontalInput();
        CheckJumpInput();
        UpdateJumpTimers();
        TryConsumeJump();

        if (grounded == false)
        {
            inAir = true;
            CharacterAnimator.ResetTrigger(JumpEndTrigger);
        }
           
    }

    private void FixedUpdate()
    {
        if (paused) return;
        MoveCharacter();
    }

    private void OnDrawGizmosSelected()
    {
        if (boxCollider2D == null) return;

        Gizmos.color = grounded ? Color.green : Color.red;

        Bounds bounds = boxCollider2D.bounds;
        Vector3 center = bounds.center + Vector3.down * groundCheckDistance;
        Gizmos.DrawWireCube(center, bounds.size * groundHorizontalTolerance);
    }

    public void CheckHorizontalInput()
    {
        inputVector = ControllerManager.GetActionVector2(MoveAction);

        // Deadzone (evita drift del stick)
        if (Mathf.Abs(inputVector.x) < deadzone)
            inputVector.x = 0f;

        // Opcional: forzar digital (solo -1, 0, 1)
        if (forceDigital)
            inputVector.x = inputVector.x == 0 ? 0 : Mathf.Sign(inputVector.x);

        if(animate)
            CharacterAnimator.SetBool(WalkBool, (inputVector.x != 0) );

        
        if (inputVector.x != 0f)
            flip = inputVector.x < 0;

        if (animate)
            CharacterAnimator.SetBool(FlipBool, flip);
    }

    public void MoveCharacter()
    {
        float targetSpeed = inputVector.x * maxSpeedX;

        float speedDiff = targetSpeed - rigidBody2D.linearVelocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        // freno extra al cambiar de dirección
        if (Mathf.Abs(targetSpeed) > 0.01f &&
            Mathf.Sign(targetSpeed) != Mathf.Sign(rigidBody2D.linearVelocity.x))
        {
            accelRate *= turnMultiplier;
        }

        float movement = speedDiff * accelRate;

        rigidBody2D.AddForce(Vector2.right * movement);
    }


    private void CheckJumpInput()
    {
        if (ControllerManager.GetActionWasPressed(JumpAction))
        {
            Debug.Log("SALTO");
            jumpBufferTimer = jumpBufferTime;
        }
    }

    private void UpdateJumpTimers()
    {
        // Coyote time: si estás en el piso, resetea, si no, cuenta hacia abajo
        if (grounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Jump buffer: cuenta hacia abajo
        jumpBufferTimer -= Time.deltaTime;
    }

    private void TryConsumeJump()
    {
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            Jump();
        }
    }

    public void Jump()
    {
        if(animate)
            CharacterAnimator.SetTrigger(JumpStartTrigger);
        //
        Vector2 v = rigidBody2D.linearVelocity;

        if (v.y < 0f)
            v.y = 0f;

        rigidBody2D.linearVelocity = v;

        rigidBody2D.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);


    }

    private void CheckAttackInput()
    {
        if (ControllerManager.GetActionWasPressed(AttackAction) && AttackMaskEnable)
        {
            Debug.Log("Ataque!");
            CharacterAnimator.SetBool(WalkBool, false);
            CharacterAnimator.SetTrigger(AttackTrigger);
        }
    }

    private void CheckMonkeyInput()
    {
        if (ControllerManager.GetActionWasPressed(AttackAction) && !AttackMaskEnable)
        {
            Debug.Log("Mono!");
            CharacterAnimator.SetTrigger(BrilloTrigger);
            PlataformChange.state.SetPlataformAState();
        }
    }

    private void CheckMaskToggleInput()
    {
        if (ControllerManager.GetActionWasPressed(MaskToggleAction))
        {
            Debug.Log("Mascara Cambio!");

            AttackMaskEnable = !AttackMaskEnable;
            CharacterAnimator.SetBool(MaskBool, AttackMaskEnable);
        }
    }


    private void CheckGrounded()
    {
        Bounds bounds = boxCollider2D.bounds;

        RaycastHit2D hit = Physics2D.BoxCast(
            bounds.center,
            (bounds.size * groundHorizontalTolerance),
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        grounded = hit.collider != null && rigidBody2D.linearVelocity.y == 0 ;
        if (animate) CharacterAnimator.SetBool(GroundedBool, grounded);

        if (inAir && grounded)
        {
            if (animate) CharacterAnimator.SetTrigger(JumpEndTrigger);
            if (animate) CharacterAnimator.ResetTrigger(JumpStartTrigger);
            inAir = false;
        }
    }

    public void OnPause()
    {
        Debug.LogWarning("OnPause() Not implemented ");
        //pausa
    }

    public void Pause()
    {
        throw new System.NotImplementedException();
    }
}
