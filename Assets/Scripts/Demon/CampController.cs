using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CampController : MonoBehaviour
{
    public Transform player; // Reference to the player
    public float detectionRange = 5f; // Range to detect player
    public GameObject gemstone; // Reference to the gemstone structure (visible when all minions and demons die)

    // Lists to store minions and demons in different states
    public List<MinionBehavior> idleMinions = new List<MinionBehavior>(); // Minions in idle state
    public List<MinionBehavior> alertedMinions = new List<MinionBehavior>(); // Minions chasing the player
    public List<Demon> patrollingDemons = new List<Demon>(); // Demons patrolling
    public List<Demon> alertedDemons = new List<Demon>(); // Demons chasing the player
    public Transform camppos;
    private bool gemActivated = false;

    // Dynamic properties to calculate totals
    public int TotalDemons
    {
        get { return patrollingDemons.Count + alertedDemons.Count; }
    }

    public int TotalMinions
    {
        get { return idleMinions.Count + alertedMinions.Count; }
    }

    // Store number of minions and demons currently active
    private int maxAlertedMinions = 5;
    private int maxAlertedDemons = 1;

    private void Start()
    {
        // Optionally, you can initialize the minion and demon lists here, either manually or via a reference to objects in the scene
    }

    private void Update()
    {
        // Check player distance to camp
        float distanceToPlayer = Vector3.Distance(camppos.position, player.position);
        // If the player enters detection range, alert minions and demons
        if (distanceToPlayer <= detectionRange)
        {
            AlertMinionsAndDemons();
        }
        else
        {
            if ((TotalDemons+ TotalMinions) > 0)
            {
                ResetCamp();

            }
            else
            {
                CampFinished();
            }
        }
    }

    private void AlertMinionsAndDemons()
    {
        //// Only activate minions and demons that aren't already alerted
        if (alertedMinions.Count < maxAlertedMinions)
        {
            // Move idle minions to alertedMinions
            while ((alertedMinions.Count < maxAlertedMinions) && idleMinions.Count > 0)
            {
                MinionBehavior minionToAlert = idleMinions[0];
                idleMinions.RemoveAt(0);
                alertedMinions.Add(minionToAlert);
                minionToAlert.player = player;
                minionToAlert.Alert(player); // Assuming you have an Alert method on Minion
            }
        }
        if (alertedDemons.Count < maxAlertedDemons)
        {
            // Move patrolling demons to alertedDemons
            if (patrollingDemons.Count > 0)
            {
                Demon demonToAlert = patrollingDemons[0];
                patrollingDemons.RemoveAt(0);
                alertedDemons.Add(demonToAlert);
                demonToAlert.player = player;
                demonToAlert.patrol = false;
                demonToAlert.chase = true; // Assuming you have an Alert method on Demon

            }

        }
    }

    private void ResetCamp()
    {
        //// If player goes out of range, reset minions and demons
        foreach (var minion in alertedMinions)
        {
            minion.ResetToIdle();
            idleMinions.Add(minion);
        }
        alertedMinions.Clear();
        foreach (var demon in alertedDemons)
        {
            demon.chase = false;
            demon.patrol = true;
            patrollingDemons.Add(demon);
        }
        alertedDemons.Clear();
    }

    // This function should be called when a minion or demon dies
    public void HandleMinionDeath(MinionBehavior minion)
    {
        alertedMinions.Remove(minion);
        idleMinions.Remove(minion);

        // Reassign another idle minion
        if (idleMinions.Count > 0)
        {
            MinionBehavior newMinion = idleMinions[0];
            idleMinions.RemoveAt(0);
            alertedMinions.Add(newMinion);
            newMinion.Alert(player);
        }
    }

    public void HandleDemonDeath(Demon demon)
    {
        alertedDemons.Remove(demon);
        patrollingDemons.Remove(demon);

        // Reassign another patrolling demon
        if (patrollingDemons.Count > 0)
        {
            Demon newDemon = patrollingDemons[0];
            patrollingDemons.RemoveAt(0);
            alertedDemons.Add(newDemon);
            newDemon.patrol = false;
            newDemon.chase = true;
        }

    }

    private void CampFinished()
    {
        // Check if all minions and demons are dead, show gemstone
        if (idleMinions.Count + patrollingDemons.Count == 0 && gemActivated == false)
        {
            gemActivated = true;
            gemstone.SetActive(true); // Show gemstone when all minions and demons are dead
        }
    }

    public void FollowClone(Transform clone)
    {
        player = clone;
        foreach (var demon in alertedDemons)
        {
            demon.player = player;
        }
        foreach (var minion in alertedMinions)
        {
            minion.player = player;
        }
        foreach (var demon in patrollingDemons)
        {
            demon.player = player;
        }
        foreach (var minion in idleMinions)
        {
            minion.player = player;
        }
    }

    public void FollowPlayer(Transform playerTransform)
    {
        player = playerTransform;
        foreach (var demon in alertedDemons)
        {
            demon.player = player;
        }
        foreach (var minion in alertedMinions)
        {
            minion.player = player;
        }
        foreach (var demon in patrollingDemons)
        {
            demon.player = player;
        }
        foreach (var minion in idleMinions)
        {
            minion.player = player;
        }
    }

}
