using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class EditorUIThemeApplier : EditorWindow
{
    private Label m_SelectedName;
    private ObjectField m_ThemeFileField;

    [MenuItem("Tools/Theme applier")]
    static void Open()
    {
        GetWindow<EditorUIThemeApplier>();
    }

    private void CreateGUI()
    {
        m_SelectedName = new Label();
        OnSelectionChange();
        
        var applyButton = new Button();
        applyButton.text = "Apply";
        applyButton.clicked += () =>
        {
            ApplyTheme();    
        };

        m_ThemeFileField = new ObjectField("Theme Data");
        m_ThemeFileField.allowSceneObjects = true;
        m_ThemeFileField.objectType = typeof(UIThemeData);
        m_ThemeFileField.RegisterValueChangedCallback(evt =>
        {
            bool selectionIsScene = Selection.activeGameObject != null && Selection.activeGameObject.scene.IsValid(); 
            
            applyButton.SetEnabled(selectionIsScene && m_ThemeFileField.value != null);
        });
        
        rootVisualElement.Add(m_SelectedName);
        rootVisualElement.Add(m_ThemeFileField);
        rootVisualElement.Add(applyButton);
    }

    void ApplyTheme()
    {
        var uiTheme = m_ThemeFileField.value as UIThemeData;
        
        Undo.RegisterFullObjectHierarchyUndo(Selection.activeGameObject, "Applying Theme");
        
        uiTheme.ApplyThemeToHierarchy(Selection.activeTransform);
        
        EditorUtility.SetDirty(Selection.activeGameObject);
    }

    private void OnSelectionChange()
    {
        m_SelectedName.text = $"CurrentSelected : {Selection.activeGameObject}";
    }
}
