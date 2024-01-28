using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MakeMeLaugh.Assets.Scripts.Bird;

// TODO: Add additional behaviors: If too far, switch to follow/chase mode, if too close, switch back to circle.
public class BirdAgent : MonoBehaviour
{
    // Speed for direct follow and attacks
    [SerializeField]
    private float movementSpeed = 8.0f;
    
    // Speed for circling target
    [SerializeField]
    private float flyCircleSpeed = 8.0f;

    public float flyCircleSpeedBase = 9.0f;
    public float flyCircleSpeedVariance = 5.0f;

    public float attackToCircleDistanceThreshold = 5.0f;

    public BirdBehaviorState moveState;

    public Transform target;

    // Update is called once per frame
    void Update()
    {
        switch (moveState)
        {
            case BirdBehaviorState.Follow:
                FlyTowardsTarget();
                break;
            case BirdBehaviorState.Circle:
                FlyAroundTarget();
                break;
            case BirdBehaviorState.Attack:
                FlyTowardsTarget();
                break;
            default:
                break;
        }
    }

    // TODO: Prevent bird from flying too close and gimbal locking/spinning.
    public void FlyTowardsTarget()
    {
        transform.LookAt(target);
        transform.position += transform.forward * movementSpeed * Time.deltaTime;
        float distanceToTarget = (transform.position - target.transform.position).magnitude;
        if (distanceToTarget < attackToCircleDistanceThreshold)
        {
            moveState = BirdBehaviorState.Circle;
        }
    }
    
    public void FlyAroundTarget()
    {
        Vector3 birdToTargetVector = target.position - transform.position;
        Vector3 birdCircleDirection = Vector3.Cross(birdToTargetVector, Vector3.up).normalized;
        transform.rotation = Quaternion.LookRotation(birdCircleDirection);
        transform.RotateAround (target.position, Vector3.up, flyCircleSpeed * Time.deltaTime);
    }

    public void SetRandomSpeedInRange()
    {
        float min = flyCircleSpeedBase - flyCircleSpeedVariance;
        float max = flyCircleSpeedBase + flyCircleSpeedVariance;
        flyCircleSpeed = UnityEngine.Random.Range(min, max);
    }
}
