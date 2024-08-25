using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerCombatManager : MonoBehaviour
{
    public PlayerManager player;

    public AttackState attackState;
    public CombatState combatState;
    public bool shouldCombo = false;
    public bool executedAttack = false;
    public bool currentlyAttacking = false;
    public float parryOpeningTime = 0.5f;

    public float parryWindow;
    public bool parryWindowSet;

    [Header("Attack Animation Thresholds")]
    [SerializeField] Dictionary<string, Tuple<float, float>> normalizedTimeThresholds;

    public enum CombatState
    {
        Air,
        Grounded
    }
    public enum AttackState
    {
        Idle,
        Attack1,
        Attack2,
        Attack3,
        Parry,
        Block
    }
    

    private void Awake()
    {
        player = GetComponent<PlayerManager>();
    }

    private void Start()
    {
        attackState = AttackState.Idle;


        // Tuple<normalizedTime, timeToStopMovement>
        normalizedTimeThresholds = new Dictionary<string, Tuple<float, float>>
        {
            { "Attack1", Tuple.Create(0.7f, 0.4f) },
            { "Attack2", Tuple.Create(0.72f, 0.3f) },
            { "Attack3", Tuple.Create(0.8f, 0.2f) },
            { "AirAttack1", Tuple.Create(0.8f, 0.2f) }
        };
    }
    private void Update()
    {
        if (player.isGrounded && !player.playerLocomotionManager.isDashing && !player.playerLocomotionManager.isRolling)
        {
            if (combatState != CombatState.Grounded)
            {
                combatState = CombatState.Grounded;
                attackState = AttackState.Idle;
            }
            HandleGroundAttackCombos();
        }
        else if (!player.isGrounded)
        {
            if (combatState != CombatState.Air)
            {
                combatState = CombatState.Air;
                attackState = AttackState.Idle;
            }
            HandleAirAttackCombos();
        }

    }

    private void HandleGroundAttackCombos()
    {
        switch (attackState)
        {
            case AttackState.Idle:
                
                // state transitions
                currentlyAttacking = false;
                shouldCombo = false;

                if (PlayerInputManager.instance.isBlocking)
                {
                    attackState = AttackState.Parry;
                }
                else if (PlayerInputManager.instance.isAttacking)
                {
                    attackState = AttackState.Attack1;
                }
                break;

            case AttackState.Attack1:
                PlayGroundAttackAnimation("Attack1", AttackState.Attack2);
               
                break;

            case AttackState.Attack2:
                PlayGroundAttackAnimation("Attack2", AttackState.Idle);

                break;

            case AttackState.Parry:
                ExecuteParryState();

                break;

            case AttackState.Block:
                ExecuteBlockState();

                break;

            case AttackState.Attack3:
                break;
        }
    }

    private void HandleAirAttackCombos()
    {
        switch (attackState)
        {
            case AttackState.Idle:

                // state transitions
                currentlyAttacking = false;
                shouldCombo = false;

                if (PlayerInputManager.instance.isAttacking)
                {
                    attackState = AttackState.Attack1;
                }

                break;
            case AttackState.Attack1:
                if (!executedAttack)
                {
                    player.playerAnimator.CrossFade("AirAttack1", 0f);
                    executedAttack = true;
                    player.playerLocomotionManager.StopAllMovement(normalizedTimeThresholds["AirAttack1"].Item2);
                    currentlyAttacking = true;
                }

                if (player.playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= normalizedTimeThresholds["AirAttack1"].Item1)
                {
                    executedAttack = false;
                    attackState = AttackState.Idle;
                }
                break;
        }
    }

    private void PlayGroundAttackAnimation(string attackName, AttackState nextState, bool stopPlayerMovement = true)
    {
        if (!executedAttack)
        {
            player.playerAnimator.CrossFade(attackName, 0f);
            executedAttack = true;
            if (stopPlayerMovement)
            {
                player.playerLocomotionManager.StopAllMovement(normalizedTimeThresholds[attackName].Item2);
            }
            currentlyAttacking = true;
        }

        if (PlayerInputManager.instance.isAttacking && player.playerAnimator.GetFloat("AttackWindowOpen") > 0f)
        {
            shouldCombo = true;
        }

        if (player.playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= normalizedTimeThresholds[attackName].Item1 && PlayerInputManager.instance.isBlocking)
        {
            executedAttack = false;
            attackState = AttackState.Parry;
        }
        else if (shouldCombo && player.playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= normalizedTimeThresholds[attackName].Item1)
        {
            executedAttack = false;
            attackState = nextState;

        }
        else if (player.playerAnimator.GetFloat("AttackWindowOpen") == 0f && player.playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= normalizedTimeThresholds[attackName].Item1)
        {
            executedAttack = false;
            attackState = AttackState.Idle;

        }
    }

    private void ExecuteParryState()
    {
        if (!PlayerInputManager.instance.isBlocking)
        {
            player.playerLocomotionManager.StopAllMovement(-1);
            player.playerAnimator.SetBool("isBlocking", false);
            parryWindowSet = false;
            attackState = AttackState.Idle;
        }
        else if (!parryWindowSet)
        {
            player.playerLocomotionManager.StopAllMovement(Mathf.Infinity);
            player.playerAnimator.SetBool("isBlocking", true);
            parryWindow = parryOpeningTime;
            parryWindowSet = true;
        }

        if (parryWindow <= 0f)
        {
            parryWindowSet = false;
            attackState = AttackState.Block;
        }

        parryWindow -= Time.deltaTime;
    }

    private void ExecuteBlockState()
    {
        if (!PlayerInputManager.instance.isBlocking)
        {
            player.playerLocomotionManager.StopAllMovement(-1);
            player.playerAnimator.SetBool("isBlocking", false);
            attackState = AttackState.Idle;
        }
        else
        {
            player.playerLocomotionManager.StopAllMovement(Mathf.Infinity);
            player.playerAnimator.SetBool("isBlocking", true);
        }
    }


}
