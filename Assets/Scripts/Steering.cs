using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour
{
    public float maxVelocity = 5f;
    public float radiusOfSatisfaction = 1f;
    public float t2t = 0.1f;

    public Vector3 direction;
    public Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void seek(Transform target)
    {
        direction = Vector3.Normalize(target.position - transform.position);
        velocity = direction * maxVelocity;
        transform.position = transform.position + velocity * Time.deltaTime;
    }

    public void arrive(Transform target)
    {
        Vector3 vectorToTarget = target.position - transform.position;
        float distance = vectorToTarget.magnitude;
        if (distance > radiusOfSatisfaction)
        {
            float currentMaxVelocity = Mathf.Min(maxVelocity, distance / t2t);
            velocity = vectorToTarget.normalized * currentMaxVelocity;
            transform.position = transform.position + velocity * Time.deltaTime;
        }
    }

}
