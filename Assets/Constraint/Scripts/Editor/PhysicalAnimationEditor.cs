using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhysicalAnimation))]
public class PhysicalAnimationEditor : Editor
{
    private int deepestIndent = 0;
    PhysicalBoneEditorData[] boneData;
    private int[] parentBoneStack;

    private void OnEnable()
    {
        RegenerateInspectorSkeletonHierarchy();
    }

    public override void OnInspectorGUI()
    {
        InspectorSkeletonHierarchy();

        SerializedProperty s_p_constraints = serializedObject.FindProperty("constraints");
        //EditorGUILayout.PropertyField(s_p_constraints);

        if (GUILayout.Button("Generate Bodies")) {

        }
    }

    void InspectorSkeletonHierarchy()
    {
        if (boneData.Length > 0)
        {
            parentBoneStack[0] = -1;
            int new_deepest_indent = 0;
            int indent = 0;

            for (int i = 0; i < boneData.Length; i++)
            {
                PhysicalBoneEditorData currentBoneData = boneData[i];
                //骨骼被删除
                if (!currentBoneData.BoneTransform) {
                    RegenerateInspectorSkeletonHierarchy();
                    return;
                }
                //reset to top
                if (currentBoneData.parentIndex == -1)
                    indent = 0;
                //骨骼层级结构改变
                else if (boneData[currentBoneData.parentIndex].BoneTransform != currentBoneData.BoneTransform.parent) {
                    RegenerateInspectorSkeletonHierarchy();
                    return;
                }
                //dive
                else if (currentBoneData.parentIndex == i - 1)
                {
                    parentBoneStack[++indent] = i - 1;
                }
                //pop up
                else if (currentBoneData.parentIndex != parentBoneStack[indent])
                {
                    int parentIndex = boneData[i].parentIndex;
                    while (indent > 0)
                    {
                        if (parentIndex == parentBoneStack[--indent]) break;
                    }
                }
                if (indent > 0)
                {
                    PhysicalBoneEditorData parentBoneData = boneData[boneData[i].parentIndex];
                    currentBoneData.ancestorsAllExpand = parentBoneData.ancestorsAllExpand && parentBoneData.expand;
                }
                else
                {
                    currentBoneData.ancestorsAllExpand = true;
                }
                if (currentBoneData.ancestorsAllExpand)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(15 * indent);
                    GUILayout.BeginVertical();

                    currentBoneData.expand = EditorGUILayout.Foldout(currentBoneData.expand, boneData[i].BoneTransform.name, true);

                    if (currentBoneData.expand)
                    {
                        SerializedObject s_o = new SerializedObject(boneData[i].constraints);
                        SerializedProperty s_p_constraints = s_o.FindProperty("constraints");

                        SerializedProperty s_p_rigidbody = s_o.FindProperty("rigidbody");
                        if (s_p_rigidbody.objectReferenceValue != null)
                        {
                            EditorGUILayout.PropertyField(s_p_rigidbody, new GUIContent(" Rigidbody"), new GUILayoutOption[] { GUILayout.ExpandWidth(true)});
                        }
                        for (int j = 0; j < s_p_constraints.arraySize; j++)
                        {
                            SerializedProperty s_p_unrealConstraint = s_p_constraints.GetArrayElementAtIndex(j);
                            UnrealConstraint constraint = s_p_unrealConstraint.objectReferenceValue as UnrealConstraint;
                            if (constraint.constraintName == "")
                            {
                                name = constraint.transform.name;
                                if (constraint.Joint.connectedBody)
                                {
                                    name = name + " to " + constraint.Joint.connectedBody.name;
                                }
                                constraint.constraintName = name;
                            }
                            EditorGUILayout.PropertyField(s_p_unrealConstraint, new GUIContent(" Constraint " + j.ToString()), new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
                        }
                        SerializedProperty s_p_colliders = s_o.FindProperty("colliders");
                        for (int j = 0; j < s_p_colliders.arraySize; j++)
                        {
                            SerializedProperty s_p_collider = s_p_colliders.GetArrayElementAtIndex(j);
                            EditorGUILayout.PropertyField(s_p_collider, new GUIContent(" Collider " + j.ToString()), new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
                        }
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(15 * (deepestIndent - indent));
                    GUILayout.EndHorizontal();
                    if (indent > new_deepest_indent) new_deepest_indent = indent;
                }
            }
            //next frame
            deepestIndent = new_deepest_indent;
        }


    }

    void RegenerateInspectorSkeletonHierarchy() {
        Transform[] childBones = (target as PhysicalAnimation).GetComponentsInChildren<Transform>();
        boneData = new PhysicalBoneEditorData[childBones.Length];
        for (int i = 0; i < childBones.Length; i++)
        {
            Transform bone_transform = childBones[i];
            boneData[i] = new PhysicalBoneEditorData(bone_transform);
            if (bone_transform.parent)
            {
                for (int j = 0; j < i; j++)
                {
                    if (childBones[j] == bone_transform.parent)
                    {
                        boneData[i].parentIndex = j;
                        break;
                    }
                }
            }
            boneData[i].constraints.constraints = bone_transform.GetComponents<UnrealConstraint>();
            boneData[i].constraints.rigidbody = bone_transform.GetComponent<Rigidbody>();
            boneData[i].constraints.colliders = bone_transform.GetComponents<Collider>();
        }
        boneData[0].ancestorsAllExpand = true;
        parentBoneStack = new int[childBones.Length];
    }

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

    [System.Serializable]
    public class PhysicalBoneEditorData {
        Transform transform;
        public bool expand = true;
        public bool ancestorsAllExpand;
        public int parentIndex = -1;
        public UnrealConstraintWrapper constraints;

        public Transform BoneTransform {
            get => transform;
        }

        public PhysicalBoneEditorData(Transform transform) {
            this.transform = transform;
            parentIndex = -1;
            constraints = ScriptableObject.CreateInstance<UnrealConstraintWrapper>();
        }
    }

    public class UnrealConstraintWrapper : ScriptableObject {
        public UnrealConstraint[] constraints;
        public Rigidbody rigidbody;
        public Collider[] colliders;
    }
}
