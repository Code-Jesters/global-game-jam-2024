using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MakeMeLaugh.Assets.Scripts.Bird;

public class BirdAgent : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed = 2.0f;

    [SerializeField]
    private Transform target;

    [SerializeField]
    public BirdBehaviorState moveState;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (moveState == BirdBehaviorState.Following)
        {
            FlyTowardsPlayer();
        }
        if (moveState == BirdBehaviorState.Attacking)
        {
            FlyTowardsPlayer();
        }
        
    }

    public void FlyTowardsPlayer()
    {
        transform.LookAt(target);
        transform.position += transform.forward * movementSpeed * Time.deltaTime;
    }
}
