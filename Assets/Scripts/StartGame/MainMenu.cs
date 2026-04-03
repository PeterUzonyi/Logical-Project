using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Starting the first scene and leaving the game
/// </summary>
public class Start : MonoBehaviour
{
    /// <summary>
    /// Loads the first scene of the game
    /// </summary>
    /// <param name="scene"></param>
    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    /// <summary>
    /// Quitting from the game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
