using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject networkManagerPrefab;

    void Awake()
    {
        if (NetworkManager.Instance == null)
        {
            Instantiate(networkManagerPrefab);
        }
    }
}
