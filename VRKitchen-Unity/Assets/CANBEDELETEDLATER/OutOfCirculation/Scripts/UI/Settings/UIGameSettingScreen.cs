using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIGameSettingScreen : UIScreen
{
    [Header("Accessibility References")]
    public Slider UIScaleSlider;
    public TextMeshProUGUI UIScaleValueDisplay;
    public Button UIScaleApply;
    
    public Toggle HighlightInteractiveObjectToggle;
    public Toggle UINarrationToggle;
    public Toggle SwitchOpenDislexicToggle;
    public Toggle UseDynamicCamera;
    
    [Header("Game Options References")] 
    public Toggle AutoPauseDialogueToggle;
    public TMP_Dropdown InputIconeSetDropdown;

    public override void Build()
    {
        UIScaleSlider.onValueChanged.RemoveAllListeners();
        UIScaleSlider.onValueChanged.AddListener(val =>
        {
            Dirty();
            UIScaleApply.gameObject.SetActive(true);
            UIScaleApply.interactable = true;
            UIScaleValueDisplay.text = val.ToString("n2");

            SetupScaleApplyNavigation();
        });

        UIScaleApply.gameObject.SetActive(false);
        UIScaleApply.interactable = false;
        UIScaleApply.onClick.RemoveAllListeners();
        UIScaleApply.onClick.AddListener(() =>
        {
            SaveSystem.CurrentSettings.UIScale = UIScaleSlider.value;
            UIScaleApply.gameObject.SetActive(false);
            UIScaleApply.interactable = false;
            
            VisualManager.RescaleUIToValue(SaveSystem.CurrentSettings.UIScale);
            UIToggleSidebar.Instance.UpdateState();
        });

        HighlightInteractiveObjectToggle.onValueChanged.AddListener(on =>
        {
            SaveSystem.CurrentSettings.HighlightInteractiveObject = on;
            Dirty();
        });
        
        UINarrationToggle.onValueChanged.AddListener(on =>
        {
            AccessibilityHelper.ToggleTTS(on);
            Dirty();
        });
        
        SwitchOpenDislexicToggle.onValueChanged.AddListener(on =>
        {
            SaveSystem.CurrentSettings.UseOpenDyslexicFont = on;
            SwitchAllFont();
            Dirty();
        });
        
        AutoPauseDialogueToggle.onValueChanged.AddListener(on =>
        {
            SaveSystem.CurrentSettings.PauseDialogue = on;
            Dirty();
        });
        
        UseDynamicCamera.onValueChanged.AddListener(on =>
        {
            SaveSystem.CurrentSettings.UseDynamicCameras = on;
            Dirty();
        });
        

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        var allSets = ControlManager.ControlIconSelector.GetSetList();
        for (int i = 0; i < allSets.Count; i++)
        {
            options.Add(new TMP_Dropdown.OptionData(allSets[i]));
        }
        InputIconeSetDropdown.options = options;

        InputIconeSetDropdown.onValueChanged.RemoveAllListeners();
        InputIconeSetDropdown.onValueChanged.AddListener(idx =>
        { 
            SaveSystem.CurrentSettings.IconeSet = InputIconeSetDropdown.options[idx].text;
            Dirty();
        });
        
        FindCurrentValue();

        base.Build();
    }

    void FindCurrentValue()
    {
        UIScaleSlider.SetValueWithoutNotify(SaveSystem.CurrentSettings.UIScale);
        UIScaleValueDisplay.text = SaveSystem.CurrentSettings.UIScale.ToString("n2");
        
        VisualManager.RescaleUIToValue(SaveSystem.CurrentSettings.UIScale);
        
        UIScaleApply.gameObject.SetActive(false);
        UIScaleApply.interactable = false;
        CleanScaleApplyNav();
        
        HighlightInteractiveObjectToggle.SetIsOnWithoutNotify(SaveSystem.CurrentSettings.HighlightInteractiveObject);
        UINarrationToggle.SetIsOnWithoutNotify(UAP_AccessibilityManager.IsEnabled());
        SwitchOpenDislexicToggle.SetIsOnWithoutNotify(SaveSystem.CurrentSettings.UseOpenDyslexicFont);
        UseDynamicCamera.SetIsOnWithoutNotify(SaveSystem.CurrentSettings.UseDynamicCameras);
        
        AutoPauseDialogueToggle.SetIsOnWithoutNotify(SaveSystem.CurrentSettings.PauseDialogue);
        
        var allSets = ControlManager.ControlIconSelector.GetSetList();
        int currentSet = -1;
        for (int i = 0; i < allSets.Count; i++)
        {
            if (SaveSystem.CurrentSettings.IconeSet == allSets[i])
            {
                currentSet = i;
            }
        }
        InputIconeSetDropdown.SetValueWithoutNotify(currentSet);
    }

    protected override void InternalClose(Action closeAction)
    {
        base.InternalClose(closeAction);
        
        CleanScaleApplyNav();
    }

    public override void Display()
    {
        base.Display();
        
        FindCurrentValue();
    }

    protected override void SaveChange(Action onSave)
    {
        VisualManager.ToggleInteractiveHighlight();
        
        UIToggleSidebar.Instance?.UpdateState();
        
        base.SaveChange(onSave);
    }

    protected override void UndoChange()
    {
        base.UndoChange();
        
        VisualManager.RescaleUIToValue(SaveSystem.CurrentSettings.UIScale);
        SwitchAllFont();
        
        FindCurrentValue();
    }

    protected override void SetupKeyInput()
    {
        EventSystem.current.SetSelectedGameObject(UIScaleSlider.gameObject);
    }

    public override Selectable GetLastControl()
    {
        return InputIconeSetDropdown;
    }

    public override Selectable GetFirstControl()
    {
        return UIScaleSlider;
    }

    void SetupScaleApplyNavigation()
    {
        var nav = UIScaleApply.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnLeft = UIScaleSlider;
        nav.selectOnDown = UIScaleSlider.navigation.selectOnDown;
        UIScaleApply.navigation = nav;

        nav = UIScaleSlider.navigation;
        nav.selectOnRight = UIScaleApply;
        UIScaleSlider.navigation = nav;
    }

    void CleanScaleApplyNav()
    {
        var nav = UIScaleApply.navigation;
        nav.mode = Navigation.Mode.None;
        nav.selectOnLeft = null;
        nav.selectOnDown = null;
        UIScaleApply.navigation = nav;

        nav = UIScaleSlider.navigation;
        nav.selectOnRight = null;
        UIScaleSlider.navigation = nav;
    }

    public static void SwitchAllFont()
    {
        var allText = GameObject.FindObjectsOfType<TextMeshProUGUI>(true);

        SwitchFontFor(allText);
    }

    public static void SwitchFontFor(TextMeshProUGUI[] textMeshProUGUI)
    {
        var font = SaveSystem.CurrentSettings.UseOpenDyslexicFont ? DataReference.Instance.OpenDislexicFont : DataReference.Instance.RegularFont;

        foreach (var t in textMeshProUGUI)
        {
            t.font = font;
        }
    }
}
