using System.Collections;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    public GameObject explosionEffect; // Assign the explosion effect prefab
    public float explosionRadius = 5f; // Radius of the explosion
    public int damage = 20;            // Damage dealt by the explosion
    public float explosionDelay = 0.5f; // Delay before explosion
    private float timer = 0f;          // Timer to track how long the explosive has been in the air
    private bool hasExploded = false;  // Flag to check if it has already exploded

    private void Start()
    {
        // Start the timer when the explosive is instantiated
        StartCoroutine(AutoExplodeAfterDelay());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Check if the explosive hits the player
        {
            Explode();
        }
    }

    private IEnumerator AutoExplodeAfterDelay()
    {
        // Wait for 0.5 second before checking for automatic explosion
        while (timer < 0.5f)
        {
            timer += Time.deltaTime; // Increment the timer with the time passed in the frame
            yield return null;       // Wait until the next frame
        }

        // If it hasn't exploded yet, trigger the explosion automatically
        if (!hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Spawn explosion effect
        if (explosionEffect != null)
        {
            GameObject explosionInstance = Instantiate(explosionEffect, transform.position, Quaternion.identity);

            // Destroy the explosion effect after it finishes
            Destroy(explosionInstance, explosionInstance.GetComponent<ParticleSystem>().main.duration);
        }

        //// Apply damage to player (if player has a health system)
        //Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        //foreach (var hitCollider in hitColliders)
        //{
        //    if (hitCollider.CompareTag("Player"))
        //    {
        //        PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
        //        if (playerHealth != null)
        //        {
        //            playerHealth.TakeDamage(damage); // Assuming the player has a TakeDamage method
        //        }
        //    }
        //}

        // Destroy the explosive object after it explodes
        Destroy(gameObject);
    }
}
