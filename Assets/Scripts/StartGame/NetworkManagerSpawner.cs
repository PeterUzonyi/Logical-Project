using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns the network manager, so returning to the main manu scene is easier
/// </summary>
public class NetworkManagerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject networkManagerPrefab;

    //Called when the script is loaded
    void Awake()
    {
        if (NetworkManager.Instance == null)
        {
            Instantiate(networkManagerPrefab);
        }
    }
}
