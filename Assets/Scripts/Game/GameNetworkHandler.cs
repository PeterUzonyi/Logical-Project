using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(PhotonView))]
public class GameNetworkHandler : MonoBehaviourPun
{
    public static GameNetworkHandler Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }


    [PunRPC]
    public void RPC_PlaceElementOnGrid(int playerID, int slotIndex, int[] squareIndexes, int itemID, float r, float g, float b, int totalSquares)
    {
        Player player = TurnManager.Instance.players
            .FirstOrDefault(p => p.PlayerID == playerID);
        if (player == null) return;

        CardLoader cardLoader = player.GetCardLoaderBySlot(slotIndex);
        if (cardLoader == null) return;

        MyGrid grid = cardLoader.gridScript;
        if (grid == null) return;

        Color elementColor = new Color(r, g, b);

        foreach (int idx in squareIndexes)
        {
            GridSquare sq = grid.GetGridSquare(idx);
            if (sq != null)
                sq.ActivateSquareSync(elementColor, itemID, totalSquares);
        }

        InventoryItem item = player.inventoryManager.GetItemById(itemID);
        if (item != null)
        {
            item.quantity--;
            item.RefreshCount();
        }
    }

    [PunRPC]
    public void RPC_CardCompleted(int playerID, int slotIndex, int[] elements, int score, int rewardElement)
    {
        Player ownerPlayer = TurnManager.Instance.players
            .FirstOrDefault(p => p.PlayerID == playerID);
        if (ownerPlayer == null) return;

        InventoryManager ownerInventory = ownerPlayer.inventoryManager;

        elements[rewardElement]++;
        CommonReserve.Instance.TakeFromInventory(rewardElement, 1);

        for (int i = 0; i < elements.Length; i++)
        {
            InventoryItem item = ownerInventory.GetItemById(i);
            if (item != null)
            {
                item.quantity += elements[i];
                item.RefreshCount();
            }
        }

        ownerPlayer.RefreshScore(score);
        ownerPlayer.RemoveCard(slotIndex);
    }
}
