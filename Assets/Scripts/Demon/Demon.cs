using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class Demon   : MonoBehaviour
{
    public Animator animator;  // Single Animator to handle all layers

    public CampController controller;
    public Transform player; // Assign the player in the Inspector
    public NavMeshAgent agent; // Reference to NavMeshAgent
    public Transform pointA, pointB;   // Patrol points
    private Transform currentTarget;   // Current patrol target
    private float detectionRange = 10f; // Range to start chasing
    private float attackRange = 0.5f;     // Range to start attacking

    public bool patrol = true;
    public bool chase = false;
    private bool isPatrolling = true;
    private bool isChasing = false;
    private bool isAttacking = false;
    private bool inAttackRange = false;
    private bool gettingHit = false;
    public bool isDead = false;

    public GameObject explosivePrefab; // Assign the explosive prefab
    public Transform throwPoint;       // Position from which the explosive is thrown
    public float throwForce = 10f;     // Force of the throw
    public GameObject sword; // Assign the sword GameObject in the Inspector

    private GameObject currentExplosive;
    private float patrolSwitchDistance = 0.5f; // Distance to consider as "reached" the point

    // Track the current attack state and progress
    private string currentAttackState = ""; // Tracks the current attack state name
    private float currentAttackProgress = 0f; // Tracks the progress in the attack sequence
    private bool inClone = false;
    HealthSystemForDummies healthSystem;

    public int health = 80;

    void Start()
    {
        healthSystem = GetComponent<HealthSystemForDummies>();
        currentTarget = pointA; // Start patrolling at point A
        agent.stoppingDistance = patrolSwitchDistance; // Ensure stopping distance matches patrol logic
        StartPatrolling();
    }

    void Update()
    {
        if (!gettingHit && !isDead)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Chasing logic
            if (chase && !inAttackRange)
            {
                StopPatrolling();
                StopAttacking(); // Ensure the attack is stopped
                StartChasing();
            }
            else if (patrol && !isPatrolling)
            {
                StopChasing();
                StartPatrolling();
            }

            // Attacking logic
            if (distanceToPlayer <= attackRange && !patrol && chase)
            {
                StopChasing();
                inAttackRange = true;
            }
            else if (distanceToPlayer > attackRange)
            {
                inAttackRange = false;
            }

            if (inAttackRange && !patrol && chase)
            {
                RotateTowardsPlayer(); // Rotate towards player while in attack range
            }

            if (inAttackRange && !isAttacking && !patrol && chase)
            {
                isAttacking = true;
                StartAttacking();
            }

            // Update patrolling logic
            PatrolUpdate();
        }
    }

    private void StartPatrolling()
    {
        isPatrolling = true;
        agent.isStopped = false;
        SetLayerWeight(0, 1); // Enable Base Layer (for chasing)
        SetLayerWeight(1, 0); // Disable Attack Layer
        SetLayerWeight(2, 0); // Disable Interrupt Layer (no getting hit or dying while chasing)
        animator.SetBool("isPatrolling", true); // Play the patrolling animation on Base Layer
        MoveToNextPatrolPoint();
    }

    private void StopPatrolling()
    {
        isPatrolling = false;
        agent.isStopped = true;
        SetLayerWeight(0, 0); // Disable Base Layer (no chasing or patrolling)
        animator.SetBool("isPatrolling", false);
    }

    private void MoveToNextPatrolPoint()
    {
        if (isPatrolling)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;

            if (currentTarget != null)
            {
                agent.SetDestination(currentTarget.position);
            }
        }
    }

    private void PatrolUpdate()
    {
        if (isPatrolling && !agent.pathPending && agent.remainingDistance <= patrolSwitchDistance)
        {
            MoveToNextPatrolPoint();
        }
    }


    private void StartChasing()
    {
        isChasing = true;
        agent.isStopped = false;
        agent.SetDestination(player.position);

        // Play the chasing animation in the Base Layer
        SetLayerWeight(0, 1); // Enable Base Layer (for chasing)
        SetLayerWeight(1, 0); // Disable Attack Layer
        SetLayerWeight(2, 0); // Disable Interrupt Layer (no getting hit or dying while chasing)

        // Play the chasing animation in the Interrupt Layer
        animator.SetBool("isChasing", true);
    }

    private void StopChasing()
    {
        isChasing = false;
        agent.isStopped = true;

        // Stop the chasing animation
        SetLayerWeight(0, 0); // Disable Base Layer (no chasing or patrolling)
        animator.SetBool("isChasing", false);
    }

    private void StartAttacking()
    {
        isAttacking = true;

        // Stop any chasing animation if it's playing
        animator.SetBool("isChasing", false); // Stop the chasing animation in the Interrupt Layer

        // Enable Attack Layer for attack animations
        SetLayerWeight(1, 1); // Enable Attack Layer
        SetLayerWeight(0, 0); // Disable Base Layer (no chasing or patrolling during attack)
        SetLayerWeight(2, 0); // Disable Interrupt Layer (no getting hit or dying during attack)

        // If there's no saved attack state, we can start fresh from the first attack animation
        if (string.IsNullOrEmpty(currentAttackState))
        {
            // Start from the first attack animation (e.g., Slash)
            animator.Play("Slash", 1, 0f); // Play "Slash" in the Attack Layer from the beginning
            currentAttackState = "Slash";  // Save the current attack state
            currentAttackProgress = 0f;    // Reset progress
        }
        else
        {
            if (currentAttackProgress <= 0.5f)
            {
                // Resume from start
                animator.Play(currentAttackState, 1, 0f); // Resume from the last saved attack state
            }
            else
            {
                // Resume from where it was interrupted
                animator.Play(currentAttackState, 1, currentAttackProgress); // Resume from the last saved attack state and progress
            }
        }
    }


    private void StopAttacking()
    {
        isAttacking = false;

        // Save current attack state and progress
        var stateInfo = animator.GetCurrentAnimatorStateInfo(1); // Attack Layer
        currentAttackState = stateInfo.IsName("Slash") ? "Slash" : stateInfo.IsName("Wait") ? "Wait" : stateInfo.IsName("Slash 0") ? "Slash 0" : stateInfo.IsName("Sheath") ? "Sheath" : stateInfo.IsName("Throw") ? "Throw" : stateInfo.IsName("Unsheath") ? "Unsheath" : "";
        currentAttackProgress = stateInfo.normalizedTime;

        // Pause the Attack Layer
        SetLayerWeight(1, 0);
    }

    private void SetLayerWeight(int layerIndex, float weight)
    {
        animator.SetLayerWeight(layerIndex, weight);
    }

    public void SheathSword()
    {
        if (sword != null) // Check if the sword is currently active
        {
            sword.SetActive(false); // Sheath the sword (hide it)
        }
    }

    public void UnSheathSword()
    {
        if (sword != null) // Check if the sword is currently active
        {
            sword.SetActive(true); // Unsheath the sword (show it)
        }
    }

    public void SpawnExplosive()
    {
        currentExplosive = Instantiate(explosivePrefab, throwPoint.position, throwPoint.rotation);
        currentExplosive.transform.SetParent(throwPoint); // Attach it to the hand temporarily
    }

    public void ReleaseExplosive()
    {
        if (currentExplosive != null && currentExplosive.gameObject.activeInHierarchy)
        {
            currentExplosive.transform.SetParent(null); // Detach from hand

            // Add Rigidbody to apply physics
            Rigidbody rb = currentExplosive.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
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

    public void isHit(int dmg)
    {
        if (health - dmg <= 0)
        {
            // Kills the enemy
            healthSystem.Kill();
            health = 0;
            die();
        }
        else
        {
            Debug.Log("A5ad Damage");
            // Damage enemy for -100 units 
            healthSystem.AddToCurrentHealth(-dmg);
            health = health - dmg;
            if (!gettingHit)
            {
                gettingHit = true;
                StopAttacking();
                StopChasing();
                StopPatrolling();

                animator.SetBool("isHit", true);
                // Play the hit animation in the Interrupt Layer
                SetLayerWeight(0, 0); // Disable Base Layer (for chasing)
                SetLayerWeight(1, 0); // Disable Attack Layer
                SetLayerWeight(2, 1); // Enable Interrupt Layer for hit animation
            }
        }
    }

    // Called via Animation Event at the end of the "hit" animation
    public void ResetHit()
    {
        gettingHit = false;
        animator.SetBool("isHit", false);

        // Resume attacking or chasing based on current state
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            StartAttacking();
        }
        else if (Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            StartChasing();
        }
        else
        {
            StartPatrolling();
        }
    }

    public void die()
    {
        isDead = true;
        // Kills the enemy
        healthSystem.Kill();
        SetLayerWeight(0, 0); // Disable Base Layer (for chasing)
        SetLayerWeight(1, 0); // Disable Attack Layer
        SetLayerWeight(2, 1); // Enable Interrupt Layer for death animation
        StopAttacking();
        StopChasing();
        StopPatrolling();
        animator.SetBool("isDead", true); // Play death animation in the Interrupt Layer
        controller.HandleDemonDeath(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Clone" && inClone == false)
        {
            inClone = true;
            StartCoroutine(TakeExplosionDamage(2f));
        }
    }

    private IEnumerator TakeExplosionDamage(float delay)
    {
        // Wait for the specified time
        yield return new WaitForSeconds(delay);
        isHit(10);
        inClone = false;
    }

}
