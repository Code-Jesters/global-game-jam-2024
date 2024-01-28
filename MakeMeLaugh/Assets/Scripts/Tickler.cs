using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;

public class Tickler : NetworkBehaviour
{
    public ProceduralCharacterAnimation proceduralAnim;
    Transform tickleTarget;
    public GameObject leftHandTarget;
    public GameObject rightHandTarget;

    public bool leftHandActive, rightHandActive;

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

    public void SetTarget(Transform target)
    {
        tickleTarget = target;
    }

    public void ClearTarget()
    {
        tickleTarget = null;
    }

    void Update()
    {
        if (tickleTarget != null)
        {
            leftHandTarget.transform.position = tickleTarget.position + transform.right * -0.5f;
            rightHandTarget.transform.position = tickleTarget.position + transform.right * 0.5f;
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
        if (leftHandActive)
        {
            leftHandTarget.transform.position +=
                cos * transform.right *  amplitude +
                cos * transform.up    * -amplitude;
        }
        if (rightHandActive)
        {
            rightHandTarget.transform.position +=
                cos * transform.right * -amplitude +
                cos * transform.up    * -amplitude;
        }
    }

}
