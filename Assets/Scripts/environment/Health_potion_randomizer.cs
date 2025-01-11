using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // For NavMesh support

public class HealthPotionRandomizer : MonoBehaviour
{
    [Header("Prefab to Instantiate")]
    public GameObject prefab; // The prefab to instantiate

    [Header("Potion Settings")]
    public int potionCount = 10; // Number of potions to generate
    public float minDistanceBetweenPotions = 5.0f; // Minimum distance between potions

    [Header("Spawn Area Settings")]
    public Vector3 centerPoint = new Vector3(63.99f, 6.33f, 38.97f); // Starting point
    public float range = 50f; // Range around the center point to generate potions

    private List<Vector3> usedPositions = new List<Vector3>();

    void Start()
    {
        InstantiatePrefabsWithinBounds();
    }

    public void InstantiatePrefabsWithinBounds()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is not assigned. Please assign a prefab.");
            return;
        }

        int attempts = 0; // Limit attempts to avoid infinite loops
        while (usedPositions.Count < potionCount && attempts < potionCount * 20) // Increased attempt limit
        {
            // Generate a random position within the defined range
            Vector3 randomPosition = GetRandomPositionInRange();

            // Validate the position using NavMesh and spacing
            if (IsPositionOnNavMesh(randomPosition) && IsPositionFarEnough(randomPosition))
            {
                // Instantiate the prefab at the valid position
                Instantiate(prefab, randomPosition, Quaternion.identity);

                // Add the position to the used list
                usedPositions.Add(randomPosition);
            }

            attempts++;
        }

        if (usedPositions.Count < potionCount)
        {
            Debug.LogWarning("Could not place all potions. Try adjusting the parameters.");
        }
    }

    // Generate a random position within the range
    private Vector3 GetRandomPositionInRange()
    {
        float randomX = Random.Range(centerPoint.x - range, centerPoint.x + range);
        float randomZ = Random.Range(centerPoint.z - range, centerPoint.z + range);

        // Raycast to determine the Y position based on the terrain height
        Vector3 rayOrigin = new Vector3(randomX, centerPoint.y + 50f, randomZ); // Start raycasting from above
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f))
        {
            return hit.point; // Return the position where the ray hit
        }
        else
        {
            return new Vector3(randomX, centerPoint.y, randomZ); // Default to center height if no hit
        }
    }

    // Check if a position is on the NavMesh
    private bool IsPositionOnNavMesh(Vector3 position)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas);
    }

    // Check if a new position is far enough from already-used positions
    private bool IsPositionFarEnough(Vector3 position)
    {
        foreach (Vector3 usedPos in usedPositions)
        {
            if (Vector3.Distance(position, usedPos) < minDistanceBetweenPotions)
            {
                return false; // Too close to an existing potion
            }
        }
        return true;
    }
}
