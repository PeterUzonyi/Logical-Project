using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// On every puzzle card, there is 1 grid which has 49 gridsquares
/// </summary>
public class GridSquare : MonoBehaviour
{
    /// <summary>
    /// The shadow image that activates when an element is hovered above a grid square and disapears after
    /// </summary>
    public Image hoverImage;

    /// <summary>
    /// The active image that activates when an element is placed on a grid square
    /// </summary>
    public Image activeImage;

    /// <summary>
    /// Needs for the hover image, whether the element is above the grid square
    /// </summary>
    public bool Selected { get; set; }

    /// <summary>
    /// True, when az element is placed on a grid square (only 1 element can be placed on a grid square)
    /// </summary>
    public bool SquareOccupied { get; set; }

    /// <summary>
    /// Index of a grid square
    /// </summary>
    public int SquareIndex { get; set; }

    //Start is called before the first frame update
    void Start()
    {
        Selected = false;
    }
    
    /// <summary>
    /// Triggered, when an element is placed on the grid
    /// </summary>
    public void PlaceElementOnBoard()
    {
        ActivateSquare();
    }

    /// <summary>
    /// The grid square, that the element is placed on, gets a new color, no more element can be placed on this
    /// </summary>
    private void ActivateSquare()
    {
        hoverImage.gameObject.SetActive(false);
        activeImage.color = InventoryItem.SelectedInventoryItem.GetComponentInChildren<Image>().color;
        activeImage.gameObject.SetActive(true);
        Selected = true;
        SquareOccupied = true;

        MyGrid grid = transform.parent.GetComponent<MyGrid>();
        if (grid != null )
        {
            grid.ElementIsPlacedOnCard(InventoryItem.SelectedInventoryItem.ID);
        }   
    }

    /// <summary>
    /// When a hovered element enters above a grid square
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(true);
        Selected = true;
    }

    /// <summary>
    /// When a hovered element stays above a grid square
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(true);
        Selected = true;
    }

    /// <summary>
    /// When a hovered element leaves above a grid square
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(false);
        Selected = false;
    }

    /// <summary>
    /// In online mode, when a grid square is activated, every other player must see it, too
    /// </summary>
    /// <param name="color"></param>
    /// <param name="itemID"></param>
    /// <param name="totalSquares"></param>
    public void ActivateSquareSync(Color color, int itemID, int totalSquares)
    {
        hoverImage.gameObject.SetActive(false);
        activeImage.color = color;
        activeImage.gameObject.SetActive(true);
        Selected = true;
        SquareOccupied = true;

        MyGrid grid = GetComponentInParent<MyGrid>();
        if (grid != null)
        {
            grid.ElementIsPlacedOnCard(itemID, totalSquares);
        }
    }
}
