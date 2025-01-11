using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cheats : MonoBehaviour
{
    public PlayerController playerController; // Reference to the player's main script
    public float slowMotionScale = 0.5f; // Scale for slow motion gameplay
    private bool isSlowMotion = false; // Toggle for slow motion gameplay
    private bool isCooldownDisabled = false; // Toggle for ability cooldowns

    void Awake()
    {
        // Initialize playerController in Awake or Start
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogError("Player object with tag 'Player' not found!");
        }
    }

    void Update()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController reference is missing!");
            return;
        }

        // Heal: Increase health by 20 points
        if (Input.GetKeyDown(KeyCode.H))
        {
            playerController.currentHealth += 20;
        }

        // Decrement Health: Decrease health by 20 points
        if (Input.GetKeyDown(KeyCode.D))
        {
            playerController.currentHealth -= 20;
        }

        // Toggle Invincibility: Prevents taking damage
        if (Input.GetKeyDown(KeyCode.I))
        {
            playerController.isInvincible = !playerController.isInvincible;
        }

        // Toggle Slow Motion
        if (Input.GetKeyDown(KeyCode.M))
        {
            isSlowMotion = !isSlowMotion; // Toggle the slow-motion state
            Time.timeScale = isSlowMotion ? slowMotionScale : 1f; // Set the game speed
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // Adjust physics updates to match the new time scale
        }

        // Toggle Cool Down: Set ability cooldown to 0
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCooldownDisabled = !isCooldownDisabled;
            if (isCooldownDisabled)
            {
                playerController.infernoCooldown =
                playerController.cloneCooldown =
                playerController.teleportCooldown =
                playerController.fireballCooldown = 0;
            }
        }

        // Unlock Abilities: Unlock all abilities
        if (Input.GetKeyDown(KeyCode.U))
        {
            playerController.unlock_abilities = true;
        }

        // Gain Ability Points: Increment ability points by 1
        if (Input.GetKeyDown(KeyCode.A))
        {
            playerController.abilityPoints += 1;
        }

        // Gain XP: Increment XP by 100 points
        if (Input.GetKeyDown(KeyCode.X))
        {
            playerController.GainXP(100);
        }
    }
}
