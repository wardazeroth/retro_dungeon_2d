using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDBinder : MonoBehaviour
{

    public Slider playerSlider;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InitializeHUDAfterSpawn());
    }

    private IEnumerator InitializeHUDAfterSpawn()
    {
        PlayerHealth playerHealth = null;

        float searchTime = 0f;
        while (playerHealth == null && searchTime < 5f)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            yield return null;
            searchTime += Time.deltaTime;
        }

        if (playerHealth != null)
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged.AddListener(UpdateHUD);

                UpdateHUD(playerHealth.CurrentHealthValue, playerHealth.MaxHealth);
            }
            else
            {
                Debug.LogError("Error: PlayerHealth NO ENCONTRADO. La barra de vida no se inicializará.");
            }
        }
    }


    public void UpdateHUD(float currentHealth, float maxHealth)
    {
        if (playerSlider != null)
        {
            playerSlider.maxValue = maxHealth;
            playerSlider.value = currentHealth;
        }
    }
}
