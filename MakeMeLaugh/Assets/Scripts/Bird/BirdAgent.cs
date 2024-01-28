using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MakeMeLaugh.Assets.Scripts.Bird;

public class BirdAgent : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed = 2.0f;

    // [SerializeField]
    // private Transform target;
    public Transform target;

    [SerializeField]
    public BirdBehaviorState moveState;

    public float flyCircleSpeed = 4.0f;

    // Update is called once per frame
    void Update()
    {
        switch (moveState)
        {
            case BirdBehaviorState.Following:
                FlyTowardsTarget();
                break;
            case BirdBehaviorState.Circling:
                FlyAroundTarget();
                break;
            case BirdBehaviorState.Attacking:
                FlyTowardsTarget();
                break;
            default:
                break;
        }
    }

    // TODO: Prevent bird from flying to close and gimbal locking/spinning.
    // (Switch to circle or attack mode if close enough.)
    public void FlyTowardsTarget()
    {
        transform.LookAt(target);
        transform.position += transform.forward * movementSpeed * Time.deltaTime;
    }
    
    public void FlyAroundTarget()
    {
        Vector3 birdToTargetVector = target.position - transform.position;
        Vector3 birdCircleDirection = Vector3.Cross(birdToTargetVector, Vector3.up).normalized;
        transform.rotation = Quaternion.LookRotation(birdCircleDirection);
        transform.RotateAround (target.position, Vector3.up, flyCircleSpeed * Time.deltaTime);
    }
}
