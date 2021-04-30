using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMaker : MonoBehaviour
{
    [SerializeField] LayerMask wallMask;
    [SerializeField] Vector2 WaypointGraphWorldSize;

    [SerializeField] float nodeRadius;
    [SerializeField] float distance;

    Node[,] waypointGraph;
    public List<Node> shortestPath;

    float nodeDiameter;
    int waypointGraphSizeX, waypointGraphSizeY;

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        waypointGraphSizeX = Mathf.RoundToInt(WaypointGraphWorldSize.x / nodeDiameter);
        waypointGraphSizeY = Mathf.RoundToInt(WaypointGraphWorldSize.y / nodeDiameter);


        CreateWaypointGraph();
    }

    // Create the waypoint graph with the multiple nodes
    void CreateWaypointGraph()
    {
        waypointGraph = new Node[waypointGraphSizeX, waypointGraphSizeY];
        Vector3 botLeft = transform.position - Vector3.right * WaypointGraphWorldSize.x / 2 - Vector3.forward * WaypointGraphWorldSize.y / 2;

        for (int y = 0; y < waypointGraphSizeX; y++)
        {
            for (int x = 0; x < waypointGraphSizeX; x++)
            {
                Vector3 nodePos = botLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool obstacle = true;

                if (Physics.CheckSphere(nodePos, nodeRadius, wallMask))
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
}

