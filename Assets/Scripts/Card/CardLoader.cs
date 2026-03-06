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


public class CardLoader : MonoBehaviour, IPointerClickHandler
{
    public Image BgImage;
    public TextMeshProUGUI ScoreText;
    
    public Sprite[] RewardSprites;
    public Image RewardImage;

    public Image GridBgImage;
    public GameObject Grid;

    public int SlotIndex;

    [HideInInspector]
    public Grid gridScript;

    public CardType CurrentCard { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForInitialization());
    }

    IEnumerator WaitForInitialization()
    {
        gridScript = Grid.GetComponent<Grid>();

        while (!gridScript.isInitialized || !CardManager.Instance.IsReady)
        {
            //Wait, until the gridsquares are initialized and the CardManager is ready too
            yield return null;
        }
    }

    public void ShowCard(CardType card)
    {
        CurrentCard = card;
        Visualize(card); // a már meglévõ Visualize() metódus
    }
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
            else //if (card.Matrix[(i / 7), (i % 7)] == 0)
            {
                img.color = Color.white;
                squareScript.SquareOccupied = false;
                squareScript.activeImage.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CommonReserve.Instance.OnSlotClicked(SlotIndex);
    }

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
