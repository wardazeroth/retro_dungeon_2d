using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        
        if (inventory != null && inventory.HasKey)
        {
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                gm.EndGameVictory();
            }
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("La puesrta de salida está bloqueada. Necesitas una llave para escapar");
        }
        
    }
}
