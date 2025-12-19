using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.up * Mathf.Sin(Time.time * 5f) * 0.002f);
    }
    private void OnTriggerEnter2D(Collider2D other)

    {
        Update();

        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.CollectKey();
                Destroy(gameObject); // La llave desaparece al tocarla
            }
        }
    }
}

