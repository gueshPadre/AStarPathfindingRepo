using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int gridX;
    public int gridY;

    public bool isObstacle;
    public Vector3 pos;

    public Node parent;     // to trace down the path

    public int gCost;
    public int hCost;
    public int FCost { get { return hCost + hCost; } }

    public Node(bool _isAvailable, Vector3 _position, int _gridX, int _gridY)
    {
        isObstacle = _isAvailable;
        pos = _position;
        gridX = _gridX;
        gridY = _gridY;
    }
}
