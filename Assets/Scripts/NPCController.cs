using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;
    public bool useNavmesh = false;
    public PoV PoVData;
    public float cooldown = 0.2f;
    List<Vector3> path;

    private GameObject lineContainer;
    // Start is called before the first frame update
    void Start()
    {
        lineContainer = new GameObject("line container");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray clickRay = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
           if(Physics.Raycast(clickRay, out hit))
            {
                if (useNavmesh)
                {
                    agent.SetDestination(hit.point);
                }
                else
                {
                    //PoVData.destinationCluster = (Room)System.Enum.Parse(typeof(Room), LayerMask.LayerToName(hit.transform.gameObject.layer));
                    //Debug.Log("destination is " + PoVData.destinationCluster);
                    //Physics.Raycast(transform.position, Vector3.down, out hit);
                    //PoVData.startingCluster = (Room)System.Enum.Parse(typeof(Room), LayerMask.LayerToName(hit.transform.gameObject.layer));
                    //Debug.Log("origin is " + PoVData.startingCluster);

                    path = PoVData.generatePath(transform.position, hit.point);
                    
                    //  ------ Draw the path -----------------------------///////////
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
