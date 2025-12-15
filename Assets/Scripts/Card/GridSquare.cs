using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSquare : MonoBehaviour
{
    public Image hoverImage;
    public Image activeImage;

    public bool Selected { get; set; }
    public bool SquareOccupied { get; set; }
    public int SquareIndex { get; set; }

    void Start()
    {
        Selected = false;

        if (this.GetComponent<Image>().color == Color.black)
        {
            SquareOccupied = true;
        }
        else
        {
            SquareOccupied = false;
        }
    }

    public void PlaceElementOnBoard()
    {
        ActivateSquare();
    }
    public void ActivateSquare()
    {
        hoverImage.gameObject.SetActive(false);
        activeImage.color = InventoryItem.SelectedInventoryItem.GetComponentInChildren<Image>().color;
        activeImage.gameObject.SetActive(true);
        Selected = true;
        SquareOccupied = true;

        Grid.ElementIsPlacedOnCard(InventoryItem.SelectedInventoryItem.ID);       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(true);
        Selected = true;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(true);
        Selected = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(false);
        Selected = false;
    }
}
