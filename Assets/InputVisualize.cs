using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputVisualize : MonoBehaviour
{
    public Transform rotationSource;

    public Vector3 worldSpaceForward = Vector3.forward;
    public UnrealConstraint constraint;

    private void Start()
    {
        ConfigurableJoint joint = constraint.Joint;
        joint.targetRotation = Quaternion.LookRotation(worldSpaceForward);
        joint.rotationDriveMode = RotationDriveMode.Slerp;
    }

    private void Update()
    {
        constraint.SetConnectedBodyWorldSpaceRotationTarget(rotationSource.rotation);
    }

    private void OnDrawGizmos()
    {


    }
}
