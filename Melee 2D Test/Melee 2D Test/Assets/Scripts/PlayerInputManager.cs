using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager instance;
    public PlayerControls playerControls;

    [Header("Movement Input")]
    public Vector2 movementInput;
    public bool isJumping = false;
    public bool isDashing = false;
    public bool isAttacking = false;
    public bool isRolling = false;
    public bool isBlocking = false;

    [Header("Button Checks")]
    public bool jumpPressed = false;
    public bool dashPressed = false;
    public bool attackPressed = false;
    public bool rollPressed = false;
    public float attackDelayTime = 0.1f;
    public float jumpDelayTime = 0.2f;
    public float dashDelayTime = 0.2f;
    public float rollDelayTime = 0.1f;
    

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.PlayerMovement.Jump.started += OnJumpStarted;
            playerControls.PlayerMovement.Jump.canceled += OnJumpCanceled;
            playerControls.PlayerMovement.Dash.started += OnDashStarted;
            playerControls.PlayerMovement.Dash.canceled += OnDashCanceled;
            playerControls.PlayerMovement.Roll.started += OnRollStarted;
            playerControls.PlayerMovement.Roll.canceled += OnRollCanceled;
            playerControls.PlayerAttack.Attack.started += OnAttackStarted;
            playerControls.PlayerAttack.Block.performed += i => isBlocking = true;
            playerControls.PlayerAttack.Block.canceled += i => isBlocking = false;
            //playerControls.PlayerAttack.Attack.canceled += OnAttackCanceled;
        }

        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.PlayerMovement.Jump.started -= OnJumpStarted;
        playerControls.PlayerMovement.Jump.canceled -= OnJumpCanceled;
        playerControls.PlayerMovement.Dash.started -= OnDashStarted;
        playerControls.PlayerMovement.Dash.canceled -= OnDashCanceled;
        playerControls.PlayerAttack.Attack.started -= OnAttackStarted;
        //playerControls.PlayerAttack.Attack.canceled -= OnAttackCanceled;
    }

    
    // jumping
    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        // Register the jump action only if it hasn't been pressed yet
      
            jumpPressed = true;
            isJumping = true;
            //StartCoroutine(ResetJumpTime(jumpDelayTime));
       
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpPressed = false;
        isJumping = false;
    }

    // dashing
    private void OnDashStarted(InputAction.CallbackContext context)
    {
        
        dashPressed = true;
        isDashing = true;
        StartCoroutine(ResetDashTime(dashDelayTime));
    }

    private void OnDashCanceled(InputAction.CallbackContext context)
    {
        dashPressed = false;
    }

    private void OnRollStarted(InputAction.CallbackContext context)
    {
        rollPressed = true;
        isRolling = true;
        StartCoroutine(ResetRollTime(rollDelayTime));
    }

    private void OnRollCanceled(InputAction.CallbackContext context)
    {
        rollPressed = false;
    }


    // attacking
    private void OnAttackStarted(InputAction.CallbackContext context)
    { 
        isAttacking = true;
        StartCoroutine(ResetAttackTime(attackDelayTime));
    }

    /*private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        attackPressed = false;
    }*/

    public Vector2 GetMovementDirection()
    {
        return movementInput.normalized;
    }
    
    public IEnumerator ResetDashTime(float time)
    {
        yield return new WaitForSeconds(time);
        isDashing = false;
    }
    public IEnumerator ResetJumpTime(float time)
    {
        yield return new WaitForSeconds(time);
        isJumping = false;
    }

    public IEnumerator ResetRollTime(float time)
    {
        yield return new WaitForSeconds(time);
        isRolling = false;
    }

    public IEnumerator ResetAttackTime(float time)
    {
        yield return null;
        isAttacking = false;
    }
}
