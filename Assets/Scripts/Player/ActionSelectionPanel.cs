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


        //Adott esetben nem lehet bizonyos akciókat választani
        if (selected == ActionType.MesterAction && TurnManager.Instance.currentPlayer.masterActionUsed)
        {
            ShowErrorMessage("Ebben a körben már elhasználtad a mester akciódat!");
            return;
        }
        if ((selected == ActionType.PlaceElement || selected == ActionType.MesterAction) && TurnManager.Instance.currentPlayer.IsCardSlotsEmpty())
        {
            ShowErrorMessage("Nem tudod ezt az akciót választani, mert nincsen elõtted feladvány kártya!");
            return;
        }
        if (selected == ActionType.TakePuzzle && TurnManager.Instance.currentPlayer.IsCardSlotsFull())
        {
            ShowErrorMessage("Nem tudod ezt az akciót választani, mert nincs elõtted hely egy új feladvány kártyának!");
            return;
        }
        if (selected == ActionType.TakeElement && TurnManager.Instance.currentPlayer.IsCardSlotsFull())
        {
            ShowErrorMessage("A CommonReserve-ben nincsen több lvl1-es elem!");
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
