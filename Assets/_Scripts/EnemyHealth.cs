using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Stats")]
    [SerializeField] private float maxHealth = 30f;
    private float currentHealth;

    public UnityEvent<float, float> OnHealthChanged;

    public UnityEvent OnEnemyDied;

    public float MaxHealth => maxHealth;
    public float CurrentHealthValue => currentHealth;

    private Animator animator;

    void Awake()
    {
        currentHealth = maxHealth;
        //Inicializa la barra de vida
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        animator = GetComponent<Animator>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            animator?.SetTrigger("DeathTrigger");
            Die();
        }
        else
        {
            GetComponent<EnemyAI>()?.FaceTarget();
            animator?.SetTrigger("HurtTrigger");
        }
    }

    public void Die()
    {
        EnemyAI aiScript = GetComponent<EnemyAI>();
        if (aiScript != null) aiScript.enabled = false;

        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        //OnEnemyDied?.Invoke();
        //OnEnemyDied?.Invoke();
        //Debug.Log($"{gameObject.name} ha sido derrotado.");
        //// Opcional: Detener la IA, activar animaciones, soltar loot, y finalmente...
        //Destroy(gameObject, 0.1f); // Pequeño delay para que el evento se propague
    }

    public void DestroyOnAnimEnd()
    {
        Destroy(gameObject);
    }
}
