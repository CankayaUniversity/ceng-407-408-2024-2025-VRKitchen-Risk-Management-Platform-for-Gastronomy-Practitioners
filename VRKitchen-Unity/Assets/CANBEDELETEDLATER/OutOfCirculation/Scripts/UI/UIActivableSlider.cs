using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

public class UIActivableSlider : Slider, ISubmitHandler, ICancelHandler, UIComponentInfo.IComponentInfoDataProvider
{
    public float IncrementValue = 0.1f;
    public Color ToggledColor = Color.blue;

    public UIComponentInfo.ComponentInfoData ElementData;
    
    private bool m_Toggled;
    private Color m_OriginalSelectedColor;
    
    protected override void Awake()
    {
        base.Awake();

        m_OriginalSelectedColor = colors.selectedColor;
        
        onValueChanged.AddListener(val =>
        {
            if(!Application.isPlaying)
                return;
            
            UAP_AccessibilityManager.Say($"{Mathf.RoundToInt(normalizedValue * 100)} percent", true, true, UAP_AudioQueue.EInterrupt.All);
        });
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (!m_Toggled)
        {
            //if we aren't toggled, we navigate normally
            switch (eventData.moveDir)
            {
                case MoveDirection.Up :
                    Navigate_Cpy(eventData, FindSelectableOnUp());
                    break;
                case MoveDirection.Down :
                    Navigate_Cpy(eventData, FindSelectableOnDown());
                    break;
                case MoveDirection.Right :
                    Navigate_Cpy(eventData, FindSelectableOnRight());
                    break;
                case MoveDirection.Left :
                    Navigate_Cpy(eventData, FindSelectableOnLeft());
                    break;
            }
        }
        else
        {
            //if we are toggled, we modified the value of the slider based on it's orientation
            switch (direction)
            {
                case Direction.BottomToTop :
                    if (eventData.moveDir == MoveDirection.Up)
                        normalizedValue += IncrementValue;
                    else if(eventData.moveDir == MoveDirection.Down)
                        normalizedValue -= IncrementValue;
                    break;
                case Direction.TopToBottom :
                    if (eventData.moveDir == MoveDirection.Up)
                        normalizedValue -= IncrementValue;
                    else if(eventData.moveDir == MoveDirection.Down)
                        normalizedValue += IncrementValue;
                    break;
                case Direction.LeftToRight :
                    if (eventData.moveDir == MoveDirection.Right)
                        normalizedValue += IncrementValue;
                    else if(eventData.moveDir == MoveDirection.Left)
                        normalizedValue -= IncrementValue;
                    break;
                case Direction.RightToLeft :
                    if (eventData.moveDir == MoveDirection.Right)
                        normalizedValue -= IncrementValue;
                    else if(eventData.moveDir == MoveDirection.Left)
                        normalizedValue += IncrementValue;
                    break;
            }
        }
    }
    
    //copy of the private navigate function 
    void Navigate_Cpy(AxisEventData eventData, Selectable sel)
    {
        if (sel != null && sel.IsActive())
            eventData.selectedObject = sel.gameObject;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        m_Toggled = !m_Toggled;
        
        if(m_Toggled)
            UAP_AccessibilityManager.Say($"Begin edit slider. Current value {Mathf.RoundToInt(normalizedValue*100)} percent");

        UpdateColors();
    }

    public void OnCancel(BaseEventData eventData)
    {
        m_Toggled = false;
        UpdateColors();
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);

        m_Toggled = false;
        UpdateColors();
    }

    void UpdateColors()
    {
        var c = colors;
        c.selectedColor = m_Toggled ? ToggledColor : m_OriginalSelectedColor;
        colors = c;
    }

    public UIComponentInfo.ComponentInfoData GetComponentInfo()
    {
        return ElementData;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(UIActivableSlider), editorForChildClasses: false)]
public class UIActivableSliderEditor : SliderEditor
{
    private SerializedProperty m_IncrementValueProperty;
    private SerializedProperty m_ToggleColorProperty;
    private SerializedProperty m_ElementDataProperty;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_IncrementValueProperty = serializedObject.FindProperty(nameof(UIActivableSlider.IncrementValue));
        m_ToggleColorProperty = serializedObject.FindProperty(nameof(UIActivableSlider.ToggledColor));
        m_ElementDataProperty = serializedObject.FindProperty(nameof(UIActivableSlider.ElementData));
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.PropertyField(m_IncrementValueProperty);
        EditorGUILayout.PropertyField(m_ToggleColorProperty);
        EditorGUILayout.PropertyField(m_ElementDataProperty);
        
        serializedObject.ApplyModifiedProperties();
    }
}

#endif