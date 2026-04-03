using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggered when the player releases a dragged element and the game checks if it can be placed on a grid
/// </summary>
public class GameEvents : MonoBehaviour
{
    public static Action CheckIfElementCanBePlaced;
}
