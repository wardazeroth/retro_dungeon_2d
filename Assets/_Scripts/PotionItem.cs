using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionItem : MonoBehaviour
{
    [Header("Curación")]
    [Range(0.01f, 1f)]
    [SerializeField]
    private float healingPercentage = 0.3f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            if (playerHealth != null) 
                {

                playerHealth.HealByPercentage(healingPercentage);

                    Destroy(gameObject);
                }
        }
    }
}
