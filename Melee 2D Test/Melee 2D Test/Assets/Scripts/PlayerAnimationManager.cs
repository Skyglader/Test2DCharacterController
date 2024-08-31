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
        HandleMovementAnimations();
        isHit = player.playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("PlayerHit");

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
        }
    }
}
