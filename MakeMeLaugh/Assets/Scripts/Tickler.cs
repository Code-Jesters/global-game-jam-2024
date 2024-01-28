using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;

public class Tickler : NetworkBehaviour
{
    public ProceduralCharacterAnimation proceduralAnim;

    public GameObject leftHandTarget;
    public GameObject rightHandTarget;

    // variables to network
    NetworkVariable<bool> targeting = new NetworkVariable<bool>();
    NetworkVariable<Vector3> targetPos = new NetworkVariable<Vector3>();
    NetworkVariable<bool> leftHandActive = new NetworkVariable<bool>();
    NetworkVariable<bool> rightHandActive = new NetworkVariable<bool>();

    void Awake()
    {
        proceduralAnim = GetComponent<ProceduralCharacterAnimation>();
    }

    void Start()
    {
        // find hand targets
        //leftHandTarget = transform.Find("LeftHandTarget").gameObject;
        //rightHandTarget = transform.Find("RightHandTarget").gameObject;
        leftHandTarget = new GameObject("Left Hand Target");
        rightHandTarget = new GameObject("Right Hand Target");
    }

    // there might be a less verbose way to do all this, but this works, so doing it

    void SetTargetOnServer(Vector3 targetPosInput)
    {
        targeting.Value = true;
        targetPos.Value = targetPosInput;
    }

    void ClearTargetOnServer()
    {
        targeting.Value = false;
    }

    void SetLeftHandActiveOnServer(bool isActive)
    {
        leftHandActive.Value = isActive;
    }

    void SetRightHandActiveOnServer(bool isActive)
    {
        rightHandActive.Value = isActive;
    }

    // Public interface
    public void SetTarget(Transform target)
    {
        if (IsServer)
        {
            SetTargetOnServer(target.position);
        }
        else
        {
            SetTargetServerRpc(target.position);
        }
    }

    public void ClearTarget()
    {
        if (IsServer)
        {
            ClearTargetOnServer();
        }
        else
        {
            ClearTargetServerRPC();
        }
    }

    public void SetLeftHandActive(bool isActive)
    {
        if (IsServer)
        {
            SetLeftHandActiveOnServer(isActive);
        }
        else
        {
            SetLeftHandActiveServerRPC(isActive);
        }
    }

    public void SetRightHandActive(bool isActive)
    {
        if (IsServer)
        {
            SetRightHandActiveOnServer(isActive);
        }
        else
        {
            SetRightHandActiveServerRPC(isActive);
        }
    }

    // Server RPCs -- Code that is run on the Server, called by a Client.

    [ServerRpc]
    void SetTargetServerRpc(Vector3 targetPosInput)
    {
        SetTargetOnServer(targetPosInput);
    }

    [ServerRpc]
    void ClearTargetServerRPC()
    {
        ClearTargetOnServer();
    }

    [ServerRpc]
    void SetLeftHandActiveServerRPC(bool isActive)
    {
        SetLeftHandActiveOnServer(isActive);
    }

    [ServerRpc]
    void SetRightHandActiveServerRPC(bool isActive)
    {
        SetRightHandActiveOnServer(isActive);
    }

    // Main logic to update every frame

    void Update()
    {
        if (targeting.Value)
        {
            leftHandTarget.transform.position = targetPos.Value + transform.right * -0.5f;
            rightHandTarget.transform.position = targetPos.Value + transform.right * 0.5f;
            proceduralAnim.LeftHandTarget = leftHandTarget.transform;
            proceduralAnim.RightHandTarget = rightHandTarget.transform;
        }
        else
        {
            proceduralAnim.LeftHandTarget = null;
            proceduralAnim.RightHandTarget = null;
        }

        // also try to tickle
        var ticklesPerSecond = 5.0f;
        var amplitude = 0.5f; // -0.25f;
        var radians = ticklesPerSecond * Time.time * (2 * Mathf.PI);
        var cos = Mathf.Cos(radians);
        if (leftHandActive.Value)
        {
            leftHandTarget.transform.position +=
                cos * transform.right *  amplitude +
                cos * transform.up    * -amplitude;
        }
        if (rightHandActive.Value)
        {
            rightHandTarget.transform.position +=
                cos * transform.right * -amplitude +
                cos * transform.up    * -amplitude;
        }
    }

}
