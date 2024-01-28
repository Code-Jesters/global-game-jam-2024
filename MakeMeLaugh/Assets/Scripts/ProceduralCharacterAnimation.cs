using UnityEngine;

public class ProceduralCharacterAnimation : MonoBehaviour
{
    Animator animator;

    public Transform LeftFootTarget;
    public Transform RightFootTarget;
    public Transform LeftHandTarget;
    public Transform RightHandTarget;

    float LeftFootTargetWeight = 0.0f;
    float RightFootTargetWeight = 0.0f;
    float LeftHandTargetWeight = 0.0f;
    float RightHandTargetWeight = 0.0f;

    void Start()
    {
        animator = GetComponent<Animator>();

        // This works, as an example
        //var target = transform.Find("Skeleton/Hips/Left_UpperLeg/Left_LowerLeg/Left_Foot");
    }

    void Update()
    {
        //animator.enabled = false;
    }

    void UpdateTargetIK(Transform target, AvatarIKGoal ikGoal, ref float targetWeight)
    {
        if (target != null)
        {
            animator.SetIKPosition(ikGoal, target.position);
        }
        targetWeight += ((target != null) ? 1.0f : -1.0f) * Time.deltaTime;
        targetWeight = Mathf.Clamp(targetWeight, 0.0f, 1.0f);
        animator.SetIKPositionWeight(ikGoal, targetWeight);
    }

    void OnAnimatorIK(int layerIndex)
    {
        UpdateTargetIK(LeftFootTarget, AvatarIKGoal.LeftFoot, ref LeftFootTargetWeight);
        UpdateTargetIK(RightFootTarget, AvatarIKGoal.RightFoot, ref RightFootTargetWeight);
        UpdateTargetIK(LeftHandTarget, AvatarIKGoal.LeftHand, ref LeftHandTargetWeight);
        UpdateTargetIK(RightHandTarget, AvatarIKGoal.RightHand, ref RightHandTargetWeight);
    }
}
