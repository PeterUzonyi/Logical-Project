using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ActionSelectionPanel : MonoBehaviour
{
    public GameObject panel;

    public void ShowPanel()
    {
        panel.SetActive(true);
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }

    public void OnActionSelected(int actionTypeIndex)
    {
        ActionType selected = (ActionType)actionTypeIndex;
        HidePanel();
        TurnManager.Instance.currentPlayer.SetSelectedAction(selected);
        
    }
}
