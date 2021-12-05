using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class UnrealConstraint : MonoBehaviour
{
    [SerializeField]
    ConfigurableJoint configurableJoint;

    [HideInInspector]
    [SerializeField]
    Quaternion initialRotationOffset = Quaternion.identity;
    [SerializeField]
    Quaternion initalLocalRotation = Quaternion.identity;

    /// <summary>
    /// 在child空间下的constraintRotation
    /// </summary>
    public Quaternion ChildAnchorRotation {
        get => initialRotationOffset;
    }

    /// <summary>
    /// 在parent空间下的constraintRotation
    /// </summary>
    public Quaternion ParentAnchorRotation {
        get => initalLocalRotation;
    }

    public ConfigurableJoint Joint
    {
        get { 
            if(!configurableJoint) configurableJoint = GetComponent<ConfigurableJoint>();
            return configurableJoint;
        }
    }

    private void Reset()
    {
        configurableJoint = GetComponent<ConfigurableJoint>();
    }

    public void SetConnectedBodyWorldSpaceRotationTarget(Quaternion worldRotation) {
        ConfigurableJoint joint = Joint;
        Quaternion constraintWorldRotation = transform.rotation * ParentAnchorRotation;
        Quaternion constraintSpaceSourceRotation = Quaternion.Inverse(constraintWorldRotation) * worldRotation;
        Quaternion rotationTarget = constraintSpaceSourceRotation * ChildAnchorRotation;
        joint.targetRotation = rotationTarget;
    }

}
