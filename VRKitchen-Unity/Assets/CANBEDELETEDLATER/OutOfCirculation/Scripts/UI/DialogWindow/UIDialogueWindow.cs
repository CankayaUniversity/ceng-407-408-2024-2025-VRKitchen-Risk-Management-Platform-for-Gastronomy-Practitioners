using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIDialogueWindow : MonoBehaviour, UIRoot.IUIInitHandler, IPointerClickHandler
{
    public static UIDialogueWindow Instance { get; protected set; }

    public ScrollRect ScrollRect;
    public RectTransform ContentRoot;

    public UIDialogueEntry EntryPrefab;
    public UIChoiceEntry ChoicePrefab;

    public GameObject NextPrompt;

    public event Action WasClickedOn;
    
    //we used different pool instead of a base class list because we need to be able to find a specific type when we
    //add a new one based on what we display (dialogue line, choice etc.)
    private readonly Queue<UIDialogueEntry> m_UsedDialogueEntries = new Queue<UIDialogueEntry>();
    private readonly Queue<UIDialogueEntry> m_DialogueEntriesPool = new Queue<UIDialogueEntry>();

    private readonly List<UIChoiceEntry> m_UsedChoiceEntries = new List<UIChoiceEntry>();
    private readonly Queue<UIChoiceEntry> m_ChoiceEntriesPool = new Queue<UIChoiceEntry>();

    private DialogueHandler m_CurrentDialogueHandler;
    
    private readonly List<string> m_AlreadyAddedChoice = new List<string>();

    private bool m_WasKeyInput;
    private TextMeshProUGUI m_NextPromptText;
    
    public void Init()
    {
        Instance = this;
        Hide();
    }

    public void Show(DialogueHandler handler)
    {
        ControlManager.SwitchToUI();
        
        gameObject.SetActive(true);
        
        NextPrompt.SetActive(false);
        m_NextPromptText = NextPrompt.GetComponentInChildren<TextMeshProUGUI>();

        ScrollRect.verticalNormalizedPosition = 0.0f;
        m_CurrentDialogueHandler = handler;

        UISidebar.PushFocusLock();
        
        HandleControlTypeChange();
        ControlManager.OnControlTypeChanged += HandleControlTypeChange;
    }

    public void Hide()
    {
        ControlManager.SwitchToGameplay();
        
        gameObject.SetActive(false);
        
        UISidebar.PopFocusLock();

        //recyle all entry. We can't just disable them as they can be interleaved with other type of entry (e.g. choice)
        //and next dialogue will have a different orders of those.
        while (m_UsedDialogueEntries.Count > 0)
        {
            var entry = m_UsedDialogueEntries.Dequeue();
            entry.gameObject.SetActive(false);
            //entry.transform.SetParent(null);
            m_DialogueEntriesPool.Enqueue(entry);
        }

        ClearChoice();
        ControlManager.OnControlTypeChanged -= HandleControlTypeChange;
    }

    void HandleControlTypeChange()
    {
        if (ControlManager.CurrentControlType == ControlManager.ControlType.Mouse)
        {
            m_WasKeyInput = false;
            EventSystem.current.SetSelectedGameObject(null);

            UpdatePrompt();
        }
        else
        {
            if (!m_WasKeyInput)
            {
                m_WasKeyInput = true;

                //if we have choice entry we select the first one
                if (m_UsedChoiceEntries.Count > 0)
                {
                    EventSystem.current.SetSelectedGameObject(m_UsedChoiceEntries[0].gameObject);
                }
            }

            UpdatePrompt();
        }
    }

    void UpdatePrompt()
    {
        if(!m_WasKeyInput)
        {
            m_NextPromptText.text = "Select the window to " + (m_CurrentDialogueHandler.HaveNextEntry() ? "advance" : "close" + " the dialogue");
        }
        else
        {
            var bindingGroup = ControlManager.CurrentControlScheme.bindingGroup;
            var bindingName = ControlManager.UISubmitAction.GetBindingDisplayString(
                InputBinding.DisplayStringOptions.DontIncludeInteractions, bindingGroup);

            m_NextPromptText.text = $"Press {bindingName} to " + (m_CurrentDialogueHandler.HaveNextEntry() ? "advance" : "close");
        }
    }

    void ClearChoice()
    {
        while (m_UsedChoiceEntries.Count > 0)
        {
            var entry = m_UsedChoiceEntries[0];
            entry.gameObject.SetActive(false);
            //entry.transform.SetParent(null);
            m_ChoiceEntriesPool.Enqueue(entry);
            
            m_UsedChoiceEntries.RemoveAt(0);
        }
        
        m_AlreadyAddedChoice.Clear();
    }

    public void AddDialogueLine(Subtitle sub)
    {
        ClearChoice();

        UIDialogueEntry entry;
        if (m_DialogueEntriesPool.Count > 0)
        {
            entry = m_DialogueEntriesPool.Dequeue();
        }
        else
        {
            entry = Instantiate(EntryPrefab, ContentRoot, false);
            UIGameSettingScreen.SwitchFontFor(entry.GetComponentsInChildren<TextMeshProUGUI>());
            
        }

        m_UsedDialogueEntries.Enqueue(entry);

        entry.gameObject.SetActive(true);
        
        entry.Setup(sub);

        StartCoroutine(ScrollToBottom());

        UpdatePrompt();
    }

    public void AddChoice(string text, int number)
    {
        //Todo : clean that, this seems way too hackish to check if we already added the choice, we need to stop
        //it to be called every frame of the time
        if(m_AlreadyAddedChoice.Contains(text))
            return;
        
        m_AlreadyAddedChoice.Add(text);
        
        UIChoiceEntry entry;
        if (m_ChoiceEntriesPool.Count > 0)
        {
            entry = m_ChoiceEntriesPool.Dequeue();
        }
        else
        {
            entry = Instantiate(ChoicePrefab, ContentRoot);
            UIGameSettingScreen.SwitchFontFor(entry.GetComponentsInChildren<TextMeshProUGUI>());
        }

        entry.interactable = false;
        var nav = entry.navigation;
        
        //as entry are reused, we make sure to "reset" the navigation
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = null;
        nav.selectOnDown = null;

        if (m_UsedChoiceEntries.Count > 0)
        {
            var prev = m_UsedChoiceEntries[m_UsedChoiceEntries.Count - 1];
            nav.selectOnUp = prev;
            var upNav = prev.navigation;
            upNav.selectOnDown = entry;
            prev.navigation = upNav;
        }

        entry.navigation = nav;

        m_UsedChoiceEntries.Add(entry);

        entry.gameObject.SetActive(true);
        entry.transform.SetAsLastSibling();

        entry.SetChoice(text, () =>
        {
            m_CurrentDialogueHandler.ChooseNext(number);
            
            UILog.Instance.AddChoice(text);
            
            //remove those from here
            ClearChoice();
        });

        StartCoroutine(ScrollToBottom());
    }

    public void ActivateChoices()
    {
        for(int i = 0; i < m_UsedChoiceEntries.Count; ++i)
        {
            m_UsedChoiceEntries[i].interactable = true;
        }

        if (ControlManager.CurrentControlType != ControlManager.ControlType.Mouse)
        {
            EventSystem.current.SetSelectedGameObject(m_UsedChoiceEntries[0].gameObject);
        }
    }

    public void DisplayNextPrompt(bool displayed)
    {
        NextPrompt.SetActive(displayed);
    }

    //TODO : is there a way tofix that? Need to wait a frame for layouting to happen so we can scroll down...
    IEnumerator ScrollToBottom()
    {
        yield return 0;
        ScrollRect.verticalNormalizedPosition = 0.0f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        WasClickedOn?.Invoke();
    }
}
