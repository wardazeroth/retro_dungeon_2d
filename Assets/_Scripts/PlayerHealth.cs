using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    private GameManager gameManager;

    public float MaxHealth => maxHealth;
    public float CurrentHealthValue => currentHealth;

    public UnityEvent<float, float> OnHealthChanged;

    //public UnityEvent OnPlayerDied;

    void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null)
        {
            Debug.LogError("FATAL: No se encontró el objeto _GameManager en la escena.");
        }
    }
    
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
        {
            currentHealth= 0;
            HandlePlayerDeath();
            //OnPlayerDied?.Invoke();
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    //Cura al jugador en base a un porcentaje de su vida máxima
    public void HealByPercentage(float percentage)
    {
        float healAmount = maxHealth * percentage;
        currentHealth += healAmount;

        //Asegurar que no exceda la máxima
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void HandlePlayerDeath()
    {
        if (gameManager != null)
        {
            gameManager.EndGameDefeat();

        }
        gameObject.SetActive(false);
    }
}
