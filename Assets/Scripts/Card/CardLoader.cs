using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/// <summary>
/// Visualize the puzzle cards
/// </summary>
public class CardLoader : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// Puzzle card background image
    /// </summary>
    public Image BgImage;

    /// <summary>
    /// Puzzle card reward score, that the player gets once the puzzle is completed
    /// </summary>
    public TextMeshProUGUI ScoreText;
    
    /// <summary>
    /// Possible reward element sprites (9 different shape)
    /// </summary>
    public Sprite[] RewardSprites;

    /// <summary>
    /// Puzzle card reward elemet, that the player gets once the puzzle is completed
    /// </summary>
    public Image RewardImage;

    /// <summary>
    /// The background image of the grid part
    /// </summary>
    public Image GridBgImage;

    /// <summary>
    /// The grid part of the puzzle card
    /// </summary>
    public GameObject Grid;

    /// <summary>
    /// The puzzle card's position index in CommonReserve from 0 to 7 (white cards: 0-3, black cards: 4-7)
    /// </summary>
    public int SlotIndex;

    /// <summary>
    /// puzzle card puzzle grid script needs for each GridSquare
    /// </summary>
    [HideInInspector]
    public MyGrid gridScript;

    /// <summary>
    /// The new puzzle card ready to visualize
    /// </summary>
    public CardType CurrentCard { get; set; }

    /// <summary>
    /// If the next card is too early, we store it
    /// </summary>
    private CardType pendingCard = null;


    //Called when the script is loaded
    void Awake()
    {
        gridScript = Grid.GetComponent<MyGrid>();
    }

    //Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForInitialization());
    }

    /// <summary>
    /// Wait for the gridsquares and the cardmanager to be initialized
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForInitialization()
    {
        while (!gridScript.isInitialized || !CardManager.Instance.IsReady)
        {
            //Wait, until the gridsquares are initialized and the CardManager is ready too
            yield return null;
        }

        // Ha közben érkezett kártya, most jelenítjük meg
        if (pendingCard != null)
        {
            Visualize(pendingCard);
            pendingCard = null;
        }
    }

    /// <summary>
    /// Visualize the give card in the parameter when everything is ready for it (accessable from other classes)
    /// </summary>
    /// <param name="card"></param>
    public void ShowCard(CardType card)
    {
        CurrentCard = card;
        if (gridScript != null && gridScript.isInitialized)
        {
            Visualize(card);
        }
        else
        {
            pendingCard = card; // majd a coroutine végén rajzoljuk ki
        }
    }

    /// <summary>
    /// Visualize the give card in the parameter
    /// </summary>
    /// <param name="card"></param>
    private void Visualize(CardType card)
    {
        //Background Color
        if (card.Color == "White")
        {
            BgImage.color = Color.white;
            ScoreText.color = Color.black;

            //Grid bg
            GridBgImage.color = Color.gray;
        }
        else if (card.Color == "Black")
        {
            BgImage.color = Color.black;
            ScoreText.color = Color.white;

            //Grid bg
            GridBgImage.color = Color.gray;
        }

        //Score
        ScoreText.text = card.Score.ToString();
        gridScript.scoreNumber = card.Score;

        //Reward Element
        RewardImage.sprite = RewardSprites[card.RewardElement - 1];
        gridScript.rewardElement = card.RewardElement - 1;

        //Grid squares
        for (int i = 0; i < Grid.transform.childCount; i++)
        {
            var square = Grid.transform.GetChild(i).gameObject;
            var img = square.GetComponent<Image>();

            var squareScript = square.GetComponent<GridSquare>();

            if (card.Matrix[i / 7, i % 7] == 10)
            {
                img.color = Color.black;

                squareScript.SquareOccupied = true;
                squareScript.activeImage.color = Color.black;
                squareScript.activeImage.gameObject.SetActive(true);
            }
            else
            {
                img.color = Color.white;
                squareScript.SquareOccupied = false;
                squareScript.activeImage.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// This triggers when a player clicks on a puzzle card in the common reserve
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        CommonReserve.Instance.OnSlotClicked(SlotIndex);
    }

    /// <summary>
    /// After a card is completed, then it disappears and reset its grid
    /// </summary>
    public void ResetGrid()
    {
        foreach (Transform child in Grid.transform)
        {
            var square = child.GetComponent<GridSquare>();
            if (square != null)
            {
                square.SquareOccupied = false;
                square.GetComponent<Image>().color = Color.white;
                square.activeImage.gameObject.SetActive(false);
            }
        }
    }
}
