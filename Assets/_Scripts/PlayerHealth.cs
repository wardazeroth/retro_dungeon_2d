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
    private Animator animator;
    private bool isDying = false;

    public float MaxHealth => maxHealth;
    public float CurrentHealthValue => currentHealth;

    public UnityEvent<float, float> OnHealthChanged;

    //public UnityEvent OnPlayerDied;

    void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        animator = GetComponent<Animator>();

        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("FATAL: No se encontró el objeto _GameManager en la escena.");
        }
    }
    
    public void TakeDamage(float amount)
    {
        if (isDying) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
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
        if (isDying) return;
        isDying = true;

        Debug.Log("[MUERTE] Iniciando secuencia de muerte del jugador.");

        // 1. Apagar scripts de control
        if (GetComponent<PlayerMovement>() != null) GetComponent<PlayerMovement>().enabled = false;

        // 2. Frenar Rigidbody y hacerlo "fantasma"
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // Evita que los enemigos lo sigan empujando
        }

        // 3. Disparar Animación
        if (animator != null)
        {
            animator.SetTrigger("DeathTrigger");
        }
        else
        {
            // Si no hay animator, mostramos el UI de inmediato como fallback
            ShowGameOverUI();
        }
    }

    public void ShowGameOverUI()
    {
        Debug.Log("Evento de animación detectado: Llamando al Panel de Muerte");
        if (gameManager != null)
        {
            gameManager.EndGameDefeat();

        }
        gameObject.SetActive(false);
    }
}
