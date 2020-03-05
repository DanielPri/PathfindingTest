using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection
{
    public Node connectedNode;
    public float distance;

    public Connection(Node _node, float _distance)
    {
        this.connectedNode = _node;
        this.distance = _distance;
    }
}
