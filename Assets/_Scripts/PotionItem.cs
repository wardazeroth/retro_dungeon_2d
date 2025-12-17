using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionItem : MonoBehaviour
{
    [Header("Curación")]
    [Range(0.01f, 1f)]
    [SerializeField]
    private float healingPercentage = 0.25f;

    [Header("Efectos (Opcional")]
    [SerializeField] private GameObject collectEffect;

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

            Debug.Log("La poción fue tocada por: " + collision.gameObject.name + " con Tag: " + collision.tag);
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            if (playerHealth != null) 
                {
                isCollected = true;

                playerHealth.HealByPercentage(healingPercentage);

                //Feedback visual
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, Quaternion.identity);
                }

                    Destroy(gameObject);
                }
        }
    }
}
