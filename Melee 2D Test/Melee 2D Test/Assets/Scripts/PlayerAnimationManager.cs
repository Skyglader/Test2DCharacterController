using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    public PlayerManager player;
    public Animator animator;

    public float dashClipLength;
    public float rollClipLength;
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

    public void UpdateAnimClipTimes()
    {
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
        }
    }
}
