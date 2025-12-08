using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidanceBehaviour : SteeringBehaviour
{
    [SerializeField]
    private float radius = 2f, agentColliderSize = 0.6f; // radio de detección de obstáculos 

    [SerializeField]
    private bool showGizmo = true;

    float[] dangersResultTemp = null;

    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        //Itera sobre todos los obstáculos detectados por el detector
        foreach (Collider2D obstacleCollider in aiData.obstacles)
        {
            //Calcula la dirección y distancia al obstáculo más cercano
            Vector2 directionToObstacle = obstacleCollider.ClosestPoint(transform.position) - (Vector2)transform.position;
            float distanceToObstacle = directionToObstacle.magnitude;

            //Calcula el peso (Weight): Cuanto más cerca esté, mayor será el peso (cercano a 1)
            float weight = distanceToObstacle <= agentColliderSize
                ? 1 //Si ya estamos colisionando, el peso es máximo(1)
                : (radius - distanceToObstacle) / radius; //Decrece linealmente hasta el radio

            Vector2 directionToObstacleNormalized = directionToObstacle.normalized;
            // Añade los parámetros de obstáculo al array de peligro
            for (int i= 0; i < Directions.eightDirections.Count; i++)
            {
                //Compara la dirección a la que apunta la baldosa con la dirección al obstáculo
                float result = Vector2.Dot(directionToObstacleNormalized, Directions.eightDirections[i]);

                //Multiplica la similitud por el peo (cercanía)
                float valueToPutIn = result * weight;

                //SOlo guarda el valor más alto (más peligroso) para cda dirección
                if (valueToPutIn > danger[i])
                {
                    danger[i] = valueToPutIn;
                }
            }
        }
        dangersResultTemp = danger;
        return (danger, interest);
    }

    private void OnDrawGizmos()
    {
        if (showGizmo == false)
            return;

        if (Application.isPlaying && dangersResultTemp != null)
        {
            if (dangersResultTemp != null)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < dangersResultTemp.Length; i++)
                {
                    Gizmos.DrawRay(
                        transform.position,
                        Directions.eightDirections[i] * dangersResultTemp[i]
                        );
                }
            }
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}


public static class Directions
{
    public static List<Vector2> eightDirections = new List<Vector2>{
            new Vector2(0,1).normalized,
            new Vector2(1,1).normalized,
            new Vector2(1,0).normalized,
            new Vector2(1,-1).normalized,
            new Vector2(0,-1).normalized,
            new Vector2(-1,-1).normalized,
            new Vector2(-1,0).normalized,
            new Vector2(-1,1).normalized
        };
}
