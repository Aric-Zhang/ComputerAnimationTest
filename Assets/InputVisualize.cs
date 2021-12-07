using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputVisualize : MonoBehaviour
{
    public Transform rotationSource;
    //public Rigidbody constraintedChild;

    public Vector3 worldSpaceForward = Vector3.forward;
    public UnrealConstraint constraint;

    private void Start()
    {
        //ConfigurableJoint joint = constraint.Joint;
        //joint.targetRotation = Quaternion.LookRotation(worldSpaceForward);
        //joint.rotationDriveMode = RotationDriveMode.Slerp;
    }

    private void Update()
    {
        //constraint.SetConnectedBodyWorldSpaceRotationTarget(rotationSource.rotation);
        //constraintedChild.rotation = rotationSource.rotation;
    }

    private void FixedUpdate()
    {
        constraint.SetConnectedBodyWorldSpaceRotationTarget(rotationSource.rotation);
        //constraint.SetConnectedBodyWorldSpaceRotation(rotationSource.rotation ,20f);
        //constraintedChild.rotation = rotationSource.rotation;
        //constraintedChild.position = rotationSource.position;
    }

    private void OnDrawGizmos()
    {


    }
}
