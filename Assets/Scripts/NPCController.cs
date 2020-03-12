using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;

public class NPCController : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;
    public bool useNavmesh = false;
    public PoV PoVData;
    public float distanceFromNode = 0.1f;
    public bool chaser;

    List<Vector3> path;
    private Vector3 destination;
    private Steering steering;
    private int pathIndex = 0;
    private GameObject lineContainer;
    private GraphicsController gc;

    Transform chaseTarget;
    // Start is called before the first frame update
    void Start()
    {
        gc = GameObject.Find("Graphics").GetComponent<GraphicsController>();
        if (chaser)
        {
            chaseTarget = GameObject.Find("NPC").transform;
        }
        lineContainer = new GameObject("line container");
        steering = GetComponent<Steering>();
    }

    // Update is called once per frame
    void Update()
    {
        if (chaser)
        {
            Chase();
        }
        else { HandleMouseButton(); }
        
        FollowPath();
        steering.faceDirection();
    }

    private void Chase()
    {
        path = PoVData.generatePath(transform.position, chaseTarget.position);
    }

    void HandleMouseButton()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray clickRay = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(clickRay, out hit))
            {
                if (useNavmesh)
                {
                    agent.SetDestination(hit.point);
                }
                else
                {
                    PoVData.destinationCluster = (Room)System.Enum.Parse(typeof(Room), LayerMask.LayerToName(hit.transform.gameObject.layer));
                    //Debug.Log("destination is " + PoVData.destinationCluster);
                    RaycastHit hit2;
                    Physics.Raycast(transform.position, Vector3.down, out hit2);
                    PoVData.startingCluster = (Room)System.Enum.Parse(typeof(Room), LayerMask.LayerToName(hit.transform.gameObject.layer));
                    //Debug.Log("origin is " + PoVData.startingCluster);

                    destination = hit.point;
                    path = PoVData.generatePath(transform.position, destination);

                    pathIndex = 0;

                    //  ------ Draw the path -----------------------------///////////
                    if (gc.displayGraphics)
                    {
                        Vector3 currentNode = path[0];
                        PoV.DrawLine(transform.position, currentNode, lineContainer, Color.green, 0.5f);
                        foreach (Vector3 nodeInPath in path)
                        {
                            PoV.DrawLine(currentNode, nodeInPath, lineContainer, Color.green, 0.5f);
                            currentNode = nodeInPath;
                        }
                        // -------------------------------------------------------///////
                    }
                }

            }
        }
    }

    void FollowPath()
    {
        if(path != null)
        {
            //Update the node to follow if you reach one
            if ((transform.position - path[pathIndex]).magnitude < distanceFromNode)
            {
                if (path[pathIndex] == destination)
                {
                    path = null;
                    pathIndex = 0;
                }
                else
                {
                    pathIndex++;
                }
                
            }
            if(path[pathIndex] == destination)
            {
                Debug.Log("arriveing");
                steering.arrive(path[pathIndex]);
            }
            else { steering.seek(path[pathIndex]); }
        }
    }
}
