using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDetector : Detector
{
    [SerializeField]
    private float targetDetectionRange = 8f;

    [SerializeField]
    private LayerMask obstaclesLayerMask;

    [SerializeField]
    private LayerMask playerLayerMask;

    [SerializeField]
    private bool showGizmos = true;

    private List<Transform> colliders;

    public override void Detect(AIData aiData)
    {
        //1. Detección´por proximidad
        //Busca cualquier Collider2D del jugador dentro del targetDetectionRange.
        Collider2D playerCollider =
            Physics2D.OverlapCircle(transform.position, targetDetectionRange, playerLayerMask);

        if (playerCollider != null)
        {
            //Hemos encontrado el jugador en el radio. Ahora verificamos la Línea de Visión (LoS).
            //2. Cálculo de la dirección
            Vector2 direction = (playerCollider.transform.position - transform.position).normalized;

            // Raycasting (línea de visión)
            // Lanza un rayo desde el enemigo hacia el jugador
            // SI el rayo choca con un obstáculo ANTES de llegar al jugador, la LoS está bloqueada
            RaycastHit2D hit =
                Physics2D.Raycast(transform.position, direction, targetDetectionRange, obstaclesLayerMask);
            //Verificación de bloqueo
            //Si el rayo no choca con NADA (hit.collider == null), o si choca con objeto que no está en la lista de obnstaculos,
            //se asume que es el jugador

            if (hit.collider == null || (playerLayerMask & (1 << hit.collider.gameObject.layer )) != 0)
            {
                //Línea de visión CLARA : El enemigo ve al jugador
                //USamos Debug.DrawRay solo en el editor para visualización
                Debug.DrawRay(transform.position, direction * targetDetectionRange, Color.green);
                colliders = new List<Transform>() { playerCollider.transform };
            }
            else
            {
                //Línea de visión BLOQUEADA (Muro, Barril, etc.)
                Debug.DrawRay(transform.position, direction * targetDetectionRange, Color.red);
                colliders = null; // No hay objetivos visibles
            }
        }
        else
        {
            //El jugador está fuera del rango de detección
            colliders = null;
        }
        // Asignar los resultados a la IA Data
        //Si colliders no es null, el enemigo tiene un objetivo, y la IA se activa
        aiData.targets = colliders;
    }

    private void OnDrawGizmosSelected()
    {
        if (showGizmos == false)
            return;
        //Dibuja el círculo de radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetDetectionRange);

        if (colliders == null)
            return;

        Gizmos.color = Color.magenta;
        foreach (var item in colliders)
        {
            Gizmos.DrawSphere(item.position, 0.3f);
        }
    }
}
