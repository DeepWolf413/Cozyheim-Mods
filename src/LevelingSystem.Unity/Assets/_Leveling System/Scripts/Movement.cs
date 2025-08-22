using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private float DistanceToTarget(Transform target)
    {
        Vector3 targetPosition = target.position;
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(targetPosition, currentPosition);
        return distance;
    }

    // Measure distance to target on trigger enter
    private void OnTriggerEnter(Collider other)
    {
        float distance = DistanceToTarget(other.transform);
    }
}
