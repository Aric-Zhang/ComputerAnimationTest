using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class UnrealConstraint : MonoBehaviour
{
    public string constraintName;
    [SerializeField]
    BakedConstraintData bakedConstraintData;

    /// <summary>
    /// 在child空间下的constraintRotation
    /// </summary>
    public Quaternion ChildAnchorRotation {
        get => bakedConstraintData.ChildAnchorRotation;
    }

    /// <summary>
    /// 在parent空间下的constraintRotation
    /// </summary>
    public Quaternion ParentAnchorRotation {
        get => bakedConstraintData.ParentAnchorRotation;
    }

    public ConfigurableJoint Joint
    {
        get {
            if (!bakedConstraintData.configurableJoint) {
                bakedConstraintData.configurableJoint = GetComponent<ConfigurableJoint>();
            }
            return bakedConstraintData.configurableJoint;
        }
    }

    private void Reset()
    {
        ConfigurableJoint configurableJoint = GetComponent<ConfigurableJoint>();
        if (bakedConstraintData == null) {
            bakedConstraintData = new BakedConstraintData();
        }
        if (configurableJoint)
        {
            bakedConstraintData.configurableJoint = configurableJoint;
        }
    }

    public void SetConnectedBodyWorldSpaceRotationTarget(Quaternion worldRotation) {
        bakedConstraintData.SetConnectedBodyWorldSpaceRotationTarget(worldRotation);
    }

    public void SetConnectedBodyWorldSpaceRotation(Quaternion worldRotation , float maxTranslationSpeed = float.MaxValue) {
        bakedConstraintData.SetConnectedBodyWorldSpaceRotation(worldRotation, maxTranslationSpeed);
    }

    public void SetConnectedBodyPositionAndRotationDirectly(Vector3 worldPosition, Quaternion worldRotation, float maxTranslationSpeed = float.MaxValue) {
        bakedConstraintData.SetConnectedBodyPositionAndRotationDirectly(worldPosition, worldRotation, maxTranslationSpeed);
    }
}

[System.Serializable]
public class BakedConstraintData {
    public ConfigurableJoint configurableJoint;
    [SerializeField]
    Quaternion initialRotationOffset = Quaternion.identity;
    [SerializeField]
    Quaternion initalLocalRotation = Quaternion.identity;

    /// <summary>
    /// 在child空间下的constraintRotation
    /// </summary>
    public Quaternion ChildAnchorRotation
    {
        get => initialRotationOffset;
    }

    /// <summary>
    /// 在parent空间下的constraintRotation
    /// </summary>
    public Quaternion ParentAnchorRotation
    {
        get => initalLocalRotation;
    }

    //to-do:用rigidbody的velocity写一套motor控制，作为motor的另外一环
    public void SetConnectedBodyWorldSpaceRotationTarget(Quaternion worldRotation)
    {
        ConfigurableJoint joint = configurableJoint;
        Quaternion constraintWorldRotation = configurableJoint.transform.rotation * ParentAnchorRotation;
        Quaternion constraintSpaceSourceRotation = Quaternion.Inverse(constraintWorldRotation) * worldRotation;
        Quaternion rotationTarget = constraintSpaceSourceRotation * ChildAnchorRotation;
        joint.targetRotation = rotationTarget;
    }

    //Rotation Only KeyFramed Controller
    public void SetConnectedBodyWorldSpaceRotation(Quaternion worldRotation, float maxTranslationSpeed = float.MaxValue) {
        Rigidbody connectedBody = configurableJoint.connectedBody;

        Vector3 connectedAnchorWorldPos = connectedBody.transform.TransformPoint(configurableJoint.connectedAnchor);
        Vector3 connectedAnchorWorldOffset = connectedAnchorWorldPos - connectedBody.position;

        /*clamp connected pos
        bool xLocked = configurableJoint.xMotion == ConfigurableJointMotion.Locked;
        bool yLocked = configurableJoint.yMotion == ConfigurableJointMotion.Locked;
        bool zLocked = configurableJoint.zMotion == ConfigurableJointMotion.Locked;*/

        if (true) {
            Vector3 anchorWorldPos = configurableJoint.transform.TransformPoint(configurableJoint.anchor);
            connectedAnchorWorldPos = anchorWorldPos;
        }

        Quaternion currentConnectedRotation = connectedBody.rotation;
        Quaternion deltaRotation = worldRotation * Quaternion.Inverse(currentConnectedRotation);
        Vector3 newConnectedAnchorOffset = deltaRotation * connectedAnchorWorldOffset;
        Vector3 newConnectedBodyPosition = connectedAnchorWorldPos - newConnectedAnchorOffset;
        //Debug.DrawLine(newConnectedBodyPosition, connectedAnchorWorldPos);

        Vector3 deltaPosition = newConnectedBodyPosition - connectedBody.position;
        float maxTransitionNextFixedFrame = maxTranslationSpeed * Time.fixedDeltaTime;

        if (maxTransitionNextFixedFrame > deltaPosition.magnitude)
        {
            connectedBody.position = newConnectedBodyPosition;
            //connectedBody.transform.rotation = worldRotation;
            connectedBody.rotation = worldRotation;
        }
        else {
            float ratio = maxTransitionNextFixedFrame / deltaPosition.magnitude;
            connectedBody.position = connectedBody.position + deltaPosition * ratio;
            Vector3 axis;
            float angle;
            deltaRotation.ToAngleAxis(out angle, out axis);
            Debug.DrawRay(connectedBody.position, axis * 10);
            connectedBody.rotation = Quaternion.Lerp(Quaternion.identity, deltaRotation, ratio) * connectedBody.rotation;
        }
        //connectedBody.velocity = 2 * deltaPosition / Time.fixedDeltaTime;

        connectedBody.velocity = Vector3.zero;
        //抵消重力()
        if (connectedBody.useGravity)
        {
            connectedBody.AddForce(-Physics.gravity, ForceMode.Acceleration);
        }
    }

    //to-do:加上最大插值限制
    public void SetConnectedBodyPositionAndRotationDirectly(Vector3 worldPosition, Quaternion worldRotation, float maxTranslationSpeed = float.MaxValue) {
        Rigidbody connectedBody = configurableJoint.connectedBody;
        Vector3 deltaPosition = worldPosition - connectedBody.position;
        Quaternion deltaRotation = worldRotation * Quaternion.Inverse(connectedBody.rotation);
        float maxTransitionNextFixedFrame = maxTranslationSpeed * Time.fixedDeltaTime;
        if (maxTransitionNextFixedFrame > deltaPosition.magnitude)
        {
            configurableJoint.connectedBody.position = worldPosition;
            configurableJoint.connectedBody.rotation = worldRotation;
        }
        else
        {
            float ratio = maxTransitionNextFixedFrame / deltaPosition.magnitude;
            connectedBody.position = connectedBody.position + deltaPosition * ratio;
            connectedBody.rotation = Quaternion.Lerp(Quaternion.identity, deltaRotation, ratio) * connectedBody.rotation;
        }
        
        connectedBody.angularVelocity = Vector3.zero;
        connectedBody.velocity = Vector3.zero;
        if (configurableJoint.connectedBody.useGravity)
        {
            configurableJoint.connectedBody.AddForce(-Physics.gravity, ForceMode.Acceleration);
        }
    }
    
}
