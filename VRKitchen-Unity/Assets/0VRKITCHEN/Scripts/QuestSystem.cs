using System.Collections.Generic;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    [System.Serializable]
    public class QuestStep
    {
        public string itemTag;        
        public string action;         // "Take" or "Place"
    }

    [SerializeField] private List<QuestStep> questSteps;
    private int currentStepIndex = 0;
    private bool isQuestCompleted = false;
    


    public void OnItemTaken(string itemTag)
    {
        if (isQuestCompleted) return;
        
        if (currentStepIndex > 0 &&
            questSteps[currentStepIndex - 1].itemTag == itemTag &&
            questSteps[currentStepIndex - 1].action == "Place")
        {
            Debug.Log($"Wrong step:You picked up {itemTag} before completing the next step.");
            currentStepIndex--;  
            return;
        }

        if (currentStepIndex < questSteps.Count &&
            questSteps[currentStepIndex].itemTag == itemTag &&
            questSteps[currentStepIndex].action == "Take")
        {
            Debug.Log($"Step completed: Take {itemTag}");
            currentStepIndex++;
            CheckQuestCompletion();
        }
        else
        {
           //direkt startta bastýrýyor çöz
                Debug.Log($"Wrong step! You need to {questSteps[currentStepIndex].action} {questSteps[currentStepIndex].itemTag}.");
            
        }
    }

    public void OnItemPlaced(string itemTag)
    {
        if (isQuestCompleted) return;

       
        if (currentStepIndex < questSteps.Count &&
            questSteps[currentStepIndex].itemTag == itemTag &&
            questSteps[currentStepIndex].action == "Place")
        {
            Debug.Log($"Step completed: Place {itemTag}");
            currentStepIndex++;
            CheckQuestCompletion();
            
        }
        else
        {
            
                Debug.Log($"Wrong step! You need to {questSteps[currentStepIndex].action} {questSteps[currentStepIndex].itemTag}.");
            
        }
    }
    public void OnItemReleased(string itemTag)
    {
        if (isQuestCompleted) return;  

        
        if (currentStepIndex > 0 && questSteps[currentStepIndex - 1].itemTag == itemTag && questSteps[currentStepIndex - 1].action == "Take" && questSteps[currentStepIndex ].action != "Place")
        {
            Debug.Log($"Wrong step:You released {itemTag} before completing the next step.");
            currentStepIndex--;  
        }
    }
    private void CheckQuestCompletion()
    {
        if (currentStepIndex >= questSteps.Count)
        {
            Debug.Log("Quest Completed!");
            isQuestCompleted = true;  
        }
        else
        {
           
            QuestStep nextStep = questSteps[currentStepIndex];
            Debug.Log($"Next step: {nextStep.action} {nextStep.itemTag}");
        }
    }
}
