using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhysicalAnimation))]
public class PhysicalAnimationEditor : Editor
{
    private void OnSceneGUI()
    {
        PhysicalAnimation physicalAnimation = target as PhysicalAnimation;
        SerializedProperty s_p_constrains = serializedObject.FindProperty("constraints");
        int constraint_count = s_p_constrains.arraySize;
        for (int i = 0; i < constraint_count; i++) {
            SerializedProperty s_p_unrealConstraint = s_p_constrains.GetArrayElementAtIndex(i);
            UnrealConstraint unrealConstraint = s_p_unrealConstraint.objectReferenceValue as UnrealConstraint;
            SerializedObject s_o_unrealConstraint = new SerializedObject(unrealConstraint);
            SerializedProperty s_p_bakedConstraintData = s_o_unrealConstraint.FindProperty("bakedConstraintData");
            UnrealConstraintEditor.DrawConstraint(s_p_bakedConstraintData, 0.5f);
        }
    }
}
