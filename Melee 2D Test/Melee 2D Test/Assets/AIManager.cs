using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    
    public Animator animator;
    public GameObject player;

    bool playerInRange = false;
    public float validRange = 3;

    public float attackCooldown = 1.5f;
    public bool canAttack = true;

    public LayerMask whatIsPlayer;
    public float attackRadius = 3f;
    public Vector2 attackOffset;

    public GameObject enemyModel;

    public List<Collider2D> collidersDamaged;
    public Collider2D enemyCollider;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerInRange = Vector2.Distance((Vector2)transform.position, (Vector2)player.transform.position) <= validRange;

        if (player.transform.position.x < transform.position.x)
        {
            enemyModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            enemyModel.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        if (playerInRange && canAttack)
        {
            canAttack = false;
            Attack();
            StartCoroutine(resetAttack(attackCooldown));

        }

        if (animator.GetFloat("WeaponActive") > 0f)
        {
            OpenSwordCollider();
        }
        else
        {
            collidersDamaged.Clear();
        }
    }

    
   public IEnumerator resetAttack(float time)
   {
        yield return new WaitForSeconds(time);
        canAttack = true;
   }

    public void Attack()
    {
        animator.SetTrigger("Attack1");
        
    }

    void OpenSwordCollider()
    {
        Collider2D[] collidersToDamage = new Collider2D[10];
        ContactFilter2D filter = new ContactFilter2D();
        filter.layerMask = whatIsPlayer;
        int colliderCount = Physics2D.OverlapCollider(enemyCollider, filter, collidersToDamage);
        for (int i = 0; i < colliderCount; i++)
        {

            if (!collidersDamaged.Contains(collidersToDamage[i]))
            {
                Debug.Log("Hit Player");
                collidersToDamage[i].gameObject.GetComponent<PlayerManager>().TakeDamage(this);
                collidersDamaged.Add(collidersToDamage[i]);
            }
        }
    }
}
