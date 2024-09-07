using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatManager : MonoBehaviour
{
    public PlayerManager player;

    public AttackState attackState;
    public CombatState combatState;
    public bool shouldCombo = false;
    public bool executedAttack = false;
    public bool currentlyAttacking = false;

    [Header("Parry")]
    public float parryOpeningTime = 0.5f;
    public float parryWindow;
    public bool parryWindowSet;

    [Header("Parry Follow Up")]
    public bool parrySuccessful = false;
    public float parryFollowUpOpening = 0.3f;
    public float parryFollowUpTime;
    public bool parryFollowUpWindowSet = false;

    [Header("Stun Status")]
    private Coroutine stunned;

    [Header("Attack Animation Thresholds")]
    [SerializeField] Dictionary<string, float> normalizedTimeThresholds;  // Tuple<normalizedTime, timeToStopMovement>

    [Header("Demon Mode")]
    public bool inPoweredState = false;
    public bool transformTriggered = false;

    [Header("Gravitate")]
    public float gravitateDistance = 3f;
    public float minDistanceFromPlayer = 2f;
    public LayerMask whatIsEnemy;
    public float gravitateSpeed = 0.25f;
    public float distanceToStop = 2f;
    public float maxDuration = 1f;
    public float elapsedTime;
    public bool isGravitating = false;
    public enum CombatState
    {
        Air,
        Grounded
    }
    public enum AttackState
    {
        Idle,
        Stunned,
        Attack1,
        Attack2,
        Attack3,
        Parry,
        ParryFollowUp,
        Block,
        SpecialAttack,
    }


    private void Awake()
    {
        player = GetComponent<PlayerManager>();
    }

    private void Start()
    {
        attackState = AttackState.Idle;


        // manually create attack timings
        normalizedTimeThresholds = new Dictionary<string, float>
        {
            { "Attack1", 0.7f},
            { "Attack2", 0.72f},
            { "Attack3", 0.85f},
            { "AirAttack1", 0.8f},
            {"SpecialAttack1", 0.95f},

            { "PoweredAttack1", 0.6f},
            { "PoweredAttack2", 0.72f},
            { "PoweredAttack3", 0.85f},
            { "PoweredAirAttack1", 0.8f},
            {"PoweredSpecialAttack1", 0.8f}
        };
    }
    private void Update()
    {
        // powered state transformation
        if (PlayerInputManager.instance.isTransforming && player.isGrounded && !transformTriggered)
        {
            Debug.Log("transform called");
            StartCoroutine(triggerPlayerTransformationState());
            transformTriggered = true;
        }

        // change states based on position in environment
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

        if (player.movementStopped && currentlyAttacking && player.playerAnimationManager.isHit)
        {
            attackState = AttackState.Idle;
        }

    }

    private void HandleGroundAttackCombos()
    {
        switch (attackState)
        {
            case AttackState.Idle:

                // state transitions
                currentlyAttacking = false;
                executedAttack = false;
                shouldCombo = false;
                isGravitating = false;

                if (PlayerInputManager.instance.isBlocking)
                {
                    attackState = AttackState.Parry;
                }
                else if (PlayerInputManager.instance.isAttacking)
                {
                    attackState = AttackState.Attack1;
                }
                else if (PlayerInputManager.instance.isSpecialAttacking)
                {
                    attackState = AttackState.SpecialAttack;
                }
                break;

            case AttackState.Stunned:
                break;

            case AttackState.Attack1:
                if (inPoweredState)
                {
                    if (!isGravitating)
                    {
                        isGravitating = true;
                        gravitateTowardsEnemy();
                    }
                    PlayGroundAttackAnimation("PoweredAttack1", AttackState.Attack2);
                }

                else
                    PlayGroundAttackAnimation("Attack1", AttackState.Attack2);

                break;

            case AttackState.Attack2:
                if (inPoweredState)
                    PlayGroundAttackAnimation("PoweredAttack2", AttackState.Idle);
                else
                    PlayGroundAttackAnimation("Attack2", AttackState.Idle);

                break;

            case AttackState.Attack3:
                if (inPoweredState)
                    PlayGroundAttackAnimation("PoweredAttack3", AttackState.Idle);
                else
                    PlayGroundAttackAnimation("Attack3", AttackState.Idle);
                break;

            case AttackState.SpecialAttack:
                if (inPoweredState)
                    PlayGroundAttackAnimation("PoweredSpecialAttack1", AttackState.Idle);
                else
                    PlayGroundAttackAnimation("SpecialAttack1", AttackState.Idle);

                break;

            case AttackState.Parry:
                ExecuteParryState();
                break;

            case AttackState.ParryFollowUp:
                ExecuteParryFollowUpState();
                break;

            case AttackState.Block:
                ExecuteBlockState();
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
                if (!inPoweredState)
                    PlayAirAttackAnimation("AirAttack1");
                break;
        }
    }

    private void PlayGroundAttackAnimation(string attackName, AttackState nextState, bool stopPlayerMovement = true)
    {
        if (!executedAttack)
        {
            player.playerAnimator.CrossFade(attackName, 0f);
            executedAttack = true;
            currentlyAttacking = true;
        }

        if (PlayerInputManager.instance.isAttacking && player.playerAnimator.GetFloat("AttackWindowOpen") > 0f)
        {
            shouldCombo = true;
        }

        if (PlayerInputManager.instance.isBlocking)
        {
            executedAttack = false;
            attackState = AttackState.Parry;
        }
        else if (shouldCombo && player.playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= normalizedTimeThresholds[attackName])
        {
            executedAttack = false;
            attackState = nextState;

        }
        else if ( player.playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= normalizedTimeThresholds[attackName])
        {
            executedAttack = false;
            attackState = AttackState.Idle;

        }
    }

    private void PlayAirAttackAnimation(string attackName)
    {
        if (!executedAttack)
        {
            player.playerAnimator.CrossFade(attackName, 0f);
            executedAttack = true;
            currentlyAttacking = true;
        }

        if (player.playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= normalizedTimeThresholds[attackName])
        {
            executedAttack = false;
            attackState = AttackState.Idle;
        }
    }

    private void ExecuteParryState()
    {
        if (!PlayerInputManager.instance.isBlocking)
        {
            //player.playerLocomotionManager.StopAllMovement(-1);
            player.playerAnimator.SetBool("isBlocking", false);
            parryWindowSet = false;
            attackState = AttackState.Idle;
        }
        else if (!parryWindowSet)
        {
            //player.playerLocomotionManager.StopAllMovement(Mathf.Infinity);
            player.playerAnimator.SetBool("isBlocking", true);
            parryWindow = parryOpeningTime;
            parryWindowSet = true;
        }

        if (parrySuccessful)
        {
            //player.playerLocomotionManager.StopAllMovement(-1);
            parrySuccessful = false;
            parryWindowSet = false;
            attackState = AttackState.ParryFollowUp;
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
            player.playerAnimator.SetBool("isBlocking", false);
            attackState = AttackState.Idle;
        }
        else
        {
            player.playerAnimator.SetBool("isBlocking", true);
        }
    }

    private void ExecuteParryFollowUpState()
    {
        if (!parryFollowUpWindowSet)
        {
            parryFollowUpTime = parryFollowUpOpening;
            parryFollowUpWindowSet = true;
        }

        if (!PlayerInputManager.instance.isBlocking)
        {
            player.playerAnimator.SetBool("isBlocking", false);
        }
        else
        {
            //player.playerLocomotionManager.StopAllMovement(Mathf.Infinity);
        }

        if (PlayerInputManager.instance.isAttacking)
        {
            player.playerAnimator.SetBool("isBlocking", false);
            parryFollowUpWindowSet = false;

            attackState = AttackState.Attack3;
        }

        if (parryFollowUpTime <= 0)
        {
            parryFollowUpWindowSet = false;
            player.playerAnimator.SetBool("isBlocking", false);
            attackState = AttackState.Idle;
        }

        parryFollowUpTime -= Time.deltaTime;
    }

    public IEnumerator triggerPlayerTransformationState()
    {
        if (!inPoweredState)
        {
            inPoweredState = true;
            player.playerAnimator.SetTrigger("isTransforming");
            player.playerAnimator.SetBool("inPoweredState", true);
        }
        else
        {
            inPoweredState = false;
            player.playerAnimator.SetBool("inPoweredState", false);
        }

        yield return new WaitForSeconds(1f) ;

        transformTriggered = false;
        attackState = AttackState.Idle;
    }

    public void gravitateTowardsEnemy()
    {

        RaycastHit2D hit = (Physics2D.Raycast(transform.position, player.retrievePlayerFacingDirection(), gravitateDistance, whatIsEnemy));

        if (hit.collider != null && Mathf.Abs(transform.position.x - hit.collider.transform.position.x) >= minDistanceFromPlayer)
        {
            StartCoroutine(forcePlayerToEnemy(hit.collider));
        }
        else
            Debug.Log("enemy null");
        

        
    }

    public IEnumerator forcePlayerToEnemy(Collider2D hit)
    {
        Vector2 enemyPosition = hit.transform.position;
        elapsedTime = 0f;

        while (Mathf.Abs(transform.position.x - enemyPosition.x) > distanceToStop && elapsedTime < maxDuration)
            {
            //transform.position = new Vector2(Mathf.Lerp(transform.position.x, enemyPosition.x, gravitateSpeed * Time.deltaTime), transform.position.y);
            transform.position = new Vector2(Mathf.MoveTowards(transform.position.x, enemyPosition.x, gravitateSpeed * Time.deltaTime), transform.position.y);

            elapsedTime += Time.deltaTime;
 
             yield return null;
        }
        isGravitating = false;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector2 direction = player.retrievePlayerFacingDirection();
        Vector2 endPosition = (Vector2)transform.position + direction * gravitateDistance;

        Gizmos.DrawLine(transform.position, endPosition);
    }


}



        
