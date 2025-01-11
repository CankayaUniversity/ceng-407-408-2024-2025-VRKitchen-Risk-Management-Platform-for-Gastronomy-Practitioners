using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIVideoOptionScreen : UIScreen
{
    public TMP_Dropdown DisplayOption;
    public TMP_Dropdown ResolutionOption;
    public TMP_Dropdown QualityLevelDropdown;

    List<Resolution> m_AvailableResolution = new List<Resolution>();

    readonly string[] m_ScreenModeName = {"Fullscreen", "Window", "Borderless Window"};
    readonly FullScreenMode[] m_ScreenMode = { FullScreenMode.ExclusiveFullScreen, FullScreenMode.Windowed, FullScreenMode.FullScreenWindow };

    public override void Build()
    {
        DisplayOption.ClearOptions();
        DisplayOption.AddOptions(new List<string>(m_ScreenModeName));

        DisplayOption.onValueChanged.RemoveAllListeners();
        DisplayOption.onValueChanged.AddListener(idx =>
        {
            Dirty();
        });

        List<TMP_Dropdown.OptionData> resolutionDropdownOptions = new List<TMP_Dropdown.OptionData>();
        m_AvailableResolution.Clear();

        
        foreach (var resolution in Screen.resolutions)
        {
            //to simplify the number of offered resolution, we just only add one per w/h ignore refresh rate.
            if(m_AvailableResolution.FindIndex(existingRes => existingRes.width == resolution.width
                                                            && existingRes.height == resolution.height) != -1)
                continue;

            resolutionDropdownOptions.Add(new TMP_Dropdown.OptionData($"{resolution.width}x{resolution.height}"));
            m_AvailableResolution.Add(resolution);
        }
        ResolutionOption.options = resolutionDropdownOptions;

        ResolutionOption.onValueChanged.RemoveAllListeners();
        ResolutionOption.onValueChanged.AddListener(idx =>
        {
            Dirty();
        });
        
        List<TMP_Dropdown.OptionData> qualityOptions = new List<TMP_Dropdown.OptionData>();
        foreach (var name in QualitySettings.names)
        {
            qualityOptions.Add(new TMP_Dropdown.OptionData(name));
        }
        QualityLevelDropdown.options = qualityOptions;

        QualityLevelDropdown.onValueChanged.RemoveAllListeners();
        QualityLevelDropdown.onValueChanged.AddListener(idx =>
        {
            Dirty();
        });
    }

    protected override void SetupKeyInput()
    {
        EventSystem.current.SetSelectedGameObject(DisplayOption.gameObject);
    }

    public override void Display()
    {
        base.Display();
        FindCurrentValues();
    }

    void FindCurrentValues()
    {
        DisplayOption.SetValueWithoutNotify(ScreenModeToIndex(Screen.fullScreenMode));
        
        for (int i = 0; i < m_AvailableResolution.Count; ++i)
        {
            var resolution = m_AvailableResolution[i];
            if (Screen.currentResolution.width == resolution.width &&
                Screen.currentResolution.height == resolution.height)
            {
                ResolutionOption.SetValueWithoutNotify(i);
                break;
            }
        }
        
        QualityLevelDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
    }

    FullScreenMode IndexToScreenMode(int index)
    {
        return m_ScreenMode[index];
    }

    int ScreenModeToIndex(FullScreenMode mode)
    {

        for (int i = 0; i < m_ScreenMode.Length; ++i)
        {
            if (mode == m_ScreenMode[i])
                return i;
        }

        return 1;
    }

    protected override void SaveChange(System.Action onSave)
    {
        //We do NOT call base.SaveChange. This will be called by ApplyChange only if change is confirmed
        //we only save to setting if the confirmation popup is validated!
        ApplyChange(onSave);
    }

    protected override void UndoChange()
    {
        base.UndoChange();
        
        FindCurrentValues();
    }

    void ApplyChange(Action onFinished)
    {
        var currentRes = Screen.currentResolution;
        var currentFullscreenMode = Screen.fullScreenMode;
        var currentQualitySettings = QualitySettings.GetQualityLevel();
        
        var res = m_AvailableResolution[ResolutionOption.value];
        Screen.SetResolution(res.width, res.height, IndexToScreenMode(DisplayOption.value));
        QualitySettings.SetQualityLevel(QualityLevelDropdown.value);
        
        UISettingMenu.Instance.ModalPopup.Show("Apply changes?",
            "Apply",
            "Undo",
            () => 
            {
                //save
                var setting = SaveSystem.CurrentSettings;
                setting.QualityLevel = QualityLevelDropdown.value;
                setting.ResolutionWidth = res.width;
                setting.ResolutionHeight = res.height;
                setting.FullScreenMode = IndexToScreenMode(DisplayOption.value);

                Save();
                
                onFinished.Invoke();
            },
            () =>
            {
                //revert
                Screen.SetResolution(currentRes.width, currentRes.height, currentFullscreenMode);
                QualitySettings.SetQualityLevel(currentQualitySettings);
                FindCurrentValues();
                
                onFinished.Invoke();
            });

        StartCoroutine(WaitForConfirmation());
    }

    IEnumerator WaitForConfirmation()
    {
        float WaitTime = 9.99f;
        var popup = UISettingMenu.Instance.ModalPopup;

        while (popup.gameObject.activeSelf && WaitTime > 0.0f)
        {
            popup.ChangeMessage($"Confirm the display change?\nReverting in {Mathf.CeilToInt(WaitTime)} second");
            yield return 0;
            WaitTime -= Time.deltaTime;
        }
    }

    public override Selectable GetLastControl()
    {
        return QualityLevelDropdown;
    }

    public override Selectable GetFirstControl()
    {
        return DisplayOption;
    }
}
