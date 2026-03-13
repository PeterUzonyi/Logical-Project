using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MyGrid : MonoBehaviour
{
    public int columns;
    public int rows;
    public float squareGap;
    public GameObject gridSquare;
    public Vector2 startPosition;
    public float squareScale;
    public float everySquareOffSet;

    public bool isInitialized = false;

    private Vector2 offSet = new Vector2(0, 0);
    private List<GameObject> gridSquares = new List<GameObject>();

    [HideInInspector]
    public int[] ElementsOnCard = new int[9];
    public int count = 0;
    public int rewardElement;
    public int scoreNumber;

    public CardLoader OwnerCardLoader;

    void Start()
    {
        SpawnGridSquares();
        SetGridSquaresPositions();
        isInitialized = true;
    }

    private void OnEnable()
    {
        GameEvents.CheckIfElementCanBePlaced += CheckIfElementCanBePlaced;
    }

    private void OnDisable()
    {
        GameEvents.CheckIfElementCanBePlaced -= CheckIfElementCanBePlaced;
    }
    private void SpawnGridSquares()
    {
        int squareIndex = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                gridSquares.Add(Instantiate(gridSquare) as GameObject);

                gridSquares[gridSquares.Count - 1].GetComponent<GridSquare>().SquareIndex = squareIndex;
                gridSquares[gridSquares.Count - 1].transform.SetParent(this.transform);
                gridSquares[gridSquares.Count - 1].transform.localScale = new Vector3(squareScale, squareScale, squareScale);
                squareIndex++;
            }
        }
    }
    private void SetGridSquaresPositions()
    {
        int columnNumber = 0;
        int rowNumber = 0;
        Vector2 squareGapNumber = new Vector2(0, 0);
        bool rowMoved = false;

        var squareRect = gridSquares[0].GetComponent<RectTransform>();

        offSet.x = squareRect.rect.width * squareRect.transform.localScale.x + everySquareOffSet;
        offSet.y = squareRect.rect.height * squareRect.transform.localScale.y + everySquareOffSet;

        foreach (GameObject square in gridSquares)
        {
            if (columnNumber + 1 > columns)
            {
                squareGapNumber.x = 0;

                //go to the next column
                columnNumber = 0;
                rowNumber++;
                rowMoved = true;
            }

            var posXOffSet = offSet.x * columnNumber + (squareGapNumber.x * squareGap);
            var posYOffSet = offSet.y * rowNumber + (squareGapNumber.y * squareGap);

            if (columnNumber > 0 && columnNumber % 3 == 0)
            {
                squareGapNumber.x++;
                posXOffSet += squareGap;
            }
            if (rowNumber > 0 && rowNumber % 3 == 0 && rowMoved == false)
            {
                rowMoved = true;
                squareGapNumber.y++;
                posYOffSet += squareGap;
            }

            square.GetComponent<RectTransform>().anchoredPosition = new Vector2(startPosition.x + posXOffSet, startPosition.y - posYOffSet);
            square.GetComponent<RectTransform>().localPosition = new Vector3(startPosition.x + posXOffSet, startPosition.y - posYOffSet, 0);

            columnNumber++;
        }
    }

    private void CheckIfElementCanBePlaced()
    {
        if (TurnManager.Instance.currentPlayer.selectedAction == ActionType.MesterAction && TurnManager.Instance.currentPlayer.gridsUsedInMasterAction.Contains(this))
        {
            return;
        }
        // Ha egyetlen Selected square sincs ebben a Grid-ben, ne csináljon semmit
        bool anySelected = false;
        foreach (var square in gridSquares)
        {
            if (square.GetComponent<GridSquare>().Selected)
            {
                anySelected = true;
                break;
            }
        }
        if (!anySelected) return; // ez kiszûri az összes "idegen" Grid-et

        var squareIndexes = new List<int>();

        foreach (var square in gridSquares)
        {
            var gridSquare = square.GetComponent<GridSquare>();

            if (gridSquare.Selected && gridSquare.SquareOccupied == false)
            {
                squareIndexes.Add(gridSquare.SquareIndex);
                gridSquare.Selected = false;
            }
        }

        var currentSelectedShape = InventoryItem.SelectedInventoryItem;
        if (currentSelectedShape == null) //Nincsen egyik elem se kiválasztva
        {
            return;
        }

        if (currentSelectedShape.TotalSquareNumber == squareIndexes.Count)
        {
            foreach (var squareIndex in squareIndexes)
            {
                gridSquares[squareIndex].GetComponent<GridSquare>().PlaceElementOnBoard();
            }

            //Elem számát egyel csökkentjük
            currentSelectedShape.quantity--;


            //Ki van töltve a kártya elemekkel
            if(IsTheCardFull())
            {
                Player ownerPlayer = TurnManager.Instance.currentPlayer;
                InventoryManager ownerInventory = ownerPlayer.inventoryManager;

                InventoryItem item;
                ElementsOnCard[rewardElement]++; //A teljesítésért járó elem

                for (int i = 0; i < ElementsOnCard.Count(); i++)
                {
                    item = ownerInventory.GetItemById(i);
                    item.quantity += ElementsOnCard[i];//Visszakap minden kártyára rakott és jutalom elemet
                    ElementsOnCard[i] = 0;
                }

                ownerPlayer.RefreshScore(scoreNumber);

                ownerPlayer.RemoveCard(OwnerCardLoader.SlotIndex);
            }

            currentSelectedShape = null;
        }

        if (TurnManager.Instance.currentPlayer.selectedAction == ActionType.MesterAction)
        {//MasterAction
            TurnManager.Instance.currentPlayer.gridsUsedInMasterAction.Add(this);

            int used = TurnManager.Instance.currentPlayer.gridsUsedInMasterAction.Count;
            int total = TurnManager.Instance.currentPlayer.masterActionCardCount;

            Debug.Log($"MasterAction: {used}/{total} elem lerakva");

            if (used == total)
            {//MasterAction has Ended
                TurnManager.Instance.currentPlayer.masterActionUsed = true;
                Debug.Log("MasterAction has Ended");
                TurnManager.Instance.currentPlayer.ActionHasEnded();
            }

            return;
        }
        else
        {//PlaceElement Action
            //PlaceElement has Ended
            TurnManager.Instance.currentPlayer.ActionHasEnded();
            Debug.Log("PlaceElement has Ended");
        }  
    }

    public void ElementIsPlacedOnCard(int id)
    {
        //Csak akkor hívódik meg, ha az egész elemet leraktuk
        count++;
        if (count == InventoryItem.SelectedInventoryItem.TotalSquareNumber)
        {
            ElementsOnCard[id]++;
            count = 0;
        }
    }

    public bool IsTheCardFull()
    {
        bool full = true;
        foreach (GameObject square in gridSquares)
        {
            if (square.GetComponent<GridSquare>().SquareOccupied == false)
            {
                full = false;
            }
        }
        return full;
    }
}
