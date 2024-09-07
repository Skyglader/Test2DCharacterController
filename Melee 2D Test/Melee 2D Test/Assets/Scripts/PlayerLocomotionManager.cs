using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLocomotionManager : MonoBehaviour
{
    public PlayerManager player;

    [Header("Movement")]
    public float moveSpeed = 10f;
    public float poweredMoveSpeed = 13f;
    public float attackSlideSpeed = 2f;

    [Header("Jumping")]
    public float jumpStartTime;
    public float jumpTime;
    public float jumpForce;
    public bool hasJumped = false;

    [Header("Gravity")]
    public float gravityOnGround = -13f;
    public float gravityInAir = -13f;

    [Header("Booleans")]
    public bool isMoving = false;
    public bool stopMoving = false;
    public bool stopJumping = false;
    public bool stopDashing = false;
    public bool stopRolling = false;
    public bool movementActive = false;

    [Header("Dashing")]
    public bool canDash = true;
    public bool canRoll = true;
    public bool isDashing;
    public float dashingPower = 24f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;

    [Header("Rolling")]
    public bool isRolling;
    public float rollingPower = 24f;
    public float rollingTime = 0.2f;
    public float rollingCooldown = 1f;

    [Header("Private Corountines")]
    private Coroutine stopMovementCoroutine;
    private Coroutine stopJumpingCoroutine;
    private Coroutine stopDashingCoroutine;
    private Coroutine stopRollingCoroutine;
    private void Awake()
    {
        player = GetComponent<PlayerManager>();
    }

    private void Start()
    {
        canRoll = true;
        canDash = true;
    }
    private void Update()
    {
        if (isDashing || isRolling)
        {
            return;
        }
        HandleRollingMovement();
        HandleDashingMovement();
        HandleInAirAnimations();
    }

    private void FixedUpdate()
    {
        if (isDashing || isRolling)
        {
            return;
        }
        HandlePlayerMovement();
        HandleJumpingMovement();
        
        HandleGravity();
    }

    private void HandleInAirAnimations()
    {
        if (player.rb.velocity.y > 0f)
        {
            player.playerAnimator.SetBool("isRising", true);
            player.playerAnimator.SetBool("isFalling", false);
        }
        else if (player.rb.velocity.y < 0f)
        {
            player.playerAnimator.SetBool("isRising", false);
            player.playerAnimator.SetBool("isFalling", true);
        }
        
        if (player.isGrounded) 
        {
            player.playerAnimator.SetBool("isRising", false);
            player.playerAnimator.SetBool("isFalling", false);
            player.playerAnimator.SetBool("isGrounded", true) ;
        }
        else
        {
            player.playerAnimator.SetBool("isGrounded", false);
        }

    }
    private void HandleGravity()
    {
        if (!player.playerCombatManager.currentlyAttacking)
        {
            if (player.isGrounded)
            {
                player.rb.AddForce(new Vector2(0f, gravityOnGround * player.rb.gravityScale), ForceMode2D.Force);
            }
            else if (!player.isGrounded)
            {
                player.rb.AddForce(new Vector2(0f, gravityInAir * player.rb.gravityScale), ForceMode2D.Force);
            }
        }

    }
    private void HandlePlayerMovement()
    {
        if (stopMoving)
        { 
            player.rb.velocity = new Vector2(Mathf.Lerp(player.rb.velocity.x, 0f, attackSlideSpeed * Time.deltaTime), 0f);
            return;
        }
        Vector2 dir = PlayerInputManager.instance.GetMovementDirection();

        if (dir != Vector2.zero)
        {
            Vector2 targetVelocity;

            if (player.playerCombatManager.inPoweredState || !player.isGrounded)
            {
                targetVelocity = dir * poweredMoveSpeed;
            }
            else
            {
                targetVelocity = dir * moveSpeed;
            }
            player.rb.velocity = targetVelocity;
            isMoving = true;

            // Change playermodel orientation based on direction

            if (dir.x > 0f)
            {
                player.playerModel.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                player.playerModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
        else
        {
            player.rb.velocity = Vector2.zero; 
            isMoving = false;
        }
    }
    
    private void HandleJumpingMovement()
    {
        if (stopJumping)
        {
            return;
        }
        if (player.isGrounded == true && PlayerInputManager.instance.isJumping == true)
        {
            player.isGrounded = false;
            player.playerAnimator.SetBool("isRising", true);
            jumpTime = jumpStartTime;

            player.rb.velocity = new Vector2(player.rb.velocity.x, jumpForce);
            hasJumped = true;
        }
        else if (PlayerInputManager.instance.isJumping == true)
        {
            if (jumpTime > 0 && hasJumped)
            {
                float jumpForceMultiplier = jumpTime / jumpStartTime;
                player.rb.velocity = new Vector2(player.rb.velocity.x, jumpForce * jumpForceMultiplier);
                jumpTime -= Time.deltaTime;
            }
        }
        else if (!PlayerInputManager.instance.isJumping)
        {
            // Reset jump status when the jump button is released
            hasJumped = false;
        }
    }

    private void HandleDashingMovement()
    {
        if (PlayerInputManager.instance.isDashing && canDash && !stopDashing && !player.playerCombatManager.currentlyAttacking)
        {
            StartCoroutine(Dash());
        }
    }

    private void HandleRollingMovement()
    {
        if (PlayerInputManager.instance.isRolling && canRoll && !stopRolling && !player.playerCombatManager.currentlyAttacking && player.isGrounded && !player.playerCombatManager.inPoweredState)
        {
            // activate i-frame at beginning of roll
            StartCoroutine(Roll());
        }
    }


    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float dashAnimatorSpeed;
        float speed = dashingPower;
        float dashDuration = dashingTime;
        float cooldown = dashingCooldown;

        if (player.playerCombatManager.inPoweredState)
        {
            dashAnimatorSpeed = player.playerAnimationManager.poweredDashClipLength / dashingTime;
            speed += speed * 0.5f;
            dashDuration = 0.3f;
            cooldown = 0.3f;
        }
        else
        {
            dashAnimatorSpeed = player.playerAnimationManager.dashClipLength / dashingTime;
        }

        player.playerAnimator.SetFloat("dashSpeed", dashAnimatorSpeed);
        player.playerAnimator.SetBool("isDashing", true);
        float originalGravity = player.rb.gravityScale;
        player.rb.gravityScale = 0f;

       
        if (player.playerModel.transform.localRotation.y == 0f)
        {
            player.rb.velocity = Vector2.right * speed;
        }
        else
        {
            player.rb.velocity = Vector2.left * speed;
        }
        
        

        StopAllMovement(dashDuration);
        yield return new WaitForSeconds(dashDuration);
        player.playerAnimator.speed = 1f;
        player.rb.gravityScale = originalGravity;
        player.playerAnimator.SetBool("isDashing", false);
        isDashing = false;
        //player.rb.velocity = new Vector2(5f, 0f);
        yield return new WaitForSeconds(cooldown);
        canDash = true;
    }
    private IEnumerator Roll()
    {
        canRoll = false;
        isRolling = true;
        float jumpAnimatorSpeed = player.playerAnimationManager.rollClipLength / rollingTime;
        player.playerAnimator.SetFloat("rollingSpeed", jumpAnimatorSpeed);
        player.playerAnimator.SetBool("isRolling", true);

        
        if (player.playerModel.transform.localRotation.y == 0f)
        {
            player.rb.velocity = Vector2.right * rollingPower;
        }
        else
        {
            player.rb.velocity = Vector2.left * rollingPower;
        }

        StopAllMovement(rollingTime);
        yield return new WaitForSeconds(rollingTime);

        player.playerAnimator.speed = 1f;
        player.playerAnimator.SetBool("isRolling", false);
        isRolling = false;

        // Apply rolling cooldown
        yield return new WaitForSeconds(rollingCooldown);
        canRoll = true;
     
    }

    public void StopAllMovement(float time)
    {
        if (time > 0)
        {
            StopGroundedMovement(time);
            StopJumpingMovement(time);
            StopDashingMovement(time);
            StopRollingMovement(time);
        }
        else
        {
            StopAllMovementRestrictions();

        }
    }

    public void StopAllMovementRestrictions()
    {
        if (stopMovementCoroutine != null)
        {
            StopCoroutine(stopMovementCoroutine);
            stopMoving = false;
        }
        if (stopJumpingCoroutine != null)
        {
            StopCoroutine(stopJumpingCoroutine);
            stopJumping = false;
        }
        if (stopDashingCoroutine != null)
        {
            StopCoroutine(stopDashingCoroutine);
            stopDashing = false;
        }
        if (stopRollingCoroutine != null)
        {
            StopCoroutine(stopRollingCoroutine);
            stopRolling = false;
        }
    }

    public void StopGroundedMovement(float time)
    {
        if (stopMovementCoroutine != null)
        {
            StopCoroutine(stopMovementCoroutine);
        }
        stopMoving = true;
        stopMovementCoroutine = StartCoroutine(ResetStopGroundedMovement(time));
    }

    public IEnumerator ResetStopGroundedMovement(float time)
    {
        yield return new WaitForSeconds(time);
        stopMoving = false;
        stopMovementCoroutine = null;
    }

    public void StopJumpingMovement(float time)
    {
        if (stopJumpingCoroutine != null)
        {
            StopCoroutine(stopJumpingCoroutine);
        }
        stopJumping = true;
        stopJumpingCoroutine = StartCoroutine(ResetStopJumpingMovement(time));
    }
    public IEnumerator ResetStopJumpingMovement(float time)
    {
        yield return new WaitForSeconds(time);
        stopJumping = false;
        stopJumpingCoroutine = null;
    }

    public void StopDashingMovement(float time)
    {
        if (stopDashingCoroutine != null)
        {
            StopCoroutine(stopDashingCoroutine);
        }
        stopDashing = true;
        stopDashingCoroutine = StartCoroutine(ResetStopDashingMovement(time));

    }

    public IEnumerator ResetStopDashingMovement(float time)
    {
        yield return new WaitForSeconds(time);
        stopDashing = false;
        stopDashingCoroutine = null;
    }

    public void StopRollingMovement(float time)
    {
        if (stopRollingCoroutine != null)
        {
            StopCoroutine(stopRollingCoroutine);
        }
        stopRolling = true;
        stopRollingCoroutine = StartCoroutine(ResetStopRollingMovement(time));
    }
    public IEnumerator ResetStopRollingMovement(float time)
    {
        yield return new WaitForSeconds(time);
        stopRolling = false;
        stopRollingCoroutine = null;
    }
   
}
