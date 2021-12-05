using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierComponent : MonoBehaviour
{
    public CtrlPoint[] ctrlPoints;

    public int SegmentCount{
        get {
            if (ctrlPoints.Length < 1) return 0;
            return ctrlPoints.Length - 1;
        }
    }

    public Vector3 Evaluate(float t) {
        if (ctrlPoints.Length == 0) return transform.position;
        if (ctrlPoints.Length == 1) return ctrlPoints[0].position;
        t = Mathf.Clamp(t, 0, SegmentCount);
        int segment_index = (int)t;
        if (segment_index == SegmentCount) segment_index -= 1;
        Vector3 p0 = ctrlPoints[segment_index].position;
        Vector3 p1 = ctrlPoints[segment_index].OutTangent + p0;
        Vector3 p3 = ctrlPoints[segment_index + 1].position;
        Vector3 p2 = ctrlPoints[segment_index + 1].InTangent + p3;

        t = t - segment_index;
        float u = 1 - t;
        return p0 * u * u * u + 3 * p1 * u * u * t + 3 * p2 * u * t * t + p3 * t * t * t;
    }

    public Vector3 Evaluate(float t, int derivativeOrder = 0)
    {
        if (ctrlPoints.Length == 0) return transform.position;
        if (ctrlPoints.Length == 1) return ctrlPoints[0].position;
        t = Mathf.Clamp(t, 0, SegmentCount);
        int segment_index = (int)t;
        if (segment_index == SegmentCount) segment_index -= 1;
        Vector3[] p = new Vector3[4];
        p[0] = ctrlPoints[segment_index].position;
        p[1] = ctrlPoints[segment_index].OutTangent + p[0];
        p[3] = ctrlPoints[segment_index + 1].position;
        p[2] = ctrlPoints[segment_index + 1].InTangent + p[3];

        t = t - segment_index;
        float u = 1 - t;
        if (derivativeOrder < 0) derivativeOrder = 0;
        //原函数
        if (derivativeOrder == 0) return p[0] * u * u * u + 3 * p[1] * u * u * t + 3 * p[2] * u * t * t + p[3] * t * t * t;
        else if (derivativeOrder > 0) {
            Vector3[] q = new Vector3[3];
            q[0] = 3 * (p[1] - p[0]);
            q[1] = 3 * (p[2] - p[1]);
            q[2] = 3 * (p[3] - p[2]);
            //一阶导
            if (derivativeOrder == 1) {
                return q[0] * u * u + 2 * q[1] * t * u + q[2] * t * t;
            }
            else if (derivativeOrder > 1) {
                Vector3[] r = new Vector3[2];
                r[0] = 2 * (q[1] - q[0]);
                r[1] = 2 * (q[2] - q[1]);
                //二阶导
                if (derivativeOrder == 2)
                {
                    return r[0] * u + r[1] * t;
                }
                else if (derivativeOrder > 2) {
                    //三阶导
                    if (derivativeOrder == 3)
                    {
                        return r[1] - r[0];
                    }
                    //其他阶导
                    else if (derivativeOrder > 3) {
                        return Vector3.zero;
                    }
                }
            }
        }
        return Vector3.zero;
    }

    public Vector3 EvaluateDerivatives(float t) {
        if (ctrlPoints.Length == 0) return transform.position;
        if (ctrlPoints.Length == 1) return ctrlPoints[0].position;
        t = Mathf.Clamp(t, 0, SegmentCount);
        int segment_index = (int)t;
        if (segment_index == SegmentCount) segment_index -= 1;
        Vector3 p0 = ctrlPoints[segment_index].position;
        Vector3 p1 = ctrlPoints[segment_index].OutTangent + p0;
        Vector3 p3 = ctrlPoints[segment_index + 1].position;
        Vector3 p2 = ctrlPoints[segment_index + 1].InTangent + p3;

        Vector3 q0 = 3 * (p1 - p0);
        Vector3 q1 = 3 * (p2 - p1);
        Vector3 q2 = 3 * (p3 - p2);

        t = t - segment_index;
        float u = 1 - t;

        return q0 * u * u + 2 * q1 * t * u + q2 * t * t;
    }
}

[System.Serializable]
public class CtrlPoint
{
    public BezierPointType type;
    public Vector3 position;
    [SerializeField]
    Vector3 inTangent;
    [SerializeField]
    Vector3 outTangent;

    public Vector3 InTangent
    {
        get {
            if (type == BezierPointType.corner) return Vector3.zero;
            else return inTangent;
        }
        set {
            if (type != BezierPointType.corner) inTangent = value;
            if (value.sqrMagnitude > 0.001 && type == BezierPointType.smooth) {
                outTangent = value.normalized * (-1) * outTangent.magnitude;
            }
        }
    }

    public Vector3 OutTangent 
    {
        get {
            if (type == BezierPointType.corner) return Vector3.zero;
            if (type == BezierPointType.smooth)
            {
                if (inTangent.sqrMagnitude > 0.001)
                {
                    return inTangent.normalized * (-1) * outTangent.magnitude;
                }
            }
            return outTangent;
        }
        set {
            if (type == BezierPointType.smooth) {
                if (value.sqrMagnitude > 0.001) {
                    inTangent = value.normalized * (-1) * inTangent.magnitude;
                }
                outTangent = value;
            }
            if (type == BezierPointType.bezierCorner) outTangent = value;
        }
    }
}

public enum BezierPointType  { 
    corner,
    smooth,
    bezierCorner
}