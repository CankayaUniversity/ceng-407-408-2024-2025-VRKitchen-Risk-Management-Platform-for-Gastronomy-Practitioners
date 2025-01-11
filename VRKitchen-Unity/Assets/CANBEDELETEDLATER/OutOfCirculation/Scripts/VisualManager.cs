using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class VisualManager
{
    private static VisualManager s_Instance;

    private static float m_StartingReferenceWidth;
    private static float m_StartingReferenceHeight;

    static VisualManager()
    {
        s_Instance = new VisualManager();
    }

    public static void Init()
    {
        s_Instance.InternalInit();
    }

    void InternalInit()
    {
        ToggleInteractiveHighlight();

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            ToggleInteractiveHighlight();
        };
        
        //set the saved resolution
        Screen.SetResolution(SaveSystem.CurrentSettings.ResolutionWidth, SaveSystem.CurrentSettings.ResolutionHeight, SaveSystem.CurrentSettings.FullScreenMode);
        
        QualitySettings.SetQualityLevel(SaveSystem.CurrentSettings.QualityLevel);
    }

    public static void InitUIData()
    {
        m_StartingReferenceWidth = UIRoot.Instance.Scaler.referenceResolution.x;
        m_StartingReferenceHeight = UIRoot.Instance.Scaler.referenceResolution.y;
    }

    public static void ToggleInteractiveHighlight()
    {
        //TODO : do better than grabbing the camera with main?
        
        var cam = Camera.main;

        if(cam == null)
            return;
        
        var addData = cam.GetUniversalAdditionalCameraData();
        
        if(addData == null)
            return;
        
        addData.SetRenderer(SaveSystem.CurrentSettings.HighlightInteractiveObject ? 1 : 0);
    }
    
    public static void RescaleUIToValue(float scaleValue)
    {
        var refRes = UIRoot.Instance.Scaler.referenceResolution;
        refRes.x = m_StartingReferenceWidth / scaleValue;
        refRes.y = m_StartingReferenceHeight / scaleValue;
        
        UIRoot.Instance.Scaler.referenceResolution = refRes;
    }
}
