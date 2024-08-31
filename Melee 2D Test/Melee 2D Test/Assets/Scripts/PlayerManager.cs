using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

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

    [Header("Invulnerable")]
    public bool isInvulnerable = false;
    public Coroutine invulnerableState;

    [Header("Stop Movement and Input Flags")]
    public bool movementAndInputStopped = false;
    public List<string> validInputStopAnimations = new();
    public List<string> validMovementStopAnimations = new();
    AnimatorClipInfo[] clipInfo;

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

    }

    // Update is called once per frame
    void Update()
    {
        if (playerAnimator.GetFloat("invulnerabilityActive") > 0f)
        {
            isInvulnerable = true;
        }
        else
        {
            isInvulnerable = false;
        }

        clipInfo = playerAnimator.GetCurrentAnimatorClipInfo(0);

        preventInputDuringAnimation();
        preventMovementDuringAnimation();


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
        else
        {
            playerAnimator.Play("PlayerHit", 0);
            playerAnimationManager.HandleTakeHitAnimation();
            Debug.Log(isInvulnerable);
        }


    }

   /* public void CheckForStopMovementAndInput()
    {
        // Get the current animation clip info
        AnimatorClipInfo[] clipInfo = playerAnimator.GetCurrentAnimatorClipInfo(0);

        if (clipInfo.Length > 0)
        {
            string currentClipName = clipInfo[0].clip.name; 

            if (validAnimatorNames.Contains(currentClipName))
            {
                if (playerAnimator.GetFloat("stopMovementAndInputActive") > 0f)
                {
                    PlayerInputManager.instance.PauseInputs(Mathf.Infinity);
                    playerLocomotionManager.StopAllMovement(Mathf.Infinity);
                    movementAndInputStopped = true;
                }
                else
                {
                    PlayerInputManager.instance.PauseInputs(-1);
                    playerLocomotionManager.StopAllMovement(-1);
                    movementAndInputStopped = false;
                }
            }
            else
            {
                if (movementAndInputStopped) 
                {
                    PlayerInputManager.instance.PauseInputs(-1);
                    playerLocomotionManager.StopAllMovement(-1);
                    movementAndInputStopped = false;
                }
            }
        }
    }*/

    public void preventInputDuringAnimation()
    {
        

        if (clipInfo.Length > 0)
        {
            string currentClipName = clipInfo[0].clip.name;
            if (validInputStopAnimations.Contains(currentClipName))
            {
                if (playerAnimator.GetFloat("stopInput") > 0f)
                {
                    PlayerInputManager.instance.PauseInputs(Mathf.Infinity);
                    inputStopped = true;
                }
                else
                {
                    PlayerInputManager.instance.PauseInputs(-1);
                    inputStopped = false;
                }
            }
            else
            {
                if (inputStopped)
                {
                    PlayerInputManager.instance.PauseInputs(-1);
                    inputStopped = false;
                }
            }
        }
    }

    public void preventMovementDuringAnimation()
    {

        if (clipInfo.Length > 0)
        {
            string currentClipName = clipInfo[0].clip.name;
            if (validMovementStopAnimations.Contains(currentClipName))
            {
                if (playerAnimator.GetFloat("stopMovement") > 0f)
                {
                    playerLocomotionManager.StopAllMovement(Mathf.Infinity);
                    movementStopped = true;
                }
                else
                {
                    playerLocomotionManager.StopAllMovement(-1);
                    movementStopped = false;
                }
            }
            else
            {
                if (movementStopped)
                {
                    playerLocomotionManager.StopAllMovement(-1);
                    movementStopped = false;
                }
            }
        }
    }

    /*public void SetInvulnerability(float time)
    {
        if (invulnerableState != null)
        {
            StopCoroutine(invulnerableState);
            isInvulnerable = false;
        }

        invulnerableState = StartCoroutine(ResetInvulnerability(time));
    }

    public IEnumerator ResetInvulnerability(float time)
    {
        yield return new WaitForSeconds(time);
        isInvulnerable = true;
    }*/
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);
    }

}
