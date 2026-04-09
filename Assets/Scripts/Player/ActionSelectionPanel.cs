using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

/// <summary>
/// This proved the panel, where the current player can choose from all of the possible actions
/// </summary>
public class ActionSelectionPanel : MonoBehaviour
{
    /// <summary>
    /// The panel itself
    /// </summary>
    public GameObject panel;

    /// <summary>
    /// This is a transparent panel, that block the cards and element from clocking and dragging
    /// </summary>
    public GameObject CommonReserveBlockingPanel;

    /// <summary>
    /// Displays any error during choosing the action
    /// </summary>
    public GameObject ErrorMessagePanel;

    /// <summary>
    /// Showing the action panel
    /// </summary>
    public void ShowPanel()
    {
        panel.SetActive(true);
    }

    /// <summary>
    /// Hiding the action panel
    /// </summary>
    public void HidePanel()
    {
        panel.SetActive(false);
    }

    /// <summary>
    /// Triggers, when an action (button) is selected and informs the player.cs, which action is choosen.
    /// </summary>
    /// <param name="actionTypeIndex"></param>
    public void OnActionSelected(int actionTypeIndex)
    {
        ActionType selected = (ActionType)actionTypeIndex;

        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        //Adott esetben nem lehet bizonyos akciókat választani
        if (selected == ActionType.MasterAction && TurnManager.Instance.currentPlayer.masterActionUsed)
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
        if ((selected == ActionType.PlaceElement || selected == ActionType.MasterAction) && (TurnManager.Instance.currentPlayer.IsCardSlotsEmpty() || TurnManager.Instance.currentPlayer.RemainingElements == 0))
        {
            if (TurnManager.Instance.currentPlayer.IsCardSlotsEmpty())
            {
                if (code == "hu")
                {
                    ShowErrorMessage("Nem tudod ezt az akciót választani, mert nincsen elõtted feladvány kártya!");
                }
                else
                {
                    ShowErrorMessage("You cannot choose this action, because you don't have any puzzle!");
                }
            }
            if (TurnManager.Instance.currentPlayer.RemainingElements == 0)
            {
                if (code == "hu")
                {
                    ShowErrorMessage("Nem tudod ezt az akciót választani, mert nincsen több az elemed!");
                }
                else
                {
                    ShowErrorMessage("You cannot choose this action, because you don't have any element in your inventory!");
                }
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

    /// <summary>
    /// If the exit button is clicked, then everything is blocked by the transparent panels 
    /// and a Choose action button appears. This way the current player has the possibility to look at 
    /// everybody's game stats and choose the next action according to the informations.
    /// </summary>
    public void OnExitClicked()
    {
        HidePanel();
        TurnManager.Instance.currentPlayer.BlockingPanel.SetActive(true);
        CommonReserveBlockingPanel.SetActive(true);

        //Buttonok megjelenítése
        TurnManager.Instance.currentPlayer.actionBtn.SetActive(true);
    }

    /// <summary>
    /// This opens up the action panel again via a button
    /// </summary>
    public void OnActionClicked()
    {
        ShowPanel();
        TurnManager.Instance.currentPlayer.BlockingPanel.SetActive(false);
        CommonReserveBlockingPanel.SetActive(false);
    }

    /// <summary>
    /// When an error accure, then it is displayed. For example the player wants to place down an element, 
    /// but there are no unsolved puzzles to put it down
    /// </summary>
    /// <param name="ErrorMessage"></param>
    private void ShowErrorMessage(string ErrorMessage)
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
