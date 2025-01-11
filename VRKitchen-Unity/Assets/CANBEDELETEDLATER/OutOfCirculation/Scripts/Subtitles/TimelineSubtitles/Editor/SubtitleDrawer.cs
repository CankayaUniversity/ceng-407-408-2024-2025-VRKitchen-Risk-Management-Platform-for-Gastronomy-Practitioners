using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SubtitleDrawer : PropertyDrawer
{
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        int fieldCount = 4;
        // TODO: complete me
        return fieldCount * EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        
    }
}
