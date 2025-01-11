using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveSystem
{
    private static SaveSystem s_Instance;
    
    public static SettingContainer CurrentSettings => s_Instance.m_SettingContainer;

    private SettingContainer m_SettingContainer;

    private const string InputSaveFile = "input.json";
    private const string BindingSaveFile = "input_bindings.json";
    private const string SettingSaveFile = "settings.json";

    private string m_InputSaveFilePath = "";
    private string m_BindingSaveFilePath = "";
    private string m_SettingSaveFilePath = "";

    [Serializable]
    private struct BindingSaveData
    {
        [Serializable]
        public struct BidingSaveDataEntry
        {
            public string ActionGuid;
            public string BindingGuid;
            public string Path;
            public string Interactions;
            public string Processor;
            public string Groups;
        }

        public List<BidingSaveDataEntry> SaveDatas;
    }

    static SaveSystem()
    {
        s_Instance = new SaveSystem();
    }

    void InternalInit()
    {
        m_InputSaveFilePath = Application.persistentDataPath + "/" + InputSaveFile;
        m_BindingSaveFilePath = Application.persistentDataPath + "/" + BindingSaveFile;

        if (File.Exists(m_InputSaveFilePath))
        {
            
            //if we have additional binding, we need first to recreate them. Binding override (what we use for remapping)
            //only work on existing binding. So for input that got a single binding (e.g q and e for moving between tab
            //in settings), we create an additional "secondary" binding that is the same as the original one when building
            //the remapping screen. But if we override those binding, as they don't exist in the InputActionMap Asset we
            //have, loading the JSON of the override we do just below would fail as it can't find the binding it override.
            //So instead we save all bindings that don't exist in the original asset into a custom json and recreate those
            //before loading override.
            if (File.Exists(m_BindingSaveFilePath))
            {
                var bindingSaveData = JsonUtility.FromJson<BindingSaveData>(File.ReadAllText(m_BindingSaveFilePath));

                foreach (var data in bindingSaveData.SaveDatas)
                {
                    InputAction action = ControlManager.CurrentInput.FindAction(data.ActionGuid);

                    InputBinding binding = new InputBinding()
                    {
                        path = data.Path,
                        interactions = data.Interactions,
                        processors = data.Processor,
                        groups = data.Groups,
                        id = new Guid(data.BindingGuid)
                    };
                    
                    action.AddBinding(binding);
                }
            }
            
            //if that file exist, we read the override we already saved
            ControlManager.CurrentInput.LoadBindingOverridesFromJson(File.ReadAllText(m_InputSaveFilePath));
        }

        m_SettingSaveFilePath = Application.persistentDataPath + "/" + SettingSaveFile;
        if (File.Exists(m_SettingSaveFilePath))
        {
            m_SettingContainer = JsonUtility.FromJson<SettingContainer>(File.ReadAllText(m_SettingSaveFilePath));
        }
        else
        {
            m_SettingContainer = new SettingContainer();
            SaveSetting();
        }
    }
    
    public static void Init()
    {
        s_Instance.InternalInit();
    }

    public static void SaveInput()
    {
        BindingSaveData saveData = new BindingSaveData();
        saveData.SaveDatas = new List<BindingSaveData.BidingSaveDataEntry>();
        
        //we're going through all our actions and look if there is any bindings for any of those actions that doesn't
        //exists in the original asset (DataReference.Instance.DefaultInputActionAsset). If we found any, that mean that
        //binding was generated at runtime (likely by the control remapping window to allow for a secondary input remapping)
        //so we need to write that binding to file so we can recreate it during the loading above.
        foreach (var action in ControlManager.CurrentInput)
        {
            var originalAction = DataReference.Instance.DefaultInputActionAsset.FindAction(action.id);

            foreach (var binding in action.bindings)
            {
                
                var idx = originalAction.bindings.IndexOf(originalBinding => originalBinding == binding);
                
                //This is a new binding added dynamically, we need to save it in a JSON File to retrieve it when loading
                if (idx == -1)
                {
                    Debug.Log($"Unknown binding {binding.path} for action { originalAction.name }");
                    saveData.SaveDatas.Add(new BindingSaveData.BidingSaveDataEntry()
                    {
                        ActionGuid = action.id.ToString(),
                        BindingGuid = binding.id.ToString(),
                        Path = binding.path,
                        Groups = binding.groups,
                        Interactions = binding.groups,
                        Processor = action.processors
                    });
                }
            }
        }
        
        File.WriteAllText(s_Instance.m_BindingSaveFilePath, JsonUtility.ToJson(saveData));
        File.WriteAllText(s_Instance.m_InputSaveFilePath, ControlManager.CurrentInput.SaveBindingOverridesAsJson());
    }

    public static void SaveSetting()
    {
        var json = JsonUtility.ToJson(s_Instance.m_SettingContainer);
        File.WriteAllText(s_Instance.m_SettingSaveFilePath, json);
    }
}
