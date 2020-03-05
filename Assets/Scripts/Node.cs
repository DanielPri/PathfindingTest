using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Room parentCluster;
    public List<Node> connections;
    public Vector3 position;
    public float CostSoFar;
    public float Heuristic;
    public Node parentInPath;

    public Node()
    {
        connections = new List<Node>();
        CostSoFar = Heuristic = 0;
    }

    public Node(Room _parent, Vector3 _position)
    {
        connections = new List<Node>();
        this.parentCluster = _parent;
        this.position = _position;
        CostSoFar = Heuristic = 0;
    }
    
    public float EstimatedTotal
    {
        get
        {
            return CostSoFar + Heuristic;
        }
    }
}
