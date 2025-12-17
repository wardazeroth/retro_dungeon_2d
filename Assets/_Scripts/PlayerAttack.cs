using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    private Transform attackPoint;

    void Start()
    {
        attackPoint = transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerHitBox()
    {
        //detectar enemigos dentro del radio de ataque
        //Esto crea un Hitbox circular al momento del golpe
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        //Iterar sobre todos los enemigos detectados
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Player")) continue;

            //Intentar dañar al enemigo
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
                Debug.Log($"Hit: {enemy.name} por {attackDamage} de daño.");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Dibuja un círculo rojo en el editor que representa el Hitbox de ataque
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }


}
