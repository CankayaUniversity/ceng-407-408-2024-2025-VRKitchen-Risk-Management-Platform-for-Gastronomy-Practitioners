using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIItemInfoPopup : MonoBehaviour, UIRoot.IUIInitHandler
{
    public static UIItemInfoPopup Instance { get; private set; }
    
    public RawImage ItemIcon;
    public TextMeshProUGUI ItemTitle;
    public TextMeshProUGUI ItemDescriptionText;

    public Button CloseButton;
    public Button UseButton;
    
    protected GameObject m_PreviousSelection;
    protected event Action m_OnClose;
    protected Item m_DisplayedItem;

    protected Camera m_RenderingCamera;
    protected Light m_RenderingLight;
    protected RenderTexture m_RenderTexture;
    protected GameObject m_3DObject;

    public void Init()
    {
        Instance = this;
        gameObject.SetActive(false);

        m_RenderTexture = new RenderTexture(512, 512, 16);
        
        GameObject camObj = new GameObject("RenderingCamera");
        DontDestroyOnLoad(camObj);
        //camObj.hideFlags = HideFlags.HideInHierarchy;
        camObj.SetActive(false);

        //only render layer 20;
        camObj.layer = 20;
        
        m_RenderingCamera = camObj.AddComponent<Camera>();
        m_RenderingCamera.fieldOfView = 40;
        m_RenderingCamera.clearFlags = CameraClearFlags.SolidColor;
        m_RenderingCamera.backgroundColor = Color.black;
        m_RenderingCamera.cullingMask = 1 << 20;
        m_RenderingCamera.targetTexture = m_RenderTexture;
        
        //light
        GameObject renderingLight = new GameObject("RenderingLight");
        DontDestroyOnLoad(renderingLight);
        renderingLight.layer = 20;
        //renderingLight.hideFlags = HideFlags.HideInHierarchy;
        m_RenderingLight = renderingLight.AddComponent<Light>();
        m_RenderingLight.cullingMask = 1 << 20;
        m_RenderingLight.intensity = 0.5f;
        m_RenderingLight.type = LightType.Directional;

        ItemIcon.texture = m_RenderTexture;
    }
    
    public void Show(Item item, System.Action onClose)
    {
        gameObject.SetActive(true);

        m_DisplayedItem = item;
        
        ItemTitle.text = item.Name;
        ItemDescriptionText.text = item.Description;

        m_PreviousSelection = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(CloseButton.gameObject);

        m_OnClose = onClose;

        ControlManager.UICancelAction.performed += HandleClose;

        if (item.Usable)
        {
            UseButton.gameObject.SetActive(true);
            var nav = UseButton.navigation;
            nav.selectOnUp = CloseButton;
            UseButton.navigation = nav;

            nav = CloseButton.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnDown = UseButton;
            Debug.Log(nav);
            CloseButton.navigation = nav;
        }
        else
        {
            UseButton.gameObject.SetActive(false);
            var nav = CloseButton.navigation;
            nav.mode = Navigation.Mode.None;
            nav.selectOnDown = null;
            CloseButton.navigation = nav;
        }

        if (item.Prefab3D)
        {
            m_3DObject = Instantiate(item.Prefab3D);
            RecursiveLayerSet(m_3DObject.transform, 20);
            m_3DObject.transform.position = new Vector3(0, 0, 1);
            m_RenderingCamera.gameObject.SetActive(true);
        }

        if (item.SFX != null)
        {
            AudioManager.Instance.PlayPointSFX(item.SFX, transform.position, item.SFXLoop);
        }
        
        UISidebar.PushFocusLock();
    }

    void RecursiveLayerSet(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t)
        {
            RecursiveLayerSet(child, layer);
        }
    }

    private void Update()
    {
        if (m_3DObject != null)
        {
            m_3DObject.transform.Rotate(Vector3.up, 45.0f * Time.deltaTime);
        }
    }

    void HandleClose(InputAction.CallbackContext ctx)
    {
        Hide();
    }

    public void Hide()
    {
        EventSystem.current.SetSelectedGameObject(m_PreviousSelection);
        
        gameObject.SetActive(false);
        
        ControlManager.UICancelAction.performed -= HandleClose;

        if (m_3DObject)
        {
            Destroy(m_3DObject);
            m_3DObject = null;
            m_RenderingCamera.gameObject.SetActive(false);
        }
        
        AudioManager.Instance.PlayPointSFX(null, Camera.main.transform.position, false);
        
        m_OnClose?.Invoke();
        
        UISidebar.PopFocusLock();
    }

    public void UseItem()
    {
        ItemUsageHandler.Used(m_DisplayedItem, () =>
        {
            EventSystem.current.SetSelectedGameObject(UseButton.gameObject);
            AudioManager.Instance.PlayPointSFX(m_DisplayedItem.SFX, Camera.main.transform.position, m_DisplayedItem.SFXLoop);
        });
    }
}
