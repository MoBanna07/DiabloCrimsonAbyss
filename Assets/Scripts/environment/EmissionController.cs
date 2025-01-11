using System.Collections;
using UnityEngine;

public class EmissionController : MonoBehaviour
{
    public GameObject object1;
    public GameObject object2;
    public float animationSpeed = 2.0f; // Speed of the emission color animation
    public float detectionRange = 10f; // Range within which the player must be
    public int requiredRunes = 3; // Minimum number of runes required

    private Renderer renderer1;
    private Renderer renderer2;
    private Material material1;
    private Material material2;
    private Color emissionColor = Color.black;

    private GameObject player;
    private PlayerController playerController; // Assumes player script has rune count

    private bool emissionActivated = false; // Track if emission animation has been activated
    private float currentEmissionIntensity = 0f; // Track current intensity

    void Start()
    {
        // Get the materials of the objects
        renderer1 = object1.GetComponent<Renderer>();
        renderer2 = object2.GetComponent<Renderer>();

        if (renderer1 != null) material1 = renderer1.material;
        if (renderer2 != null) material2 = renderer2.material;

        // Cache player and player controller reference
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        Debug.Log("EmissionController initialized.");
    }

    void Update()
    {
        if (player == null || playerController == null || material1 == null || material2 == null)
        {
            Debug.LogWarning("Missing references. Cannot proceed.");
            return;
        }

        // Check if conditions are met
        bool isPlayerInRange = (player.transform.position.x - transform.position.x) <= detectionRange;
        bool hasEnoughRunes = playerController.Runes>= requiredRunes;

        if (isPlayerInRange && hasEnoughRunes && !emissionActivated)
        {
            Debug.Log("Conditions met. Starting emission animation.");
            // Start the emission animation
            StartCoroutine(ActivateEmission());
        }
    }

    private IEnumerator ActivateEmission()
    {
        emissionActivated = true; // Prevent reactivation

        while (currentEmissionIntensity < 1f)
        {
            currentEmissionIntensity += Time.deltaTime * animationSpeed;
            emissionColor = new Color(currentEmissionIntensity, currentEmissionIntensity, currentEmissionIntensity);

            // Apply the emission color to both materials
            if (material1 != null)
            {
                material1.EnableKeyword("_EMISSION");
                material1.SetColor("_EmissionColor", emissionColor);
                DynamicGI.SetEmissive(renderer1, emissionColor); // Update Global Illumination
                Debug.Log("Object1 emission updated: " + emissionColor);
            }

            if (material2 != null)
            {
                material2.EnableKeyword("_EMISSION");
                material2.SetColor("_EmissionColor", emissionColor);
                DynamicGI.SetEmissive(renderer2, emissionColor); // Update Global Illumination
                Debug.Log("Object2 emission updated: " + emissionColor);
            }

            yield return null; // Wait for the next frame
        }

        // Ensure maximum intensity is set
        emissionColor = new Color(1f, 1f, 1f);
        if (material1 != null)
        {
            material1.EnableKeyword("_EMISSION");
            material1.SetColor("_EmissionColor", emissionColor);
            DynamicGI.SetEmissive(renderer1, emissionColor);
            Debug.Log("Object1 emission set to maximum intensity.");
        }
        if (material2 != null)
        {
            material2.EnableKeyword("_EMISSION");
            material2.SetColor("_EmissionColor", emissionColor);
            DynamicGI.SetEmissive(renderer2, emissionColor);
            Debug.Log("Object2 emission set to maximum intensity.");
        }

        Debug.Log("Emission animation completed.");
    }
}
