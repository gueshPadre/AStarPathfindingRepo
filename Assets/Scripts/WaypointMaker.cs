using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMaker : MonoBehaviour
{
    [Tooltip("The Layer Mask that the obstacles are on")]
    [SerializeField] LayerMask wallMask;
    [Tooltip("The size of the waypoint Graph")]
    [SerializeField] Vector2 WaypointGraphWorldSize;

    [Tooltip("The size of a node on the graph. The smaller, the more precise it is but the more expensive it is. 0.2 is a good number")]
    [SerializeField] float nodeRadius;

    Node[,] waypointGraph;  // 2 dimensional graph
    public List<Node> shortestPath;
    float distance = 0.1f;  //distance between the nodes when displaying it with the Gizmos

    float nodeDiameter;
    public float NodeDiameter { get { return nodeDiameter; } }
    int waypointGraphSizeX, waypointGraphSizeY;

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        waypointGraphSizeX = Mathf.RoundToInt(WaypointGraphWorldSize.x / nodeDiameter);
        waypointGraphSizeY = Mathf.RoundToInt(WaypointGraphWorldSize.y / nodeDiameter);


        CreateWaypointGraph();
    }

    // Create the waypoint graph with the multiple nodes
    public void CreateWaypointGraph()
    {
        waypointGraph = new Node[waypointGraphSizeX, waypointGraphSizeY];
        Vector3 botLeft = transform.position - Vector3.right * WaypointGraphWorldSize.x / 2 - Vector3.forward * WaypointGraphWorldSize.y / 2;

        for (int y = 0; y < waypointGraphSizeX; y++)
        {
            for (int x = 0; x < waypointGraphSizeX; x++)
            {
                Vector3 nodePos = botLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool obstacle = true;

                if (Physics.CheckSphere(nodePos, nodeRadius +0.15f, wallMask))   // Adding 0.15f to catch a bigger sphere when checking the diagonal walls
                {
                    obstacle = false;
                }

                waypointGraph[x, y] = new Node(obstacle, nodePos, x, y);
            }
        }
    }

    /// <summary>
    /// Converts the world position of the agent to the closest node position
    /// </summary>
    /// <param name="worldPos"> the world position of the agent</param>
    /// <returns> The closest node that indicates the start of the run </returns>
    public Node GetClosestNode(Vector3 worldPos)
    {
        float xPoint = ((worldPos.x + WaypointGraphWorldSize.x / 2) / WaypointGraphWorldSize.x);
        float yPoint = ((worldPos.z + WaypointGraphWorldSize.y / 2) / WaypointGraphWorldSize.y);

        xPoint = Mathf.Clamp01(xPoint);
        yPoint = Mathf.Clamp01(yPoint);

        int x = Mathf.RoundToInt((waypointGraphSizeX - 1) * xPoint);
        int y = Mathf.RoundToInt((waypointGraphSizeY - 1) * yPoint);

        return waypointGraph[x, y];

    }

    /// <summary>
    /// Check all the nodes around the one we're currently on
    /// </summary>
    /// <param name="_currentNode"> The node we're on at that moment </param>
    /// <returns> A list of all the nodes around the one we're on </returns>
    public List<Node> NeighboringNodes(Node _currentNode)
    {
        List<Node> neighboringNodes = new List<Node>();
        //int xCheck;
        //int yCheck;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                //if we are on the node tha was passed in, skip this iteration.
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = _currentNode.gridX + x;
                int checkY = _currentNode.gridY + y;

                //Make sure the node is within the grid.
                if (checkX >= 0 && checkX < waypointGraphSizeX && checkY >= 0 && checkY < waypointGraphSizeY)
                {
                    neighboringNodes.Add(waypointGraph[checkX, checkY]); //Adds to the neighbours list.
                }

            }
        }
        return neighboringNodes;
    }

    public void EmptyCurrentGraph()
    {
        waypointGraph = null;
    }

    /// <summary>
    /// Visual representation of the calculated best run for the Agent
    /// </summary>
    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireCube(transform.position, new Vector3(WaypointGraphWorldSize.x, 1, WaypointGraphWorldSize.y));//Draw a wire cube with the given dimensions from the Unity inspector

    //    if (waypointGraph != null)//If the grid is not empty
    //    {
    //        foreach (Node n in waypointGraph)//Loop through every node in the grid
    //        {
    //            if (n == null) { return; }
    //            if (n.isObstacle)//If the current node is a wall node
    //            {
    //                Gizmos.color = Color.grey;//Set the color of the node
    //            }
    //            else
    //            {
    //                Gizmos.color = Color.white;//Set the color of the node
    //            }


    //            if (shortestPath != null)//If the final path is not empty
    //            {
    //                if (shortestPath.Contains(n))//If the current node is in the final path
    //                {
    //                    Gizmos.color = Color.green;//Set the color of that node
    //                }

    //            }


    //            Gizmos.DrawCube(n.pos, Vector3.one * (nodeDiameter - distance));//Draw the node at the position of the node.
    //        }
    //    }
    //}
}

