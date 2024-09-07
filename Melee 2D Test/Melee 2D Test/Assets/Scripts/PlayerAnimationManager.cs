using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerAnimationManager : MonoBehaviour
{
    public PlayerManager player;
    public Animator animator;

    public float dashClipLength;
    public float rollClipLength;
    public float takeHitClipLength;
    public float specialAttackClipLength;
    public float poweredDashClipLength;
    public float takeHitSpeed = 1.5f;

    public bool isHit = false;
    private void Awake()
    {
        player = GetComponent<PlayerManager>();
    }
    void Start()
    {
        UpdateAnimClipTimes();
        animator = player.playerAnimator;
    }


    // Update is called once per frame
    void Update()
    {
        HandleHitStopOnAnimation();
        HandleActivateHitColliderOnAnimation();
        HandleInvincibilityOnAnimation();
        preventInputDuringAnimation();
        preventMovementDuringAnimation();
        HandleMovementAnimations();
        isHit = player.playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("PlayerHit");

    }

    private void HandleHitStopOnAnimation()
    {
        if (player.playerAnimator.GetFloat("hitStopActive") > 0f)
        {
            player.canHitStop = true;
        }
        else
        {
            player.canHitStop = false;
        }
    }
    private void HandleActivateHitColliderOnAnimation()
    {
        if (player.playerAnimator.GetFloat("weaponColliderActive") > 0f)
        {
            player.Attack();
        }
        else
        {
            player.collidersDamaged.Clear();
        }
    }
    private void HandleInvincibilityOnAnimation()
    {
        if (player.playerAnimator.GetFloat("invulnerabilityActive") > 0f)
        {
            player.isInvulnerable = true;
        }
        else
        {
            player.isInvulnerable = false;
        }
    }
    public void preventInputDuringAnimation()
    {


        if (player.clipInfo.Length > 0)
        {
            string currentClipName = player.clipInfo[0].clip.name;
            if (player.validInputStopAnimations.Contains(currentClipName))
            {
                if (player.playerAnimator.GetFloat("stopInput") > 0f)
                {
                    PlayerInputManager.instance.PauseInputs(Mathf.Infinity);
                    player.inputStopped = true;
                }
                else
                {
                    PlayerInputManager.instance.PauseInputs(-1);
                    player.inputStopped = false;
                }
            }
            else
            {
                if (player.inputStopped)
                {
                    PlayerInputManager.instance.PauseInputs(-1);
                    player.inputStopped = false;
                }
            }
        }
    }

    public void preventMovementDuringAnimation()
    {

        if (player.clipInfo.Length > 0)
        {
            string currentClipName = player.clipInfo[0].clip.name;
            if (player.validMovementStopAnimations.Contains(currentClipName))
            {
                if (player.playerAnimator.GetFloat("stopMovement") > 0f)
                {
                    player.playerLocomotionManager.StopAllMovement(Mathf.Infinity);
                    player.movementStopped = true;
                }
                else
                {
                    player.playerLocomotionManager.StopAllMovement(-1);
                    player.movementStopped = false;
                }
            }
            else
            {
                if (player.movementStopped)
                {
                    player.playerLocomotionManager.StopAllMovement(-1);
                    player.movementStopped = false;
                }
            }
        }
    }

    private void HandleMovementAnimations()
    {
        if (player.playerLocomotionManager.isMoving)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    public void HandleTakeHitAnimation()
    {
       

        // stop player when hit
        //player.playerCombatManager.stunPlayer(takeHitClipLength/ takeHitSpeed);

        if (player.playerLocomotionManager.isRolling)
        {
            player.playerAnimator.SetBool("isRolling", false);
        }
        if (player.playerLocomotionManager.isDashing)
        {
            player.playerAnimator.SetBool("isDashing", false) ;
        }
        
        
    }
    
    public void UpdateAnimClipTimes()
    {
        // collect all animation clip lengths
        AnimationClip[] clips = player.playerAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "PlayerDash")
            {
                dashClipLength = clip.length;
            }

            if (clip.name == "PlayerRoll")
            {
                rollClipLength = clip.length;

            }

            if (clip.name == "PlayerHit")
            {
                takeHitClipLength = clip.length;
                Debug.Log("hit length" + takeHitClipLength);
            }

            if (clip.name == "SpecialAttack1")
            {
                specialAttackClipLength = clip.length;
            }

            if (clip.name == "PoweredPlayerDash")
            {
                
            }
        }
    }
}
