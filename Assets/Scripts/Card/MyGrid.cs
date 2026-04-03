using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

/// <summary>
/// The grid part of the puzzle card
/// </summary>
public class MyGrid : MonoBehaviourPun
{
    /// <summary>
    /// Number of columns
    /// </summary>
    public int columns;

    /// <summary>
    /// Number of rows
    /// </summary>
    public int rows;

    /// <summary>
    /// The gap between the grid sqaures (needs for visualization)
    /// </summary>
    public float squareGap;

    /// <summary>
    /// The child prefabs for every square (grid squares)
    /// </summary>
    public GameObject gridSquare;

    /// <summary>
    /// Position of the grid squares
    /// </summary>
    public Vector2 startPosition;

    /// <summary>
    /// Size of the grid squares
    /// </summary>
    public float squareScale;

    /// <summary>
    /// Additional offset applied between every grid square
    /// </summary>
    public float everySquareOffSet;

    /// <summary>
    /// True, when the grid id initialized
    /// </summary>
    public bool isInitialized = false;

    /// <summary>
    /// Calculated offset between grid squares for positioning
    /// </summary>
    private Vector2 offSet = new Vector2(0, 0);

    /// <summary>
    /// Every grid square in a list (from 0 to 48)
    /// </summary>
    private List<GameObject> gridSquares = new List<GameObject>();

    /// <summary>
    /// Stores when az element is placed on a grid. After finishing the grid, we gave back evey element 
    /// (every shape index is in the corresponding index in this block)
    /// </summary>
    [HideInInspector]
    public int[] ElementsOnCard = new int[9];

    /// <summary>
    /// Counts the placed down grid squares
    /// The whole element is placed, whether the grid squares number is equal with the number of squares of the element
    /// </summary>
    public int count = 0;

    /// <summary>
    /// The corresponding index of the ElementOnCard block. Finishing the puzzle, this is the extra element
    /// </summary>
    public int rewardElement;

    /// <summary>
    /// The point that the player gets after finisheing this puzzle
    /// </summary>
    public int scoreNumber;

    /// <summary>
    /// The parent puzzle card of this grid
    /// </summary>
    public CardLoader OwnerCardLoader;

    //Start is called before the first frame update
    void Start()
    {
        SpawnGridSquares();
        SetGridSquaresPositions();
        isInitialized = true;
    }

    /// <summary>
    /// Subscribes to the CheckIfElementCanBePlaced game event
    /// </summary>
    private void OnEnable()
    {
        GameEvents.CheckIfElementCanBePlaced += CheckIfElementCanBePlaced;
    }

    /// <summary>
    /// Unsubscribes from the CheckIfElementCanBePlaced game event
    /// </summary>
    private void OnDisable()
    {
        GameEvents.CheckIfElementCanBePlaced -= CheckIfElementCanBePlaced;
    }

    /// <summary>
    /// Making the grid squares
    /// </summary>
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

    /// <summary>
    /// Visualizing hte grid squares
    /// </summary>
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

    /// <summary>
    /// Whether the element can be placed on this grid and whether placing down the element was successful
    /// </summary>
    private void CheckIfElementCanBePlaced()
    {
        if (TurnManager.Instance.currentPlayer.selectedAction == ActionType.MasterAction && TurnManager.Instance.currentPlayer.gridsUsedInMasterAction.Contains(this))
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
        if (!anySelected)
        {
            return; // ez kiszûri az összes "idegen" Grid-et
        }

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
            if (PhotonNetwork.IsConnected)
            {
                //Online mód
                int playerID = TurnManager.Instance.currentPlayer.PlayerID;
                int slotIndex = OwnerCardLoader.SlotIndex;
                int itemID = currentSelectedShape.ID;
                Color c = currentSelectedShape.ItemColor;

                int totalSquares = currentSelectedShape.TotalSquareNumber;

                GameNetworkHandler.Instance.photonView.RPC(
                    nameof(GameNetworkHandler.RPC_PlaceElementOnGrid),
                    RpcTarget.All,
                    playerID,
                    slotIndex,
                    squareIndexes.ToArray(),
                    itemID,
                    c.r, c.g, c.b,
                    totalSquares);
            }
            else
            {
                //Lokális mód
                foreach (var squareIndex in squareIndexes)
                {
                    gridSquares[squareIndex].GetComponent<GridSquare>().PlaceElementOnBoard();
                }

                //Elem számát egyel csökkentjük
                currentSelectedShape.quantity--;
            }

            Player ownerPlayer = TurnManager.Instance.currentPlayer;
            ownerPlayer.ElementPlacementSuccessfull = true;

            //Ki van töltve a kártya elemekkel
            if (IsTheCardFull())
            {
                //Player ownerPlayer = TurnManager.Instance.currentPlayer;
                ownerPlayer.CompletedPuzzles++;

                if (PhotonNetwork.IsConnected)
                {
                    //Online mód
                    int[] elementsSnapshot = (int[])ElementsOnCard.Clone();

                    GameNetworkHandler.Instance.photonView.RPC(
                        nameof(GameNetworkHandler.RPC_CardCompleted),
                        RpcTarget.All,
                        ownerPlayer.PlayerID,
                        OwnerCardLoader.SlotIndex,
                        elementsSnapshot,
                        scoreNumber,
                        rewardElement);
                }
                else
                {
                    //Lokális mód
                    InventoryManager ownerInventory = ownerPlayer.inventoryManager;

                    InventoryItem item;

                    //A teljesítésért járó elem
                    ElementsOnCard[rewardElement]++; 
                    CommonReserve.Instance.TakeFromInventory(rewardElement, 1);

                    for (int i = 0; i < ElementsOnCard.Count(); i++)
                    {
                        item = ownerInventory.GetItemById(i);
                        item.quantity += ElementsOnCard[i];//Visszakap minden kártyára rakott és jutalom elemet
                        ElementsOnCard[i] = 0;
                    }

                    ownerPlayer.RefreshScore(scoreNumber);

                    ownerPlayer.RemoveCard(OwnerCardLoader.SlotIndex);
                }
            }

            currentSelectedShape = null;
        }


        if (TurnManager.Instance.currentPlayer.selectedAction == ActionType.MasterAction)
        {//MasterAction
            if (TurnManager.Instance.currentPlayer.ElementPlacementSuccessfull)
            {
                TurnManager.Instance.currentPlayer.ElementPlacementSuccessfull = false;
                TurnManager.Instance.currentPlayer.gridsUsedInMasterAction.Add(this);

                int used = TurnManager.Instance.currentPlayer.gridsUsedInMasterAction.Count;
                int total = TurnManager.Instance.currentPlayer.masterActionCardCount;

                Debug.Log($"MasterAction: {used}/{total} elem lerakva");

                if (used == total)
                {//MasterAction has Ended
                    TurnManager.Instance.currentPlayer.masterActionUsed = true;
                    TurnManager.Instance.currentPlayer.endMasterActionBtn.SetActive(false);
                    Debug.Log("MasterAction has Ended");
                    TurnManager.Instance.currentPlayer.ActionHasEnded();
                }
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

    /// <summary>
    /// Getting the correct grid square based on the index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GridSquare GetGridSquare(int index)
    {
        if (index < 0 || index >= gridSquares.Count)
        {
            return null;
        }

        return gridSquares[index].GetComponent<GridSquare>();
    }


    /// <summary>
    /// Online mode, when the puzzle card is completed
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="slotIndex"></param>
    /// <param name="elements"></param>
    /// <param name="score"></param>
    /// <param name="rewardElement"></param>
    [PunRPC]
    private void RPC_CardCompleted(int playerID, int slotIndex, int[] elements, int score, int rewardElement)
    {
        // Megkeressük a játékost PlayerID alapján
        Player ownerPlayer = TurnManager.Instance.players.FirstOrDefault(p => p.PlayerID == playerID);

        if (ownerPlayer == null)
        {
            Debug.LogWarning($"RPC_CardCompleted: nem található játékos ID={playerID}");
            return;
        }

        InventoryManager ownerInventory = ownerPlayer.inventoryManager;

        // Jutalom elem hozzáadása
        elements[rewardElement]++;

        // Elemek visszaadása az inventoryba
        for (int i = 0; i < elements.Length; i++)
        {
            InventoryItem item = ownerInventory.GetItemById(i);
            if (item != null)
                item.quantity += elements[i];
        }

        // Pontszám frissítése
        ownerPlayer.RefreshScore(score);

        // Kártya eltávolítása
        ownerPlayer.RemoveCard(slotIndex);

        // ElementsOnCard nullázása lokálisan
        for (int i = 0; i < ElementsOnCard.Length; i++)
        {
            ElementsOnCard[i] = 0;
        }
    }

    /// <summary>
    /// Called when the whole element is successfully placed on this grid
    /// </summary>
    /// <param name="id"></param>
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

    /// <summary>
    /// Online mode, called when the whole element is successfully placed on this grid
    /// </summary>
    /// <param name="id"></param>
    /// <param name="totalSquares"></param>
    public void ElementIsPlacedOnCard(int id, int totalSquares)
    {
        count++;
        if (count == totalSquares)
        {
            ElementsOnCard[id]++;
            count = 0;
        }
    }

    /// <summary>
    /// True, whether the puzzle is completed
    /// </summary>
    /// <returns></returns>
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
