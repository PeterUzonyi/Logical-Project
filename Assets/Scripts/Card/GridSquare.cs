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
        Invoke("DelayedMethod", 0.1f);
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

        Grid grid = transform.parent.GetComponent<Grid>();
        if (grid != null )
        {
            grid.ElementIsPlacedOnCard(InventoryItem.SelectedInventoryItem.ID);
        }   
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

    private void DelayedMethod()
    {
        //Késleltetni kell, hogy a card beszínezze a gridsquare-ket
        if (this.GetComponent<Image>().color == Color.black)
        {
            SquareOccupied = true;
            activeImage.color = Color.black;
            activeImage.gameObject.SetActive(true);
        }
        else
        {
            SquareOccupied = false;
        }
    }
}
