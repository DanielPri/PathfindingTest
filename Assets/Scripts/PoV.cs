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

    // Start is called before the first frame update
    void Start()
    {
        nodes = new List<Node>();
        generateNodes();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Vector3> generatePath(Vector3 from, Vector3 to)
    {
        Node startNode = new Node(Room.CORRIDOR1, new Vector3(999999,0,999999));
        Node endNode = startNode;
        
        // find the start node
        foreach (Node node in nodes)
        {
            RaycastHit hit;
            Vector3 diff = node.position - from;
            if (!Physics.SphereCast(from, AIRadius, diff, out hit, diff.magnitude, layerMask))
            {
                if ((node.position - from).magnitude < (startNode.position - from).magnitude)
                {
                    startNode = node;
                }
            }
        }
        Instantiate(debugMarkerPrefab, startNode.position, Quaternion.identity);

        // find the end node
        foreach (Node node in nodes)
        {
            RaycastHit hit;
            Vector3 diff = node.position - to;
            if (!Physics.SphereCast(to, AIRadius, diff, out hit, diff.magnitude, layerMask))
            {
                if ((node.position - to).magnitude < (endNode.position - to).magnitude)
                {
                    endNode = node;
                }
            }
        }

        List<Vector3> path = Astar(startNode, endNode, to);
        path.Insert(0, from);
        path.Add(to);
        return path;
    }

    List<Vector3> Astar(Node startNode, Node goalNode, Vector3 destination)
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

            Vector3 diff = currentNode.position - destination;
            RaycastHit hit;
            if (currentNode == goalNode || !Physics.SphereCast(destination, AIRadius, diff, out hit, diff.magnitude, layerMask))
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
                    connection.Heuristic = calcHeuristic(connection, destination); ;
                    connection.parentInPath = currentNode;
                    if (!OpenList.Contains(connection))
                    {
                        OpenList.Add(connection);
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
                    Instantiate(markerPrefab, PoVNode.position, Quaternion.identity);
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

    public static void DrawLine(Vector3 from, Vector3 to, GameObject parent)
    {
        GameObject someLine = new GameObject();
        if(parent != null) { someLine.transform.parent = parent.transform;  }
        someLine.AddComponent<LineRenderer>();
        LineRenderer lr = someLine.GetComponent<LineRenderer>();
        lr.startWidth = 0.2f;
        lr.endWidth = lr.startWidth;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        Destroy(someLine, 2f);
    }

    float GetDistance(Node node1, Node node2)
    {
        return Vector3.Magnitude(node1.position - node2.position);
    }
}
