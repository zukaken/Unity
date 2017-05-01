using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CanEditMultipleObjects]
[CustomEditor(typeof(FlowNode), true)]
public class EditorFlowNode : Editor
{
    ReorderableList m_list;
    void OnEnable()
    {
        m_list = new ReorderableList(
            serializedObject,
            serializedObject.FindProperty("NodeLinks")
        );

        m_list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var motion = m_list.serializedProperty.GetArrayElementAtIndex(index);
            var displayName = string.Format("{0} : {1}",
                motion.FindPropertyRelative("dstPin").intValue,
                motion.FindPropertyRelative("srcPin").intValue
            );
            Rect rect1 = new RectOffset(0, 10, 0, 0).Remove(new Rect(rect.x, rect.y, rect.width / 3, rect.height));
            Rect rect2 = new RectOffset(10, 10, 0, 0).Remove(new Rect(rect1.xMax, rect.y, rect.width / 3, rect.height));
            Rect rect3 = new RectOffset(10, 10, 0, 0).Remove(new Rect(rect2.xMax, rect.y, rect.width / 3, rect.height));
            EditorGUI.LabelField(rect1, "Link " + index);
            EditorGUI.LabelField(rect2, "Dst " + motion.FindPropertyRelative("dstPin").intValue);
            EditorGUI.LabelField(rect3, "Src " + motion.FindPropertyRelative("srcPin").intValue);
        };
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty property = serializedObject.GetIterator();

        bool child = true;
        GUI.enabled = false;
        property = serializedObject.FindProperty("m_Script");
        EditorGUILayout.PropertyField(property);
        GUI.enabled = true;
        m_list.DoLayoutList();
        //while(property.NextVisible(child))
        //{
        //    child = false;
        //    if (property.name == "m_Script")
        //    {

        //    }
        //    else if(property.name == "NodeLinks")
        //    {
        //        var type = property.propertyType;
        //        property.
        //    }
        //}

        serializedObject.ApplyModifiedProperties();
    }
}
