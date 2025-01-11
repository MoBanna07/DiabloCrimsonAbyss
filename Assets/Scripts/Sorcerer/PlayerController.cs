using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using UnityEditor.Animations;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    const string IDLE = "Standing";
    const string WALK = "walking";
    const string APPEAR = "Appearing";
    const string DISAPPEAR = "Disappearing";
    const string SHOOT = "gooooo";
    const string HIT = "Hit";
    const string DYING = "dying";

    // Character Stats
    public int currentHealth;
    public int maxHealth;
    public int currentLevel;
    public int maxXP;
    public int currentXP;
    public int abilityPoints;
    public int potions;
    public int Runes;

    GameObject activeParticle;
    GameObject selectedEnemy; // The currently selected enemy
    Vector3 fireballTarget;   // The target position for the fireball

    const int MaxPotions = 3;

    CustomActions input;

    NavMeshAgent agent;
    Animator animator;

    [Header("Movement")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;

    [Header("Abilities")]
    [SerializeField] GameObject fireballEffect;
    [SerializeField] ParticleSystem teleportEffect;
    [SerializeField] GameObject clonePrefab;
    [SerializeField] ParticleSystem infernoEffect;

    [SerializeField] public float fireballCooldown = 1f;
    [SerializeField] public float teleportCooldown = 10f;
    [SerializeField] public float cloneCooldown = 10f;
    [SerializeField] public float infernoCooldown = 15f;

    [SerializeField] KeyCode fireballKey = KeyCode.Alpha1;
    [SerializeField] KeyCode teleportKey = KeyCode.Alpha2;
    [SerializeField] KeyCode cloneKey = KeyCode.Alpha3;
    [SerializeField] KeyCode infernoKey = KeyCode.Alpha4;
    [SerializeField] KeyCode potionKey = KeyCode.F;

    float fireballTimer;
    float teleportTimer;
    float cloneTimer;
    float infernoTimer;
    public bool isInvincible = false;
    public bool unlock_abilities = false; 
    float lookRotationSpeed = 8f;
    bool isFireballTargeting;
    bool isTeleportTargeting;
    bool isCloneTargeting;
    bool isInfernoTargeting;
    private bool isHitAnimating = false; // Prevent overlapping animations
    public bool isDying = false; // To prevent multiple triggers
    private bool isAbilityActive = false; // Prevent multiple abilities being active at once
    bool drinkPotion = false;
    private bool isPaused = false;
    public GameObject pauseMenu; // Assign this in the Unity Editor
    public GameObject Gameover_scene; // Assign this in the Unity Editor

    public List<CampController> camps;

    IEnumerator PlayHitAnimation(int damage, float delay = 0)
    {
        if(isDying)
            yield break;

        if (isHitAnimating)
            yield break; // Skip if already animating

        isHitAnimating = true; // Lock animation
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        animator.SetLayerWeight(1, 1); // Activate hit layer
        animator.SetTrigger(HIT);     // Trigger the hit animation

        // Wait for animation duration
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(1).length);

        TakeDamage(damage); // Apply damage after animation
        if (isDying)
            animator.SetLayerWeight(1, 1);
        else
            animator.SetLayerWeight(1, 0); // Deactivate hit layer
        isHitAnimating = false; // Unlock animation
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        currentLevel = 1;
        maxXP = 100 * currentLevel;
        currentXP = 0;
        abilityPoints = 0;
        potions = 0;
        maxHealth = 100;
        currentHealth = maxHealth;
        Runes = 0;
        input = new CustomActions();
        AssignInputs();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Health Potion") && potions < MaxPotions)
        {
            Debug.Log("Collided with a Health Potion!");

            int healAmount = Mathf.Min(maxHealth / 2, maxHealth - currentHealth);
            currentHealth += healAmount;

            Debug.Log($"Health after: {currentHealth}");
            Destroy(other.gameObject);

            CollectPotion();
        }
        else if (other.CompareTag("Punch Player")&& !isInvincible)
        {
            StartCoroutine(PlayHitAnimation(5));
        }
        else if (other.CompareTag("Attack Player") && !isInvincible)
        {
            StartCoroutine(PlayHitAnimation(10));
        }
        else if (other.CompareTag("Bomb Player") && !isInvincible)
        {
            StartCoroutine(PlayHitAnimation(15));
        }
        if (other.CompareTag("Rune") )
        {
            Debug.Log("Collided with a Rune!");
            Runes ++;
            Destroy(other.gameObject);

        }
    }


    

    public void TakeDamage(int damage)
    {
        if (isDying)
            return; // Ignore damage if already dying

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0; // Prevent negative health
            StartCoroutine(PlayDyingAnimation());
        }
    }

    IEnumerator PlayDyingAnimation()
    {
        if (isDying)
            yield break; // Prevent multiple triggers

        isDying = true; // Lock dying state
        animator.SetLayerWeight(1, 1); // Activate dying layer
        animator.SetTrigger(DYING);   // Trigger the dying animation

        // Wait for animation duration
        float dyingAnimDuration = GetAnimationClipLength(DYING);
        yield return new WaitForSeconds(dyingAnimDuration);

        Debug.Log("Game Over!");
        // Implement Game Over logic here
    }



    private float GetAnimationClipLength(string clipName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        Debug.LogWarning($"Animation clip {clipName} not found!");
        return 1f; // Default fallback length
    }


    void Update()
    {
        HandleTimers();
        HandleAbilities();
        if (currentHealth <= 0)
        {
            GameOver();
        }
        // Handle pause/resume and close UI screens
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
            return; // Exit Update to prevent other actions during the same frame
        }

        if (isDying)
        {
            return;
        }

        // Left-click for walking only
        if (Input.GetMouseButtonDown(0) && !isDying)
        {
            if (!isAbilityActive)
            {
                ClickToMove();
            }
        }

        if (Input.GetKeyDown(potionKey) && !isAbilityActive && !isDying)
        {
            drinkPotion = true;
            UsePotion();
        }

        // Right-click for using abilities
        if (Input.GetMouseButtonDown(1) && !isDying)
        {
            if (!isAbilityActive && !drinkPotion) // Only allow selecting an ability if none is active
            {
                if (isFireballTargeting)
                {
                    SetFireballTarget();
                    isAbilityActive = true; // Lock until the ability is used
                }
                else if (isTeleportTargeting)
                {
                    SetTeleportTarget();
                    isAbilityActive = true; // Lock until the ability is used
                }
                else if (isCloneTargeting)
                {
                    SetCloneTarget();
                    isAbilityActive = true; // Lock until the ability is used
                }
                else if (isInfernoTargeting)
                {
                    SetInfernoTarget();
                    isAbilityActive = true; // Lock until the ability is used
                }
            }
        }

        // Reset `isAbilityActive` after using the selected ability
        if (HasReachedDestination() && !isDying)
        {
            StopMovement();

            if (isAbilityActive)
            {
                isAbilityActive = false; // Unlock after ability is executed
            }
        }
        else
        {
            FaceTargetSmoothly(); // Use the updated rotation logic
            SetAnimations();
        }
    }


    // Method to toggle pause state
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0; // Freeze the game
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true); // Show the pause menu
            }
        }
        else
        {
            Time.timeScale = 1; // Resume the game
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false); // Hide the pause menu
            }
        }
    }

    IEnumerator HandleFireball()
    {

        if (selectedEnemy == null)
        {
            Debug.LogWarning("No enemy selected or fireball target invalid!");
            yield break;
        }

        // Smoothly rotate the player to face the enemy
        Vector3 directionToEnemy = (selectedEnemy.transform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToEnemy.x, 0, directionToEnemy.z));

        while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Adjust rotation speed if needed
        }

        animator.SetLayerWeight(1, 1);
        Debug.Log("Casting Fireball");
        animator.Play(SHOOT, 1);

        // Instantiate the fireball prefab
        GameObject fireball = Instantiate(
            fireballEffect,
            transform.position + Vector3.up * 0.5f, // Adjust the height if necessary
            Quaternion.identity
        );

        if (fireball == null)
        {
            Debug.LogError("Fireball prefab instantiation failed!");
            yield break;
        }

        Debug.Log($"Fireball instantiated at {fireball.transform.position}");

        // Rotate the fireball to face the target
        Vector3 fireballDirection = (fireballTarget - fireball.transform.position).normalized;
        fireball.transform.rotation = Quaternion.LookRotation(new Vector3(fireballDirection.x, 0, fireballDirection.z));

        // Move fireball toward the target position
        while (fireball != null && Vector3.Distance(fireball.transform.position, fireballTarget) > 0.1f)
        {
            fireball.transform.position = Vector3.MoveTowards(
                fireball.transform.position,
                fireballTarget,
                3f * Time.deltaTime // Adjust speed as needed
            );
            yield return null;
        }

        // Fireball reached the target
        Destroy(fireball);
        Debug.Log("Fireball reached target and is destroyed.");

        animator.SetLayerWeight(1,0);

        // Reset state
        selectedEnemy = null;
        fireballTimer = fireballCooldown; // Reset cooldown

    }




    void AssignInputs()
    {
        input.MainMenu.Move.performed += ctx => ClickToMove();
    }


    void OnEnable()
    {
       input.Enable();
        
    }

    void OnDisable()
    {
        if (input != null)
        {
            input.Disable();
        }
    }



    void HandleTimers()
    {
        if (fireballTimer > 0) fireballTimer -= Time.deltaTime;
        if (teleportTimer > 0) teleportTimer -= Time.deltaTime;
        if (cloneTimer > 0) cloneTimer -= Time.deltaTime;
        if (infernoTimer > 0) infernoTimer -= Time.deltaTime;
    }

    void HandleAbilities()
    {
        if (Input.GetKeyDown(fireballKey) && fireballTimer <= 0)
        {
            Debug.Log("Fireball targeting activated.");
            isFireballTargeting = true; // Set targeting flag
        }
        if (Input.GetKeyDown(teleportKey) && teleportTimer <= 0)
        {
            if ((currentLevel > 1)|| unlock_abilities)
            {
                Debug.Log("Teleport targeting activated.");
            isTeleportTargeting = true; // Set targeting flag
            }
        }
        if (Input.GetKeyDown(cloneKey) && cloneTimer <= 0)
        {
            if ((currentLevel > 2) || unlock_abilities)
            {
                Debug.Log("Clone targeting activated.");
                isCloneTargeting = true; // Set targeting flag
            }
        }
        if (Input.GetKeyDown(infernoKey) && infernoTimer <= 0)
        {
            if ((currentLevel > 3) || unlock_abilities)
            {
                Debug.Log("Inferno targeting activated.");
                isInfernoTargeting = true; // Set targeting flag
            }

        }

    }

    void SetFireballTarget()
    {
        RaycastHit hit;
        // Perform raycast to detect clicked object
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, clickableLayers))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Check if the clicked object is an enemy
            if (hitObject.CompareTag("Enemy") || hitObject.CompareTag("Demon"))
            {
                selectedEnemy = hitObject; // Assign the enemy to selectedEnemy
                fireballTarget = selectedEnemy.transform.position; // Set the target position to the enemy's location

                Debug.Log($"Fireball target set on enemy: {selectedEnemy.name}");

                // Start the fireball coroutine
                StartCoroutine(HandleFireball());
            }
            else
            {
                Debug.Log("No valid enemy clicked!");
            }
        }
        else
        {
            Debug.Log("No target detected!");
        }

        // Reset targeting mode
        isFireballTargeting = false;
    }


    void SetTeleportTarget()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, clickableLayers))
        {
            StartCoroutine(HandleTeleport(hit.point)); // Pass clicked position
        }
        isTeleportTargeting = false; // Reset flag
    }

    void SetCloneTarget()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, clickableLayers))
        {
            // Instantiate the clone at the hit point
            GameObject clone = Instantiate(clonePrefab, hit.point, transform.rotation);

            // Notify camps to follow the clone
            foreach (CampController camp in camps)
            {
                if (camp != null) // Optional null check
                {
                    camp.FollowClone(clone.transform);
                }
            }

            // Destroy the clone after 5 seconds
            Destroy(clone, 5f);

            // Start the coroutine to execute the foreach loop after 5 seconds
            StartCoroutine(ExecuteAfterDelay(4.9f));
        }

        cloneTimer = cloneCooldown;
        isCloneTargeting = false; // Reset flag
    }

    // Coroutine to execute after a delay
    private IEnumerator ExecuteAfterDelay(float delay)
    {
        // Wait for the specified time
        yield return new WaitForSeconds(delay);

        // Execute the foreach loop
        foreach (CampController camp in camps)
        {
            if (camp != null) // Optional null check
            {
                camp.FollowPlayer(transform);
            }
        }
    }


    void SetInfernoTarget()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, clickableLayers))
        {
            ParticleSystem inferno = Instantiate(infernoEffect, hit.point, Quaternion.Euler(90, 0, 0)); // Rotate horizontal
            Destroy(inferno.gameObject, 5f); // Destroy after 5 seconds

        }
        infernoTimer = infernoCooldown;
        isInfernoTargeting = false; // Reset flag
    }

    IEnumerator HandleTeleport(Vector3 targetPosition)
    {
        Debug.Log("Teleport");
        animator.SetLayerWeight(1, 1);
        animator.Play(DISAPPEAR, 1);

        ParticleSystem startEffect = Instantiate(teleportEffect, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        Destroy(startEffect.gameObject);

        transform.position = targetPosition;

        animator.Play(APPEAR, 1);
        ParticleSystem endEffect = Instantiate(teleportEffect, targetPosition, Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        Destroy(endEffect.gameObject);

        animator.SetLayerWeight(1, 0);
        teleportTimer = teleportCooldown;
    }

    void ClickToMove()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, clickableLayers)&& !isDying)
        {
            agent.isStopped = false;
            agent.destination = hit.point;

            if (clickEffect != null)
            {
                if (activeParticle != null)
                {
                    Destroy(activeParticle);
                }
                activeParticle = Instantiate(clickEffect.gameObject, hit.point + new Vector3(0, 0.1f, 0), clickEffect.transform.rotation);
            }
        }
    }

    bool HasReachedDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude < 0.01f)
            {
                return true;
            }
        }
        return false;
    }

    void StopMovement()
    {
        agent.velocity = Vector3.zero;
        agent.isStopped = true;

        if (activeParticle != null)
        {
            Destroy(activeParticle);
            activeParticle = null;
        }

        animator.Play(IDLE,0);
    }

    void SetAnimations()
    {
        if (isDying)
        {
            return;
        }
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            animator.Play(WALK,0);
        }
        else
        {
            animator.Play(IDLE,0);
        }
    }

    void FaceTarget()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 direction = (agent.destination - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
        }
    }
    void FaceTargetSmoothly()
    {
        if (agent.hasPath)
        {
            // Get direction to the target
            Vector3 direction = agent.steeringTarget - transform.position;
            direction.y = 0; // Ignore vertical rotation

            // Avoid rotation if the target direction is very small
            if (direction.sqrMagnitude < 0.01f) return;

            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly rotate towards the target
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                360f * Time.deltaTime // Adjust rotation speed here
            );
        }
    }
    public void UsePotion()
    {
        if (potions > 0 && currentHealth < maxHealth)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + maxHealth / 2);
            potions--;
            drinkPotion = false;
        }
    }

    public void CollectPotion()
    {
        if (potions < MaxPotions)
        {
            potions++;
        }
    }

    public void GainXP(int xp)
    {
        if (currentLevel < 4)
        {
            currentXP += xp;
            if (currentXP >= maxXP)
            {
                LevelUp();
            }
        }
    }

    void LevelUp()
    {
        currentXP -= maxXP;
        currentLevel++;
        abilityPoints++;
        maxHealth += 100;
        currentHealth = maxHealth;
        maxXP = 100 * currentLevel;
    }

    void GameOver()
    {
        Gameover_scene.SetActive(true);
        animator.Play("dying");
        new WaitForSeconds(1.5f);
        Debug.Log("Game Over!");
        // Implement Game Over screen logic here
    }
}
