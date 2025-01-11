using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


//Base class for a control screen. Contains function to list input and handle their remapping.
//Each control type will override this and can add their additional settings to it.
public class UIControlSettingBase : UIScreen
{
    private struct InputRemapSave
    {
        public int Index;
        public InputBinding Binding;
        
        public UIRemappingButton ButtonToReset;
        public TextMeshProUGUI PathText;
        public Image IconImage;
        public string BindingGroup;
    }

    //used to keep track of which entry don't have a second binding
    private struct EntryTempData
    {
        public InputAction Action;
        public InputBinding Binding;
    }

    struct InputIcon
    {
        public string Path;
        public Image Image;
    }


    [Header("Prefabs")]
    public UIInputRemapEntry EntryPrefab;
    public UIControlCategoryHeader HeaderPrefab;

    [Header("UI References")] 
    public ScrollRect ScrollRect;
    public RectTransform ListParent;
    public RectTransform InputWaitOverlay;

    public Button ResetToDefaultButton;

    private List<Transform> m_CreatedTransform = new List<Transform>();
    private List<UIInputRemapEntry> m_CreatedEntry = new List<UIInputRemapEntry>();

    private List<InputRemapSave> m_ModifiedBindings = new List<InputRemapSave>();
    
    private Selectable m_FirstInput = null;

    string m_UsedIconSet = "";
    List<InputIcon> m_InputIcons = new List<InputIcon>();

    public override void Display()
    {
        base.Display();
        
        if(m_UsedIconSet != SaveSystem.CurrentSettings.IconeSet)
            CheckIconeSet();
    }

    void CheckIconeSet()
    {
        m_UsedIconSet = SaveSystem.CurrentSettings.IconeSet;
        
        foreach (var icon in m_InputIcons)
        {
            var iconMapping =
                ControlManager.ControlIconSelector.GetMappingForSetAndPath(SaveSystem.CurrentSettings.IconeSet,
                    icon.Path);

            if (iconMapping != null)
            {
                icon.Image.sprite = iconMapping.Icone;
                icon.Image.color = iconMapping.Color;
            }
        }
    }

    public void CreateRemappingUI(InputControlScheme scheme)
    {
        //init all control
        var input = ControlManager.CurrentInput;

        m_UsedIconSet = SaveSystem.CurrentSettings.IconeSet;

        UIInputRemapEntry previousEntry = null;

        Dictionary<UIInputRemapEntry, EntryTempData> entryWithNoSecondBinding = new Dictionary<UIInputRemapEntry, EntryTempData>();

        foreach (var actionMap in input.actionMaps)
        {
            //can't remap those
            if(actionMap.name == "Common")
                continue;

            var categoryHeader = Instantiate(HeaderPrefab, ListParent);
            categoryHeader.Text.text = actionMap.name + " controls";
            
            m_CreatedTransform.Add(categoryHeader.transform);

            foreach (var action in actionMap.actions)
            {
                //find this actions in both scheme
                for(int i = 0; i < action.bindings.Count; ++i)
                {
                    var binding = action.bindings[i];
                    
                    //jump over composite, we want the thing INSIDE that composite that come after
                    if(binding.isComposite) continue;
                    
                    //skip binding not part of the requested control scheme
                    if(binding.groups != scheme.bindingGroup) continue;
                    
                    string fullName = action.name;

                    if (binding.isPartOfComposite) fullName += ": " + binding.name;

                    //check if we already have an entry with the same name, this is a secondary input
                    var entry = m_CreatedEntry.Find(remapEntry => remapEntry.InputName.text == fullName);
                    
                    UIRemappingButton remappingButton;
                    TextMeshProUGUI pathText;
                    Image image;
                    
                    if (entry != null)
                    {
                        entryWithNoSecondBinding.Remove(entry);
                        
                        pathText = entry.SecondaryInputPath;
                        image = entry.SecondaryInputIcon;
                        remappingButton = entry.SecondaryInputButton;
                    }
                    else
                    {
                        var newEntry = Instantiate(EntryPrefab, ListParent);
                        newEntry.name = fullName;
                        newEntry.InputName.text = fullName;
                        
                        newEntry.SecondaryInputIcon.gameObject.SetActive(false);

                        if (m_FirstInput == null) m_FirstInput = newEntry.PrimaryInputButton;
                        
                        remappingButton = newEntry.PrimaryInputButton;
                        image = newEntry.PrimaryInputIcon;
                        pathText = newEntry.PrimaryInputPath;

                        m_CreatedEntry.Add(newEntry);
                        m_CreatedTransform.Add(newEntry.transform);

                        //map primary input to sconadry input navigation
                        var nav = newEntry.PrimaryInputButton.navigation;
                        nav.mode = Navigation.Mode.Explicit;
                        nav.selectOnRight = newEntry.SecondaryInputButton;
                        if (previousEntry != null)
                        {
                            //if we have a previous entry, we map going up to that entry
                            nav.selectOnUp = previousEntry.PrimaryInputButton;
                            //and map going down from that previous entry to that new one
                            var prevNav = previousEntry.PrimaryInputButton.navigation;
                            prevNav.selectOnDown = newEntry.PrimaryInputButton;
                            previousEntry.PrimaryInputButton.navigation = prevNav;
                        }
                        newEntry.PrimaryInputButton.navigation = nav;
                        
                        //map secondary -> primary navigation
                        nav = newEntry.SecondaryInputButton.navigation;
                        nav.mode = Navigation.Mode.Explicit;
                        nav.selectOnLeft = newEntry.PrimaryInputButton;
                        if (previousEntry != null)
                        {
                            //if we have a previous entry, we map going up to that entry
                            nav.selectOnUp = previousEntry.SecondaryInputButton;
                            //and map going down from that previous entry to that new one
                            var prevNav = previousEntry.SecondaryInputButton.navigation;
                            prevNav.selectOnDown = newEntry.SecondaryInputButton;
                            previousEntry.SecondaryInputButton.navigation = prevNav;
                        }
                        newEntry.SecondaryInputButton.navigation = nav;

                        previousEntry = newEntry;
                        
                        entryWithNoSecondBinding.Add(newEntry, new EntryTempData()
                        {
                            Action = action,
                            Binding = binding
                        });
                    }
                    
                    CreateRemappingButton(action, i, pathText, image, remappingButton, binding, scheme.bindingGroup);
                }
            }
        }

        //we go over every secondary entry that have no binding (the input asset only define one binding for that action)
        //we will make clicking the button create a new binding instead of just overriding an existing one.
        foreach (var pair in entryWithNoSecondBinding)
        {
            var entryTempData = pair.Value;
            var action = entryTempData.Action;
            var entry = pair.Key;
            var existingBinding = entryTempData.Binding;

            var newBinding = action.AddBinding(existingBinding.path, existingBinding.interactions, existingBinding.processors,
                existingBinding.groups);

            var idx = action.GetBindingIndex(newBinding.binding);
            
            CreateRemappingButton(action, idx, entry.SecondaryInputPath, entry.SecondaryInputIcon, 
                entry.SecondaryInputButton, newBinding.binding, scheme.bindingGroup);
        }

        if (previousEntry != null)
        {
            var lastNav = previousEntry.PrimaryInputButton.navigation;
            lastNav.selectOnDown = ResetToDefaultButton;
            previousEntry.PrimaryInputButton.navigation = lastNav;

            lastNav = ResetToDefaultButton.navigation;
            lastNav.selectOnUp = previousEntry.PrimaryInputButton;
            ResetToDefaultButton.navigation = lastNav;
        }
    }

    void CreateRemappingButton(InputAction action, int index, TextMeshProUGUI pathText, Image image, 
        UIRemappingButton remappingButton, InputBinding binding, string bindingGroup)
    {
        var displayString = action.GetBindingDisplayString(index, out string device, out string controlPath);
        var iconMapping =
            ControlManager.ControlIconSelector.GetMappingForSetAndPath(SaveSystem.CurrentSettings.IconeSet,
                controlPath);

        if (iconMapping != null)
        {
            pathText.gameObject.SetActive(false);
            image.gameObject.SetActive(true);

            image.sprite = iconMapping.Icone; 
            image.color = iconMapping.Color;
            
            m_InputIcons.Add(new InputIcon()
            {
                Image = image,
                Path = controlPath
            });
        }
        else
        {
            pathText.gameObject.SetActive(true);
            image.gameObject.SetActive(false);

            pathText.text = displayString;
        }
        
        remappingButton.onClick.RemoveAllListeners();
        
        remappingButton.onClick.AddListener(() =>
        {
            if(action.enabled)
                action.Disable();
            
            InputWaitOverlay.gameObject.SetActive(true);

            var previousOverride = binding.overridePath;

            var rebindingOperation = action.PerformInteractiveRebinding(index)
                .WithBindingGroup(bindingGroup)
                .WithCancelingThrough("<Keyboard>/escape")
                .Start();

            rebindingOperation.OnComplete(operation =>
            {
                InputWaitOverlay.gameObject.SetActive(false);

                pathText.text = action.GetBindingDisplayString(index);

                //we save the previous binding, so we can recover the override if cancel the change
                //but only if we don't already have a binding save for the ID (meaning we are remapping something
                //we already remap once since last save)
                if(m_ModifiedBindings.FindIndex(inputBinding => inputBinding.Binding.id == binding.id) == -1)
                    m_ModifiedBindings.Add(new InputRemapSave()
                    {
                        Binding = binding,
                        Index = index,
                        
                        PathText = pathText,
                        IconImage = image,
                        BindingGroup = bindingGroup,
                        ButtonToReset = remappingButton
                    });

                //we recreate the button to update the graphics 
                CreateRemappingButton(action, index, pathText, image, remappingButton, binding, bindingGroup);
                
                operation.Dispose();
                
                action.Enable();
                
                Dirty();
            });

            rebindingOperation.OnCancel(operation =>
            {
                InputWaitOverlay.gameObject.SetActive(false);
                operation.Dispose();
                
                action.Enable();
            });
        });
    }

    protected override void SetupKeyInput()
    {
        EventSystem.current.SetSelectedGameObject(m_FirstInput.gameObject);
    }

    protected override void SaveChange(System.Action onSave)
    {
        m_ModifiedBindings.Clear();
        
        base.SaveChange(onSave);
        SaveSystem.SaveInput();
    }

    protected override void UndoChange()
    {
        base.UndoChange();
        
        var input = ControlManager.CurrentInput;
        foreach (var modifiedBinding in m_ModifiedBindings)
        {
            var action = input.FindAction(modifiedBinding.Binding.action);
            action.ApplyBindingOverride(modifiedBinding.Index, modifiedBinding.Binding);
            
            CreateRemappingButton(action, modifiedBinding.Index, modifiedBinding.PathText, modifiedBinding.IconImage,
                modifiedBinding.ButtonToReset, modifiedBinding.Binding, modifiedBinding.BindingGroup);
        }
        
        m_ModifiedBindings.Clear();
    }

    public void ResetToDefault()
    {
        UISettingMenu.Instance.ModalPopup.Show("Reset all inputs to default?", "Reset", "Cancel",
            () =>
            {
                var input = ControlManager.CurrentInput;
                input.RemoveAllBindingOverrides();
                SaveSystem.SaveInput();
            },
            () =>
            {
                
            });
    }

    public override Selectable GetLastControl()
    {
        return ResetToDefaultButton;
    }

    public override Selectable GetFirstControl()
    {
        return m_FirstInput;
    }
}
