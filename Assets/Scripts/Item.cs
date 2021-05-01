using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private void Start()
    {
        GetComponent<BoxCollider>().size *= 2;      // to make sure that it's big enough to get collided with when agent is travelling through the nodes
    }

    private void OnTriggerEnter(Collider other)
    {
        // When the player hits the item, it disapears
        if (other.GetComponent<Agent>() && this.gameObject == NextOnList())
        {
            
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
        }
    }

    /// <summary>
    /// Ensures that the item will only disappear if it's his turn to get picked up
    /// </summary>
    /// <returns></returns>
    GameObject NextOnList()
    {
        return FindObjectOfType<PathFinding>().Goals[Agent.goalIndex].gameObject;
    }
}
