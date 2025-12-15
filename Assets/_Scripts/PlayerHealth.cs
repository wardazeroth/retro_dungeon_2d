using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealthValue => currentHealth;

    public UnityEvent<float, float> OnHealthChanged;

    public UnityEvent OnPlayerDied;

    void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
        {
            currentHealth= 0;
            OnPlayerDied?.Invoke();
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

    void Update()
    {
        // PRUEBA DE DAÑO TEMPORAL
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10f); // Verifica que la barra baja
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            HealByPercentage(0.15f); // Verifica que la barra sube
        }
    }
}
