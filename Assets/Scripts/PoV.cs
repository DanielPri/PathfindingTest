using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoV : MonoBehaviour
{
    public GameObject markerPrefab;
    public GameObject debugMarkerPrefab;
    List<Node> nodes;
    public float AIRadius = 1f;
    public LayerMask layerMask;
    public Heuristic heuristic;
    public enum Heuristic
    {
        NULL,
        EUCLIDEAN,
        CLUSTER
    }

    private GameObject markerContainer;

    // Start is called before the first frame update
    void Start()
    {
        nodes = new List<Node>();
        markerContainer = new GameObject("markerContainer");
        generateNodes();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Vector3> generatePath(Vector3 from, Vector3 to)
    {
        Node startNode = new Node(Room.NONE, from);
        Node endNode = new Node(Room.NONE, to);

        // generate the start node
        foreach (Node node in nodes)
        {
            RaycastHit hit;
            Vector3 diff = node.position - from;
            if (!Physics.SphereCast(from, AIRadius, diff, out hit, diff.magnitude, layerMask))
            {
                startNode.connections.Add(node);
                node.connections.Add(startNode);
            }
        }
        nodes.Insert(0, startNode);

        // generate the end node
        foreach (Node node in nodes)
        {
            RaycastHit hit;
            Vector3 diff = node.position - to;
            if (!Physics.SphereCast(to, AIRadius, diff, out hit, diff.magnitude, layerMask))
            {
                endNode.connections.Add(node);
                node.connections.Add(endNode);
            }
        }
        nodes.Add(endNode);

        List<Vector3> path = Astar(startNode, endNode);

        //clean up the nodes list since we dont need startnode and endnode anymore
        foreach(Node startNodeConnection in startNode.connections)
        {
            startNodeConnection.connections.Remove(startNode);
        }
        foreach (Node endNodeConnection in endNode.connections)
        {
            endNodeConnection.connections.Remove(endNode);
        }
        nodes.Remove(startNode);
        nodes.Remove(endNode);

        return path;
    }

    List<Vector3> Astar(Node startNode, Node goalNode)
    {
        List<Node> OpenList = new List<Node>();
        List<Node> ClosedList = new List<Node>();

        OpenList.Add(startNode);
        
        while (OpenList.Count > 0)
        {
            Node currentNode = new Node();
            currentNode.CostSoFar = 99999f;
            // find cheapest node in open list, have it be the current observed node
            foreach (Node node in OpenList)
            {
                if(node.EstimatedTotal < currentNode.EstimatedTotal || node.EstimatedTotal == currentNode.EstimatedTotal && node.Heuristic < currentNode.Heuristic)
                {
                    currentNode = node;
                }
            }
            OpenList.Remove(currentNode);
            ClosedList.Add(currentNode);
            
            if (currentNode == goalNode)
            {
                return retracePath(startNode, currentNode);
            }

            //check connected nodes
            foreach (Node connection in currentNode.connections)
            {
                if (ClosedList.Contains(connection))
                {
                    continue;
                }
                float newPossibleCostSoFar = GetDistance(currentNode, connection) + currentNode.CostSoFar;
                if (newPossibleCostSoFar < connection.CostSoFar || !OpenList.Contains(connection))
                {
                    
                    connection.CostSoFar = newPossibleCostSoFar;
                    connection.Heuristic = calcHeuristic(connection, goalNode.position); ;
                    connection.parentInPath = currentNode;
                    if (!OpenList.Contains(connection))
                    {
                        OpenList.Add(connection);
                        DrawLine(currentNode.position, connection.position, null, Color.red, 0.05f);
                        var marker = Instantiate(markerPrefab, connection.position, Quaternion.identity, markerContainer.transform);
                        Destroy(marker, 2);
                    }
                }
            }
        }
        Debug.Log("Pathfinding error!");
        return null;
    }

    private float calcHeuristic(Node potential, Vector3 target)
    {
        switch (heuristic)
        {
            case Heuristic.EUCLIDEAN:
                Debug.Log("Euclidean heuristic");
                return (target - potential.position).magnitude;
            case Heuristic.CLUSTER:
                Debug.Log("Cluster heuristic");
                // return cluster
                break;
            default:
                Debug.Log("Null heuristic");
                return 0;
        }
        return 0;
    }

    List<Vector3> retracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;
        while(currentNode != startNode)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parentInPath;
        }
        path.Reverse();
        return path;
    }

    bool canReach(Vector3 from, Vector3 to)
    {
        Vector3 diff = from - to;
        RaycastHit hit;
        if (Physics.SphereCast(from, AIRadius, diff, out hit, diff.magnitude, layerMask))
        {
            return false;
        }
        else return true;
    }

    void generateNodes()
    {
        foreach (Transform room in transform)
        {
            foreach (Transform childnode in room)
            {
                if (childnode.tag == "Node")
                {
                    Node PoVNode = new Node((Room)System.Enum.Parse(typeof(Room), room.name.ToUpper()), childnode.position);
                    nodes.Add(PoVNode);
                }
            }
        }

        foreach (Node node in nodes)
        {
            foreach (Node otherNode in nodes)
            {
                if (node != otherNode)
                {
                    RaycastHit hit;
                    Vector3 diff = otherNode.position - node.position;
                    if (Physics.SphereCast(node.position, AIRadius, diff, out hit, diff.magnitude, layerMask))
                        {
                        if (hit.transform.tag != "Wall")
                        {
                            node.connections.Add(otherNode);
                        }
                    }
                    else
                    {
                        node.connections.Add(otherNode);
                    }
                }
            }
            //foreach (Node connection in node.connections)
            //{
            //    //DrawLine(node.position, connection.position, null);
            //}
        }
    }

    public static void DrawLine(Vector3 from, Vector3 to, GameObject parent, Color color, float width)
    {
        GameObject someLine = new GameObject();
        if(parent != null) { someLine.transform.parent = parent.transform;  }
        someLine.AddComponent<LineRenderer>();
        LineRenderer lr = someLine.GetComponent<LineRenderer>();
        lr.startWidth = width;
        lr.endWidth = lr.startWidth;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        lr.material.color = color;
        Destroy(someLine, 2f);
    }

    float GetDistance(Node node1, Node node2)
    {
        return Vector3.Magnitude(node1.position - node2.position);
    }
}
