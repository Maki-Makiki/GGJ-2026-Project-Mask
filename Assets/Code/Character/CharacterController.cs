using Code.Character;
using UnityEngine;
using UnityEngine.Windows;

public class CharacterController : MonoBehaviour, IPausable
{
    [Space]
    [Header("Internal Values")]
    [SerializeField] CapsuleCollider2D boxCollider2D;
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
    [SerializeField] string PausaAction = "Pausa";
    [SerializeField] string AttackAction = "Action";
    [SerializeField] string JumpAction = "Jump";
    [SerializeField] string MoveAction = "Move";
    [SerializeField] Vector2 inputVector = Vector2.zero;

    [Space]
    [Header("Atack Timers")]
    [SerializeField] public float cooldownAttack = 0.15f; // Evita el ruido de metralleta
    [SerializeField] private float timerAttack;

    [Space]
    [Header("Jump Assist")]
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBufferTime = 0.12f;
    float coyoteTimer = 0f;
    float jumpBufferTimer = 0f;

    [Space]
    [Header("Ground Check")]
    [SerializeField] bool onSteepSlope = false;
    [SerializeField] bool jumpConsumedThisFrame = false;
    [SerializeField] Vector2 groundNormal = Vector2.up;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheckDistance = 0.05f;
    [SerializeField] float groundHorizontalTolerance = 0.9f;

    [Space]
    [Header("States")]
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
        if (boxCollider2D == null)
            boxCollider2D = this.gameObject.GetComponent<CapsuleCollider2D>();

        if (rigidBody2D == null)
            rigidBody2D = this.gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckPausaInput();

        if (ControllerManager.state.isPaused)
            return;

        CheckActionInput();
        CheckMaskToggleInput();
        CheckJumpInput();

        CheckHorizontalInput();

        UpdateAttackTimers();

        UpdateJumpTimers();
        TryConsumeJump();
    }

    private void FixedUpdate()
    {
        if (paused) return;

        CheckGrounded();

        MoveCharacter();

        if (grounded && !jumpConsumedThisFrame)
            StickToGround();

        jumpConsumedThisFrame = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (boxCollider2D == null) return;

        Gizmos.color = grounded ? Color.green : Color.red;

        Bounds bounds = boxCollider2D.bounds;
        Vector3 center = bounds.center + Vector3.down * groundCheckDistance;
        Gizmos.DrawWireCube(center, bounds.size * groundHorizontalTolerance);
    }

    void StickToGround()
    {
        Vector2 v = rigidBody2D.linearVelocity;
        float normalDot = Vector2.Dot(v, groundNormal);

        // Solo “pegar” al suelo si nos estamos yendo hacia abajo
        if (normalDot > 0f && v.y <= 0f)
            v -= normalDot * groundNormal;

        rigidBody2D.linearVelocity = v;
    }

    public void CheckPausaInput() {
        if (ControllerManager.GetActionWasPressed(PausaAction))
        {
            SetPausePressed("CheckPausaInput()");
        }
    }


    public void CheckHorizontalInput()
    {
        if(ControllerManager.state.ImputDivece == ControllerManager.m_ImputDivice.Touch){return;}
        HorizontalVectorUpdate(ControllerManager.GetActionVector2(MoveAction).x, "CheckHorizontalInput()");
    }

    public void HorizontalVectorUpdate(float inputVectorX, string origin)
    {
        if (ControllerManager.GetBlockedControllers())
            inputVectorX = 0;
        //ControllerManager.ConsoleLog($"HorizontalVectorUpdate({origin}) - X = {inputVectorX}");


        // Deadzone (evita drift del stick)
        if (Mathf.Abs(inputVectorX) < deadzone)
            inputVectorX = 0f;

        // Opcional: forzar digital (solo -1, 0, 1)
        if (forceDigital)
            inputVectorX = inputVectorX == 0 ? 0 : Mathf.Sign(inputVectorX);

        inputVector.x = inputVectorX;

        if (animate)
            CharacterAnimator.SetBool(WalkBool, (inputVectorX != 0) );

        if (inputVectorX != 0f)
            flip = inputVectorX < 0;

        if (animate)
            CharacterAnimator.SetBool(FlipBool, flip);
    }

    public void MoveCharacter()
    {
        // sin control en rampas empinadas
        if (onSteepSlope)
            return;

        float targetSpeed = inputVector.x * maxSpeedX;
        float speedDiff = targetSpeed - rigidBody2D.linearVelocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f)
            ? acceleration
            : deceleration;

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
            SetJumpPressed("CharacterController");
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
    private void UpdateAttackTimers()
    {
        if (timerAttack > 0) timerAttack -= Time.deltaTime;
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
        if (animate)
            CharacterAnimator.SetTrigger(JumpStartTrigger);

        jumpConsumedThisFrame = true;
        grounded = false;
        onSteepSlope = false;

        // 🔒 limpiar TODA la velocidad previa
        rigidBody2D.linearVelocity = Vector2.zero;

        rigidBody2D.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
    }



    private void CheckActionInput()
    {
        if (ControllerManager.GetActionWasPressed(AttackAction))
        {
            SetActionPressed("CharacterController");
        }
    }

    private void Attack(string origin)
    {
        if (timerAttack <= 0)
        {
            ControllerManager.ConsoleLog($"Attack({origin})");
            CharacterAnimator.SetBool(WalkBool, false);
            CharacterAnimator.SetTrigger(AttackTrigger);
            timerAttack = cooldownAttack;
        }
    }

    private void MonkeyPower(string origin)
    {
        ControllerManager.ConsoleLog($"MonkeyPower({origin})");
        CharacterAnimator.SetTrigger(BrilloTrigger);
        PlataformChange.state.SetPlataformAState();
    }


    private void CheckMaskToggleInput()
    {
        if (ControllerManager.GetActionWasPressed(MaskToggleAction))
        {
            SetMaskPressed("CharacterController");
        }
    }

    private void ChangeMask(string origin)
    {
        Debug.Log($"ChangeMask({origin})");

        AttackMaskEnable = !AttackMaskEnable;
        CharacterAnimator.SetBool(MaskBool, AttackMaskEnable);
        ///aqui
    }

    private void CheckGrounded()
    {
        Bounds bounds = boxCollider2D.bounds;

        // Detecta piso simple por debajo
        RaycastHit2D hit = Physics2D.BoxCast(
            bounds.center,
            new Vector2(bounds.size.x * groundHorizontalTolerance, bounds.size.y),
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        bool wasGrounded = grounded;
        grounded = hit.collider != null;

        // Animaciones
        if (animate)
            CharacterAnimator.SetBool(GroundedBool, grounded);

        if (!wasGrounded && grounded)
        {
            if (animate)
            {
                CharacterAnimator.SetTrigger(JumpEndTrigger);
                CharacterAnimator.ResetTrigger(JumpStartTrigger);
            }
        }

        if (wasGrounded && !grounded)
        {
            if (animate)
                CharacterAnimator.ResetTrigger(JumpEndTrigger);
        }
    }


    // ----------------------
    // INPUT INTERMEDIARIO
    // ----------------------
    public void SetHorizontalInput(float x, string origin)
    {
       
        if (ControllerManager.state.ImputDivece == ControllerManager.m_ImputDivice.Touch)
        {
            //ControllerManager.ConsoleLog($"SetJumpPressed({origin}) - X = {x}");
            HorizontalVectorUpdate(x, origin);
        }
    }

    public void SetJumpPressed(string origin)
    {
        ControllerManager.ConsoleLog($"SetJumpPressed({origin})");
        jumpBufferTimer = jumpBufferTime;
    }

    public void SetActionPressed(string origin)
    {
        if (ControllerManager.GetBlockedControllers()) return;
        ControllerManager.ConsoleLog($"SetActionPressed({origin})");
        if (AttackMaskEnable)
            Attack(origin);
        else
            MonkeyPower(origin);
    }

    public void SetMaskPressed(string origin)
    {
        if (ControllerManager.GetBlockedControllers()) return;
        ControllerManager.ConsoleLog($"SetMaskPressed({origin})");
        ChangeMask(origin);
    }

    public void OnPause()
    {
        Debug.LogWarning("OnPause() Not implemented ");
        //pausa
    }

    public void SetPausePressed(string origin)
    {
        if (ControllerManager.GetBlockedControllers()) return;
        ControllerManager.ConsoleLog($"SetPausePressed({origin})");
        SendPause();
    }

    public void SendPause()
    {
        ControllerManager.state.PauseGame();
    }

    public void Pause()
    {
        //throw new System.NotImplementedException();
    }


    public void LockControls() => ControllerManager.LockControls();
    public void UnlockControls() => ControllerManager.UnlockControls();

}
