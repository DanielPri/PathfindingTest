using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour
{
    public float maxVelocity = 5f;
    public float radiusOfSatisfaction = 1f;
    public float t2t = 2f;

    public float maxAngularVelocity = 5f;

    public Vector3 direction;
    public Vector3 velocity;
    private float currentMaxVelocity = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void seek(Vector3 target)
    {
        currentMaxVelocity = maxVelocity;
        direction = Vector3.Normalize(target - transform.position);
        velocity = direction * maxVelocity;
        transform.position = transform.position + velocity * Time.deltaTime;
    }

    public void arrive(Vector3 target)
    {
        Vector3 vectorToTarget = target - transform.position;
        float distance = vectorToTarget.magnitude;
        if (distance > radiusOfSatisfaction)
        {
            currentMaxVelocity = Mathf.Min(maxVelocity, distance / t2t);
            Debug.Log("velocity is" + currentMaxVelocity);
            velocity = vectorToTarget.normalized * currentMaxVelocity;
            transform.position = transform.position + velocity * Time.deltaTime;
        }
    }

    public void faceDirection()
    {
        float angle = Vector3.SignedAngle(direction, transform.forward, transform.up);
        if (currentMaxVelocity > 3)
        {
            transform.Rotate(0, -angle * maxAngularVelocity * Time.deltaTime, 0);
        }
    }

}
