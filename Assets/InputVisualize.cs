using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputVisualize : MonoBehaviour
{
    int frame = 0;
    Vector3 lastPos = Vector3.zero;
    public Transform inputObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 deltaPos = inputObject.position - lastPos;
        Debug.DrawRay(Vector3.right * 0.02f * frame, Vector3.up*deltaPos.magnitude,Color.red,5f);
        lastPos = inputObject.position;
        frame++;
    }
}
