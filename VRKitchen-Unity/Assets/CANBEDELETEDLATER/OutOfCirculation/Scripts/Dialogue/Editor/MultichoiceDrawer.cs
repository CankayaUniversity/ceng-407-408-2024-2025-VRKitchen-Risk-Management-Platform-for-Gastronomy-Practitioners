using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MultichoiceBehaviour))]
public class MultichoiceDrawer : PropertyDrawer
{
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        var choices = property.FindPropertyRelative("conversationChoices");
        
        return   EditorGUI.GetPropertyHeight(choices, true);
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        var choices = property.FindPropertyRelative("conversationChoices");

        EditorGUI.PropertyField(position, choices, true);
    }
}
