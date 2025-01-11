using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

/// <summary>
/// A button that toggle between two state, changing its look based on its state. Easier to do than trying to twist
/// a toggle in acting like a button. 
/// </summary>
public class UIToggleButton : Button, UIComponentInfo.IComponentStateDataProvider, UIRoot.IUIInitHandler
{
    [Header("Toggle Button Datas")]
    public Image ToggleGraphic;

    public Sprite OnSprite;
    public Sprite OffSprite;

    [Tooltip("Override either the Colors or Sprite State when toggled")]
    public bool ChangeState = false;
    
    public ColorBlock OffColorBlock;
    public SpriteState OffSpriteState;

    public string OnStateName = "On";
    public string OffStateName = "Off";
    
    public bool Toggled
    {
        get => m_Toggled;
        set => Toggle(value);
    }
    
    private bool m_Toggled;
    
    protected ColorBlock m_OriginalColor;
    protected SpriteState m_OriginalSpriteState;
    
    public void Init()
    {
        m_OriginalColor = colors;
        m_OriginalSpriteState = spriteState;
    }

    public void Toggle(bool value, bool notifyTTS = true)
    {
        m_Toggled = value;
        
        if(notifyTTS)
            UAP_AccessibilityManager.Say(GetState(), true, true, UAP_AudioQueue.EInterrupt.All);
        
        if (value)
        {
            if (ToggleGraphic != null)
            {
                ToggleGraphic.sprite = OnSprite;
            }

            if (ChangeState)
            {
                if (transition == Transition.ColorTint)
                    colors = m_OriginalColor;
                else if (transition == Transition.SpriteSwap)
                    spriteState = m_OriginalSpriteState;
            }
        }
        else
        {
            if (ToggleGraphic != null)
            {
                ToggleGraphic.sprite = OffSprite;
            }

            if (ChangeState)
            {
                if (transition == Transition.ColorTint)
                    colors = OffColorBlock;
                else if (transition == Transition.SpriteSwap)
                    spriteState = OffSpriteState;
            }
        }
    }
    
    public string GetState()
    {
        return m_Toggled ? OnStateName : OffStateName;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(UIToggleButton), false)]
public class UIToggleButtonEditor : ButtonEditor
{
    private SerializedProperty m_ToggleGraphicProperty;
    private SerializedProperty m_OnSpriteProperty;
    private SerializedProperty m_OffSpriteProperty;
    private SerializedProperty m_OffColorProperty;
    private SerializedProperty m_OffSpriteStateProperty;
    private SerializedProperty m_OnStateNameProperty;
    private SerializedProperty m_OffStateNameProperty;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_ToggleGraphicProperty = serializedObject.FindProperty(nameof(UIToggleButton.ToggleGraphic));
        m_OnSpriteProperty = serializedObject.FindProperty(nameof(UIToggleButton.OnSprite));
        m_OffSpriteProperty = serializedObject.FindProperty(nameof(UIToggleButton.OffSprite));
        m_OffColorProperty = serializedObject.FindProperty(nameof(UIToggleButton.OffColorBlock));
        m_OffSpriteStateProperty = serializedObject.FindProperty(nameof(UIToggleButton.OffSpriteState));
        m_OnStateNameProperty = serializedObject.FindProperty(nameof(UIToggleButton.OnStateName));
        m_OffStateNameProperty = serializedObject.FindProperty(nameof(UIToggleButton.OffStateName));
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.PropertyField(m_ToggleGraphicProperty);
        EditorGUILayout.PropertyField(m_OnSpriteProperty);
        EditorGUILayout.PropertyField(m_OffSpriteProperty);
        EditorGUILayout.PropertyField(m_OffColorProperty);
        EditorGUILayout.PropertyField(m_OffSpriteStateProperty);
        EditorGUILayout.PropertyField(m_OnStateNameProperty);
        EditorGUILayout.PropertyField(m_OffStateNameProperty);

        serializedObject.ApplyModifiedProperties();
    }
}

#endif
