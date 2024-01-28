using UnityEngine;

public class ProceduralCharacterAnimation : MonoBehaviour
{
    public Transform objToPickUp;
    Animator animator;

    public Transform Left_UpperLeg;
    public Transform Head;
    public float Weight = 1.0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        //var target = transform.Find("Skeleton/Hips/Left_UpperLeg/Left_LowerLeg/Left_Foot");
        Left_UpperLeg = transform.Find("Skeleton/Hips/Left_UpperLeg");
        Head = transform.Find("Skeleton/Hips/Spine/Chest/UpperChest/Neck/Head");
        objToPickUp = Head;

        Debug.Log("=== DJMC ProceduralCharacterAnimation ===");
        for (var i = 0; i < animator.parameterCount; ++i)
        {
            var parameter = animator.GetParameter(i);
            string valueStr = "";
            if (parameter.type == AnimatorControllerParameterType.Float)
            {
                valueStr = "" + animator.GetFloat(parameter.name);
            }
            else if (parameter.type == AnimatorControllerParameterType.Int)
            {
                //valueStr = "" + animator.GetInt(parameter.name);
                valueStr = "<TBD>";
            }
            else if (parameter.type == AnimatorControllerParameterType.Bool)
            {
                valueStr = "" + animator.GetBool(parameter.name);
            }
            else if (parameter.type == AnimatorControllerParameterType.Trigger)
            {
                //valueStr = "" + animator.GetTrigger(parameter.name);
                valueStr = "<TBD>";
            }
            Debug.Log("  " + parameter.name + "<" + parameter.type.ToString() + ">: " + valueStr);
        }
    }

    void Update()
    {
        //animator.enabled = false;
    }

    void OnAnimatorIK(int layerIndex)
    {
        //Debug.Log("OnAnimatorIK(" + layerIndex + ")");
        //float reach = animator.GetFloat("RightHandReach");
        float reach = Weight;
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, reach);
        if (objToPickUp != null)
        {
            animator.SetIKPosition(AvatarIKGoal.RightHand, objToPickUp.position);
        }
    }
}
