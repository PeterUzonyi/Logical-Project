using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The necessary datas for the game (both online and local) and it's default values
/// </summary>
public static class GameConfig
{
    //Alapértelmezett
    public static int PlayerCount = 2;
    public static float ThinkingTime = 30f;
    public static string[] PlayerNames = { "Játékos 1", "Játékos 2", "Játékos 3", "Játékos 4" };
    public static Color[] PlayerColors = {
        new Color(0.8f, 0.2f, 0.2f), // piros
        new Color(0.2f, 0.4f, 0.8f), // kék
        new Color(0.2f, 0.7f, 0.3f), // zöld
        new Color(0.9f, 0.7f, 0.1f)  // sárga
    };
}
