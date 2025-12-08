using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextSolver : MonoBehaviour
{
    [SerializeField]
    private bool showGizmos = true;

    //Arrays de clase para almacenar las fuerzas de interés y de peligro
    //Se inicializan con 8 posiciones (para las 8 direcciones posibles)
    private float[] interest = new float[8];
    private float[] danger = new float[8];

    //Variables de depuración (Gizmos)
    [Header("debug")]
    float[] interestGizmo = new float[0];
    Vector2 resultDirection = Vector2.zero;
    private float rayLength = 1;

    private void Start()
    {
        //Se asegura que el array de depuración esté ionicializado
        interestGizmo = new float[8];
    }

    //Método principal: Toma las 'opiniones' y devuelve la mejor dirección
    public Vector2 GetDirectionToMove(List<SteeringBehaviour> behaviours, AIData aiData)
    {
        //1. Reiniciar arrays
        //Reiniciamos los arrays a cero antes de cada cálculo para que las fuerzas
        ////no se acumulen de un frame a otro
        for (int i = 0; i < 8; i++)
        {
            interest[i] = 0f;
            danger[i] = 0f;
        }

        //Recibir opiniones (steering)
        //Recorremos cada SteeringBehaviour (Seek, Avoidance, etc.).
        //Cada comportamiento rellena el array 'danger' o 'interest'.
        foreach (SteeringBehaviour behaviour in behaviours)
        {
            //El comportamiento de Steering devuelve los arrays actualizados
            (danger, interest) = behaviour.GetSteering(danger, interest, aiData);
        }

        // 3. FUSIÓN: RESTAR PELIGRO DE INTERÉS
        //Restamos el valor de Peligro (obstáculos) del valor de Interés (Objetivo)
        //Sie el preligro es alto en una dirección, el interés se anula
        for (int i = 0; i < 8; i++)
        {
            //Aseguramos que el valor resultante nunca sea negativo
            interest[i] = Mathf.Clamp01(interest[i] - danger[i]);
        }

        //Guardar para Gizmos de depuración
        interestGizmo = interest;

        //4. SELECCIÓN FINAL (Weighted Sum)
        //Encontramos la dirección con el valor más alto
        Vector2 outputDirection = Vector2.zero;
        float highestResult = 0f;
        int bestDirectionIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            float currentResult = interest[i];

            //Si esta dirección tiene un resultado (interés - peligro) mayor que el actual máximo
            if (currentResult > highestResult)
            {
                highestResult = currentResult;
                bestDirectionIndex = i; //Guardamos el indice de la direccion mas fuerte
            }
        }

        //Devolver el vector de movimiento
        //CRITICO : Sie el máximo de inter´res es mayor a cero, devolvemos el vector completo (magnitud 1)
        //Esto garantiza que el enemigo siempre se mueva a velocidad máxima en la dirección elegida
        if (highestResult > 0)
        {
            //Usamos la dirección correspondiente al índice ganador
            outputDirection = Directions.eightDirections[bestDirectionIndex];
        }
        else
        {
            //Si no hay ningún interés (ej, ya está en el objetivo o completamente bloqueado), devolvemos cero.
            outputDirection = Vector2.zero;
        }

        //El vector resultante ya no necesita Normalizarse, ya que lo obtenemos
        // de un array de direcciones normalizadas
        resultDirection = outputDirection;
        return resultDirection;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && showGizmos)
        {
            Gizmos.color = Color.yellow;
            // Dibuja la dirección final de movimiento
            Gizmos.DrawRay(transform.position, resultDirection * rayLength);

            // Opcional: Dibuja todas las direcciones de interés (para ver las fuerzas)
            // if (interestGizmo.Length > 0)
            // {
            //     for (int i = 0; i < 8; i++)
            //     {
            //         if (interestGizmo[i] > 0)
            //         {
            //             Gizmos.color = Color.Lerp(Color.blue, Color.red, interestGizmo[i]);
            //             Gizmos.DrawRay(transform.position, Directions.eightDirections[i] * interestGizmo[i] * rayLength);
            //         }
            //     }
            // }
        }
    }
}
