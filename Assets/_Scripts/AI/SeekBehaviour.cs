using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SeekBehaviour : SteeringBehaviour
{
    [SerializeField]
    private float targetRechedThreshold = 0.5f; //Distancia para considerar que se ha llegado al objetivo

    [SerializeField]
    private bool showGizmo = true;
    bool reachedLastTarget = true;

    private Vector2 targetPositionCached;
    private float[] interestsTemp;

    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        //1. Si la IA ya alcanzó su ultimo punto (o no tiene uno), busca uno nuevo
        if (reachedLastTarget)
        {
            if (aiData.targets == null || aiData.targets.Count <= 0)
            {
                //Si no hay objetivos visibles, dejar de buscar
                aiData.currentTarget = null;
                return (danger, interest);
            }
            else
            {
                //Encontró un objetivo, selecciona el más cercano
                reachedLastTarget = false;
                aiData.currentTarget = aiData.targets.OrderBy(
                    target => Vector2.Distance(target.position, transform.position)).FirstOrDefault();
            }
        }
        //Cacha la posición del objetivo actual (sscar un nuevo objetivo en el próximo ciclo.
        if (aiData.currentTarget != null && aiData.targets != null && aiData.targets.Contains(aiData.currentTarget))
            targetPositionCached = aiData.currentTarget.position;
        //VERIFICACIÓN DE LLEGADA

        //Hemos llegado al destino?
        if (Vector2.Distance(transform.position, targetPositionCached) < targetRechedThreshold)
        {
            //Hemos llegado. Reinicia el estado para buscar un nuevo objetivo en el proximo ciclo
            reachedLastTarget = true;
            aiData.currentTarget = null;
            return (danger, interest);
        }

        //CALCULO DE INTERES
        //Si no hemos llegado, calcula qué direcciones se acercan al objetivo
        Vector2 directionToTarget = (targetPositionCached - (Vector2)transform.position);

        for (int i= 0; i < interest.Length; i++)
        {
            //Compara la dirección al objetivo con cada una de las 8 direcciones posibles
            float result = Vector2.Dot(directionToTarget.normalized, Directions.eightDirections[i]);
            //Solo nos interesan las direcciones que están dentro de un ángulo de 90 grados (>0)
            if (result > 0)
            {
                float valueToPutIn = result;
                if (valueToPutIn > interest[i])
                {
                    interest[i] = valueToPutIn;
                }
            }
        }
        interestsTemp = interest;
        float maxInterest = interest.Max();
        if (maxInterest == 0) Debug.LogError($"[SEEK FAIL] Interés máximo es CERO! Distancia: {Vector2.Distance(transform.position, targetPositionCached)}");
        return (danger, interest);
    }

    private void OnDrawGizmos()
    {

        if (showGizmo == false)
            return;
        Gizmos.DrawSphere(targetPositionCached, 0.2f);

        if (Application.isPlaying && interestsTemp != null)
        {
            if (interestsTemp != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < interestsTemp.Length; i++)
                {
                    Gizmos.DrawRay(transform.position, Directions.eightDirections[i] * interestsTemp[i]);
                }
                if (reachedLastTarget == false)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(targetPositionCached, 0.1f);
                }
            }
        }
    }

}
