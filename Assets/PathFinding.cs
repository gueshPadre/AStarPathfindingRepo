using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathFinding : MonoBehaviour
{
    WaypointMaker waypointGraph;
    [SerializeField] Transform startPos = null;
    [SerializeField] List<Transform> goals = new List<Transform>();
    public List<Transform> Goals { get { return goals; } set { goals = value; } }
    public bool IsDone { get; set; }

    public event Action MoveToGoal;

    private void Awake()
    {
        waypointGraph = GetComponent<WaypointMaker>();
    }

    private void Update()
    {
        if (!IsDone)
        {
            FindPath(startPos.position, goals[Agent.goalIndex].position); 
        }
    }

    /// <summary>
    /// Find Quickest path between two points in the waypoint graph
    /// </summary>
    /// <param name="_startPos"> starting node </param>
    /// <param name="_goal"> goal </param>
    void FindPath(Vector3 _startPos, Vector3 _goal)
    {
        Node startNode = waypointGraph.GetClosestNode(_startPos);
        Node targetNode = waypointGraph.GetClosestNode(_goal);


        List<Node> openList = new List<Node>();     // List of the nodes that we need to visit
        HashSet<Node> closedList = new HashSet<Node>();     // List of the nodes we already checked

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)        // check which one in the neighboring nodes is the closest to the goal
            {
                if (openList[i].FCost < currentNode.FCost || openList[i].FCost == currentNode.FCost && openList[i].hCost < currentNode.hCost)
                {
                    currentNode = openList[i];
                }
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == targetNode)      // check if we're at destination
            {
                GetPathBack(startNode, targetNode);
            }

            foreach (Node neighborNode in waypointGraph.NeighboringNodes(currentNode))      // explore neighboring nodes
            {
                if (!neighborNode.isObstacle || closedList.Contains(neighborNode))
                {
                    continue;
                }
                int moveCost = currentNode.gCost + GetManhattanDistance(currentNode, neighborNode);

                if (moveCost < neighborNode.gCost || !openList.Contains(neighborNode))
                {
                    neighborNode.gCost = moveCost;
                    neighborNode.hCost = GetManhattanDistance(neighborNode, targetNode);
                    neighborNode.parent = currentNode;  // set parent so we can trace back after

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }

                }
            }
        }
        if (!IsDone)
        {
            Debug.LogWarning("No Path was found");
        }
    }

    /// <summary>
    /// Trace back the most efficient path taken to get to the goal
    /// </summary>
    /// <param name="_startNode"> the starting node </param>
    /// <param name="_endNode"> the end node </param>
    void GetPathBack(Node _startNode, Node _endNode)
    {
        List<Node> finalPath = new List<Node>();
        Node currentNode = _endNode;

        while (currentNode != _startNode)
        {
            finalPath.Add(currentNode);
            currentNode = currentNode.parent;   //going back the path
        }

        finalPath.Reverse();
        for (int i = 0; i < finalPath.Count; i++)       //add the moving position to the queue
        {
            Agent.Instance.movingPos.Enqueue(new Vector3(finalPath[i].pos.x,Agent.Instance.transform.position.y, finalPath[i].pos.z));
        }

        waypointGraph.shortestPath = finalPath;
        IsDone = true;
        MoveToGoal?.Invoke();
    }

    // Calculate the manhattan distance of two nodes
    int GetManhattanDistance(Node _nodeA, Node _nodeB)
    {
        int iX = Mathf.Abs(_nodeA.gridX - _nodeB.gridX);
        int iY = Mathf.Abs(_nodeA.gridY - _nodeB.gridY);

        return iX + iY;
    }
}
