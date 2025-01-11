using System.Collections;
using UnityEngine;

public class MinionBehavior : MonoBehaviour
{
    public Transform player; // Player reference
    public float detectionRange = 30f; // Range for detection
    private float attackRange = 0.7f; // Range for attack
    public float attackCooldown = 2f; // Cooldown between attacks in seconds
    public float health = 20f; // Minion health
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent; // Navigation agent
    private float attackTimer = 0f;
    private bool isAlerted = false; // Tracks if the minion is alerted
    private Vector3 initialPosition; // Initial position of the minion
    private bool isReturning = false; // Tracks if the minion is returning to camp
    private bool inClone = false;
    public bool isDead = false;
    public CampController controller;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.stoppingDistance = 0.7f;
        initialPosition = transform.position; // Save the initial position

        if (player == null)
        {
            player = GameObject.Find("Sphere").transform; // Replace "Sphere" with your player GameObject's name
        }
    }

    void Update()
    {
        if (health <= 0)
        {
            Die();
            return;
        }

        if (isReturning)
        {
            // Return to camp
            float distanceToCamp = Vector3.Distance(transform.position, initialPosition);
            if (distanceToCamp <= 1f)
            {
                ResetToIdle();
            }
            else
            {
                agent.SetDestination(initialPosition);
                animator.SetBool("isRunning", true);
            }
            return;
        }

        if (isAlerted)
        {
            float distanceToPlayer = Mathf.Abs(transform.position.x - player.position.x);
                if (distanceToPlayer <= attackRange)
            {
                RotateTowardsPlayer(); // Rotate towards player while in attack range
                // Attack the player
                if (attackTimer <= 0)
                {
                    Attack();
                }
            }
            else if (distanceToPlayer <= detectionRange)
            {
                // Chase the player
                agent.SetDestination(player.position);
                animator.SetBool("isRunning", true);
            }
            else
            {
                // Stop chasing and return to camp
                isAlerted = false;
                isReturning = true;
            }
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Keep rotation in the horizontal plane
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Apply an offset to adjust the slash direction
        float offsetAngle = 30f; // Adjust this angle based on your animation's skew
        lookRotation *= Quaternion.Euler(0, offsetAngle, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void Attack()
    {
        animator.SetBool("isRunning", false);
        animator.SetBool("isAttacking", true);
        attackTimer = attackCooldown;
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        animator.SetBool("isAttacking", false);
        attackTimer = 0f;
    }

    public void Alert(Transform playerTransform)
    {
        if (!isAlerted)
        {
            isAlerted = true;
            isReturning = false;
            player = playerTransform; // Update the player reference
            animator.SetBool("isRunning", true);
        }
    }

    public void ResetToIdle()
    {
        isAlerted = false;
        isReturning = false;
        agent.SetDestination(initialPosition);
        animator.SetBool("isRunning", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isIdle", true); // Assuming you have an "isIdle" animation state
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        else
        {
            animator.SetBool("isAttacked", true);
            StartCoroutine(ResetAttacked());
        }
    }

    private IEnumerator ResetAttacked()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("isAttacked", false);
    }

    private void Die()
    {
        animator.SetBool("isDead", true);
        agent.isStopped = true;
        isDead = true;
        controller.HandleMinionDeath(this);
        Destroy(gameObject, 3f); // Destroy the object after 2 seconds
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Clone" && inClone == false)
        {
            inClone = true;
            StartCoroutine(TakeExplosionDamage(2f));
        }
    }

    private IEnumerator TakeExplosionDamage(float delay)
    {
        // Wait for the specified time
        yield return new WaitForSeconds(delay);
        TakeDamage(10);
        inClone = false;
    }
}
