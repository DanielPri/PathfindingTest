using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;
    public bool useNavmesh = false;
    public PoV PoVData;
    List<Vector3> path;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
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
                    path = PoVData.generatePath(transform.position, hit.point);
                    
                    //  ------ Draw the path -----------------------------///////////
                    Vector3 currentNode = path[0];
                    foreach (Vector3 nodeInPath in path)
                    {
                        PoV.DrawLine(currentNode, nodeInPath, null);
                        currentNode = nodeInPath;
                    }
                    // -------------------------------------------------------///////
                }
                
            }
        }
    }
}
