using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

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

        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        //Adott esetben nem lehet bizonyos akciókat választani
        if (selected == ActionType.MesterAction && TurnManager.Instance.currentPlayer.masterActionUsed)
        {
            if (code == "hu")
            {
                ShowErrorMessage("Ebben a körben már elhasználtad a mester akciódat!");
            }
            else
            {
                ShowErrorMessage("You already used your master action in this round!");
            }
            
            return;
        }
        if ((selected == ActionType.PlaceElement || selected == ActionType.MesterAction) && TurnManager.Instance.currentPlayer.IsCardSlotsEmpty())
        {
            if (code == "hu")
            {
                ShowErrorMessage("Nem tudod ezt az akciót választani, mert nincsen elõtted feladvány kártya!");
            }
            else
            {
                ShowErrorMessage("You cannot choose this action, because you don't have any puzzle!");
            }
            return;
        }
        if (selected == ActionType.TakePuzzle && TurnManager.Instance.currentPlayer.IsCardSlotsFull())
        {
            if (code == "hu")
            {
                ShowErrorMessage("Nem tudod ezt az akciót választani, mert nincs elõtted hely egy új feladvány kártyának!");
            }
            else
            {
                ShowErrorMessage("You cannot choose this action, because you don't have space for more puzzle!");
            }
            return;
        }
        if (selected == ActionType.TakeElement && (CommonReserve.Instance.inventoryManager.GetItemById(0) == null || CommonReserve.Instance.inventoryManager.GetItemById(0).quantity < 1))
        {
            if (code == "hu")
            {
                ShowErrorMessage("A CommonReserve-ben nincsen több lvl 1-es elem!");
            }
            else
            {
                ShowErrorMessage("There are no more lvl 1 item in the Common Reserve!");
            }
            return;
        }
        ShowErrorMessage("");

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

    
    public void ShowErrorMessage(string ErrorMessage)
    {
        if (ErrorMessage == "")
        {
            TMP_Text text = ErrorMessagePanel.GetComponentInChildren<TMP_Text>();
            text.text = "";
            ErrorMessagePanel.SetActive(false);
        }
        else
        {
            TMP_Text text = ErrorMessagePanel.GetComponentInChildren<TMP_Text>();
            text.text = ErrorMessage;
            ErrorMessagePanel.SetActive(true);
        }
        
    }
}
