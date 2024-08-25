using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Rigidbody2D rb;
    public GameObject playerModel; // assign in inspector
    public Animator playerAnimator;
    public PlayerLocomotionManager playerLocomotionManager;
    public PlayerAnimationManager playerAnimationManager;
    public PlayerCombatManager playerCombatManager;
    public PlayerParticleStorage playerParticleStorage;
    [Header("Ground Check")]
    public bool isGrounded;
    public Vector2 groundCheckOffset;
    public float groundCheckRadius;
    public LayerMask whatIsGround;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void TakeDamage(AIManager enemy)
    {
        if (playerCombatManager.attackState == PlayerCombatManager.AttackState.Parry)
        {
            Debug.Log("Attack Parried");
            playerParticleStorage.PlayParryParticle();

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
        else if (playerCombatManager.attackState == PlayerCombatManager.AttackState.Block)
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


    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);
    }

}
