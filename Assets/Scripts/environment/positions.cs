using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class positions : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform centerPoint; // Assign a central point in the scene
    public float spacing = 2f; // Adjust spacing between objects

    void Start()
    {
        RecenterObjects();
    }

    void RecenterObjects()
    {
        Transform[] objects = FindObjectsOfType<Transform>();

        foreach (Transform obj in objects)
        {
            if (obj != this.transform && obj != centerPoint)
            {
                // Calculate a new position relative to the center
                Vector3 direction = obj.position - centerPoint.position;
                direction.Normalize();
                obj.position = centerPoint.position + direction * spacing;
            }
        }
    }


}
