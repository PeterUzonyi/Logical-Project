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
    void Start()
    {
        Selected = false;
        SquareOccupied = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(true);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(true);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        hoverImage.gameObject.SetActive(false);
    }
}
