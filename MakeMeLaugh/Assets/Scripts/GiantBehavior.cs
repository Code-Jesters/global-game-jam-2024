using UnityEngine;
using Unity.Netcode;

public class GiantBehavior : NetworkBehaviour
{
    public GameObject LeftLegPivot;
    public GameObject RightLegPivot;

    void Start()
    {
        //
    }

    void Update()
    {
        // only server should preform logic update
        if (!IsServer) { return; }

        // For now do a procedural walk forward
        var dir = new Vector3(0.0f, 0.0f, 1.0f);
        var speed = 1.0f;
        transform.position = transform.position + dir * speed * Time.deltaTime;

        // angle his legs
        var maxAngle = 10.0f;
        var xAngle = maxAngle * Mathf.Sin(Time.time);
        LeftLegPivot.transform.localRotation = Quaternion.Euler(xAngle, 0.0f, 0.0f);
        RightLegPivot.transform.localRotation = Quaternion.Euler(-xAngle, 0.0f, 0.0f);

        // swing forward and back
        var swingForwardBackAngle = xAngle * 0.5f;
        var swingLeftRightAngle = -xAngle * 0.5f;
        transform.localRotation = Quaternion.Euler(swingForwardBackAngle, swingLeftRightAngle, 0.0f);
    }
}
