using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ActionSelectionPanel : MonoBehaviour
{
    public GameObject panel;
    public GameObject CommonReserveBlockingPanel;

    public GameObject ErrorMessagePanel;

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

        //Buttonok megjelenítése
        if (actionTypeIndex == 4 && TurnManager.Instance.currentPlayer.masterActionUsed == false)
        {
            TurnManager.Instance.currentPlayer.endMasterActionBtn.SetActive(true);
        }
    }

    public void OnExitClicked()
    {
        HidePanel();
        TurnManager.Instance.currentPlayer.BlockingPanel.SetActive(true);
        CommonReserveBlockingPanel.SetActive(true);

        //Buttonok megjelenítése
        TurnManager.Instance.currentPlayer.actionBtn.SetActive(true);
    }

    public void OnActionClicked()
    {
        ShowPanel();
        TurnManager.Instance.currentPlayer.BlockingPanel.SetActive(false);
        CommonReserveBlockingPanel.SetActive(false);
    }

    /*
    public void ShowErrorMessage(string ErrorMessage)
    {
        if (ErrorMessage == "")
        {
            TMP_Text text = ErrorMessagePanel.GetComponent<TMP_Text>();
            text.text = "";
            ErrorMessagePanel.SetActive(false);
        }
        else
        {
            TMP_Text text = ErrorMessagePanel.GetComponent<TMP_Text>();
            text.text = ErrorMessage;
            ErrorMessagePanel.SetActive(true);
        }
        
    }
    */
}
