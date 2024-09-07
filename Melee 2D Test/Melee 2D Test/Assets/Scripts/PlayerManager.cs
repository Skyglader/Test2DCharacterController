using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerManager : MonoBehaviour
{
    [Header("Component References")]
    public Rigidbody2D rb;
    public GameObject playerModel; // assign in inspector
    public Animator playerAnimator;
    public PlayerLocomotionManager playerLocomotionManager;
    public PlayerAnimationManager playerAnimationManager;
    public PlayerCombatManager playerCombatManager;
    public PlayerParticleStorage playerParticleStorage;
    public Collider2D swordCollider; // assign in inspector

    [Header("Ground Check")]
    public bool isGrounded;
    public Vector2 groundCheckOffset;
    public float groundCheckRadius;
    public LayerMask whatIsGround;
    public float gravityScale;

    [Header("Invulnerable")]
    public bool isInvulnerable = false;
    public Coroutine invulnerableState;

    [Header("Layers")]
    public LayerMask whatIsEnemy;

    [Header("Attacking")]
    public List<Collider2D> collidersDamaged;
    public bool canHitStop = false;
    public float hitStopDuration = 0.04f;

    [Header("Stop Movement and Input Flags")]
    public bool movementAndInputStopped = false;
    public List<string> validInputStopAnimations = new();
    public List<string> validMovementStopAnimations = new();
    public AnimatorClipInfo[] clipInfo;
    public bool inputStopped = false;
    public bool movementStopped = false;

    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponentInChildren<Animator>();
        playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        playerAnimationManager = GetComponentInChildren<PlayerAnimationManager>();
        playerCombatManager = GetComponent<PlayerCombatManager>();
        playerParticleStorage = GetComponentInChildren<PlayerParticleStorage>();
    }
    void Start()
    {
        gravityScale = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        clipInfo = playerAnimator.GetCurrentAnimatorClipInfo(0);

    }

    private void FixedUpdate()
    {
        CheckGrounded();
    }

    private void CheckGrounded()
    {
        Collider2D[] groundColliders = Physics2D.OverlapCircleAll((Vector2)transform.position + groundCheckOffset, groundCheckRadius, whatIsGround);

        isGrounded = groundColliders.Length > 0;

    }

    public void Attack()
    {
        Collider2D[] collidersToDamage = new Collider2D[10];
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.layerMask = whatIsEnemy;
        filter.useLayerMask = true;
        int colliderCount = Physics2D.OverlapCollider(swordCollider, filter, collidersToDamage);
        for (int i = 0; i < colliderCount; i++)
        {
            
            if (!collidersDamaged.Contains(collidersToDamage[i]))
            {
                Debug.Log(collidersToDamage[i].gameObject.name);
                collidersToDamage[i].gameObject.GetComponent<AIManager>().TakeDamage(this);
                collidersDamaged.Add(collidersToDamage[i]);

                if (canHitStop)
                    HitStop.instance.Stop(hitStopDuration);
            }
        }
    }

    public void TakeDamage(AIManager enemy)
    {

        if (isInvulnerable)
        {
            return;
        }
        if (playerCombatManager.attackState == PlayerCombatManager.AttackState.Parry)
        {
            Debug.Log("Attack Parried");
            playerParticleStorage.PlayParryParticle();
            playerCombatManager.parrySuccessful = true;

            if (enemy.transform.position.x < transform.position.x)
            {
                playerModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            }
            else
            {
                playerModel.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }

            CameraShake.instance.ShakeCamera(1.5f, 0.3f);
        }
        else if (playerCombatManager.attackState == PlayerCombatManager.AttackState.Block || playerCombatManager.attackState == PlayerCombatManager.AttackState.ParryFollowUp)
        {
            Debug.Log("Attack Blocked");
            playerParticleStorage.PlayBlockParticle();

            if (enemy.transform.position.x < transform.position.x)
            {
                playerModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                rb.AddForce(new Vector2(10f, 0), ForceMode2D.Impulse);
            }
            else
            {
                playerModel.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                rb.AddForce(new Vector2(-10f, 0), ForceMode2D.Impulse);
            }
        }
        else
        {
            if (playerCombatManager.inPoweredState)
                playerAnimator.Play("PoweredPlayerHit", 0);
            else
                playerAnimator.Play("PlayerHit", 0);
            playerAnimationManager.HandleTakeHitAnimation();
            Debug.Log(isInvulnerable);
        }


    }

   public Vector2 retrievePlayerFacingDirection()
    {
        if (playerModel.transform.localRotation == Quaternion.Euler(0f, 0f, 0f))
        {
            return new Vector2(1, 0);
        }
        else
        {
            return new Vector2(-1, 0);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);
    }

}
