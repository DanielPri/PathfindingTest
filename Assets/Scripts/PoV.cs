using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoV : MonoBehaviour
{
    public GameObject markerPrefab;
    public GameObject debugMarkerPrefab;
    public GameObject clustersGameObject;
    
    public float AIRadius = 1f;
    public LayerMask layerMask;
    public Heuristic heuristic;

    [HideInInspector]
    public enum Heuristic
    {
        NULL,
        EUCLIDEAN,
        CLUSTER
    }
    [HideInInspector]
    public Room destinationCluster;
    [HideInInspector]
    public Room startingCluster;

    private GameObject markerContainer;
    private float[,] lookupTable;
    private List<Node> nodes;
    private List<Node> clusters;

    // Start is called before the first frame update
    void Start()
    {
        destinationCluster = Room.NONE;
        startingCluster = Room.NONE;
        nodes = new List<Node>();
        clusters = new List<Node>();
        markerContainer = new GameObject("markerContainer");
        generateNodes();
        initializeClusters();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // overloads generatePath without the output float
    public List<Vector3> generatePath(Vector3 from, Vector3 to)
    {
        float ignoreMePls;
        return generatePath(from, to, out ignoreMePls);
    }

    // generates a path using A* between two points, also optionally outputs cost for cluster data
    public List<Vector3> generatePath(Vector3 from, Vector3 to, out float finalCost)
    {
        Node startNode = new Node(startingCluster, from);
        Node endNode = new Node(destinationCluster, to);

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
        
        // here is the path. Before returning, we need to remove our newly added nodes and their connections
        List<Vector3> path = Astar(startNode, endNode, out finalCost);

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

        // now we can return the data
        return path;
    }

    // The classic A* algorithm
    List<Vector3> Astar(Node startNode, Node goalNode, out float finalCost)
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
                return retracePath(startNode, currentNode, out finalCost);
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
                    connection.Heuristic = calcHeuristic(connection, goalNode); ;
                    connection.parentInPath = currentNode;
                    if (!OpenList.Contains(connection))
                    {
                        OpenList.Add(connection);
                        // show that it has been added to the open list
                        var marker = Instantiate(markerPrefab, connection.position, Quaternion.identity, markerContainer.transform);
                        Destroy(marker, 2);
                    }
                }
            }
        }
        Debug.Log("Pathfinding error!");
        finalCost = 0f;
        return null;
    }

    // calculates a heuristic for a node based on the type chosen
    private float calcHeuristic(Node potential, Node target)
    {
        switch (heuristic)
        {
            case Heuristic.EUCLIDEAN:
                Debug.Log("Euclidean heuristic");
                return (target.position - potential.position).magnitude;
            case Heuristic.CLUSTER:
                Debug.Log("Cluster heuristic");
                if(potential.parentCluster == Room.NONE || target.parentCluster == Room.NONE)
                {
                    Debug.Log("Cluster heuristic unexpected error");
                    return 0;
                }
                return lookupTable[(int)potential.parentCluster, (int)target.parentCluster];
            default:
                Debug.Log("Null heuristic");
                return 0;
        }
    }

    // checks all the parents of the endNode until it reaches the startnode, and returns a list of their positions
    List<Vector3> retracePath(Node startNode, Node endNode, out float finalCost)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;
        finalCost = endNode.CostSoFar;
        while (currentNode != startNode)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parentInPath;
        }
        path.Reverse();
        return path;
    }

    // checks if a node is separated from another by a wall
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

    // When the program starts, all the PoV nodes are placed in a list along with their connections and cluster
    void generateNodes()
    {
        foreach (Transform room in transform)
        {
            foreach (Transform childnode in room)
            {
                if (childnode.tag == "Node")
                {
                    Node PoVNode = new Node((Room)Enum.Parse(typeof(Room), room.name.ToUpper()), childnode.position);
                    nodes.Add(PoVNode);
                }
            }
        }

        // finds all connections that can be reached 
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
            //Draw a line for EVERY node, because it's cool.
            foreach (Node connection in node.connections)
            {
                DrawLine(node.position, connection.position, null, Color.magenta, 0.1f);
            }
        }
    }

    // generates the lookup table for clusters by running the A* algorithm between them
    private void initializeClusters()
    {
        foreach(Transform child in clustersGameObject.transform)
        {
            Room clusterName = (Room)Enum.Parse(typeof(Room), child.name);
            clusters.Add(new Node(clusterName, child.position));
        }

        // this will store the clusters distance data
        lookupTable = new float[clusters.Count, clusters.Count];

        foreach (Node currentCluster in clusters)
        {
            foreach(Node otherCluster in clusters)
            {
                float costToNode;
                generatePath(currentCluster.position, otherCluster.position, out costToNode);
                lookupTable[(int)currentCluster.parentCluster, (int)otherCluster.parentCluster] = costToNode;
            }
        }
    }

    // used to demonstrate graphically what path is chosen
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

    // euclidean distance
    float GetDistance(Node node1, Node node2)
    {
        return Vector3.Magnitude(node1.position - node2.position);
    }
}
