using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventoryEntry : Selectable, ISubmitHandler, IPointerClickHandler, UIComponentInfo.IComponentInfoDataProvider
{
    public Image Frame;
    public Image IconDisplay;

    public Color SelectedColor;

    private Item m_CurrentItem;

    private UIInventorySidebar m_Sidebar;
    private UIComponentInfo.ComponentInfoData m_Data = new UIComponentInfo.ComponentInfoData();
    
    public void DisplayItem(Item item)
    {
        m_CurrentItem = item;
        IconDisplay.gameObject.SetActive(true);
        IconDisplay.sprite = item.Icon;

        m_Data.Name = "Item: " + item.Name;
    }

    public void RemoveItem()
    {
        m_CurrentItem = null;
        IconDisplay.gameObject.SetActive(false);
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        OpenInfoPopup();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenInfoPopup();
    }

    public void SetupSidebar(UIInventorySidebar sidebar)
    {
        m_Sidebar = sidebar;
    }

    void OpenInfoPopup()
    {
        if(m_CurrentItem == null)
            return;

        m_Sidebar.EnableLeavingInput(false);
        UIItemInfoPopup.Instance.Show(m_CurrentItem, () =>
        {
            m_Sidebar.EnableLeavingInput(true);
        });
    }

    // public override void OnSelect(BaseEventData eventData)
    // {
    //     Frame.color = SelectedColor;
    // }
    //
    // public override void OnDeselect(BaseEventData eventData)
    // {
    //     Frame.color = Color.white;
    // }

    public UIComponentInfo.ComponentInfoData GetComponentInfo()
    {
        if (m_CurrentItem == null)
            return null;

        return m_Data;
    }
}
