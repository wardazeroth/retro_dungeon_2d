using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour

{
    [SerializeField]
    private List<SteeringBehaviour> steeringBehaviours; // Lógica local para evitar obstáculos (ej: steering)

    [SerializeField]
    private List<Detector> detectors; // Lógica para encontrar al jugador (Line of Sight, Proximity)

    [SerializeField]
    private AIData aiData; //Contenedor de datos de IA

    [SerializeField]
    private float detectionDelay = 0.05f, aiUpdateDelay = 0.06f, attackDelay = 1f; //Tiempos de actualizacion

    [SerializeField]
    private float attackDistance = 0.5f; //distancia inicio de ataque

    //Referencia al componente que ejecuta el cálculo de ruta (almacena  el mapa de Djikstra)
    [Header("Dungeon Navigation")]
    [SerializeField]
    private GraphTest graphTest;

    //Inputs enviados desde la IA al controlador de miovimiento del enemigo
    public UnityEvent OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;

    [SerializeField]
    public Vector2 DEBUG_MovementInput;

    [SerializeField]
    private ContextSolver movementDirectionSolver;

    bool following = false;

    // Start is called before the first frame update
    void Start()
    {
        // 🛑 IMPORTANTE: Busca el objeto central por su nombre EXACTO 
        GameObject spawnerObject = GameObject.Find("_RoomSpawnerController");

        if (spawnerObject != null)
        {
            // Obtener las herramientas directamente del objeto encontrado
            graphTest = spawnerObject.GetComponent<GraphTest>();
            movementDirectionSolver = spawnerObject.GetComponent<ContextSolver>();
        }
        else
        {
            Debug.LogError("Error FATAL de IA: No se encontró el objeto central '_RoomSpawnerController'.");
        }
        //Iniciar la detección y obstáculos del jugador de forma periódica
        InvokeRepeating("PerformDetection", 0, detectionDelay);
    }

    private void PerformDetection()
    {
        //Ejecuta todos los deetectores (ej: comprueba si el jugador está en rango)
        foreach (Detector detector in detectors)
        {
            detector.Detect(aiData);
        }
        if (aiData.currentTarget != null)
        {
            Debug.Log($"[DETECCIÓN] Enemigo '{gameObject.name}' te está persiguiendo.");
        }
    }

    private void Update()
    {
        //1. Adquisición del objetivo
        if (aiData.currentTarget != null)
        {
            //Apuntar al objetivo
            OnPointerInput?.Invoke(aiData.currentTarget.position);

            //Si no estamos persiguiendo, inicamos la corrutina de persecución
            if (following == false)
            {
                following = true;
                StartCoroutine(ChaseAndAttack());
            }
        }
        else if (aiData.GetTargetsCount() > 0)
        {
            // Si se detecta un objetivo, pero current target es null, lo adquirimos
            aiData.currentTarget = aiData.targets[0];
        }
        OnMovementInput?.Invoke(DEBUG_MovementInput);
    }

    //LOGICA DE PERSECUCION Y ATAQUE
    private IEnumerator ChaseAndAttack()
    {
        if (aiData.currentTarget == null)
        {
            //Lógica de detención si el objetivo se pierde
            DEBUG_MovementInput = Vector2.zero;
            following = false;
            yield break;
        }

        //Determinar la acción (atacar o perseguir)
        float distance = Vector2.Distance(aiData.currentTarget.position, transform.position);

        if (distance < attackDistance)
        {
            //FASE 1: ATAQUE
            DEBUG_MovementInput = Vector2.zero;
            OnAttackPressed?.Invoke();
            yield return new WaitForSeconds(attackDelay);
            StartCoroutine(ChaseAndAttack()); //Reinicia el ciclo de ataque/persecucion
        }
        else
        {
            //FASE 2: PERSECUCIÓN
                
            ////Lógica de Pathfinding por Gráfico
            if (graphTest != null)
            {
                //Convertir la posición del enemigo a coordenadas de la cuadrícula
                Vector2Int currentGridPosition = Vector2Int.RoundToInt(transform.position);

                //Consultar el mapa de Djikstra: ¿Cuál es el siguiente paso más cercano al jugador?
                Vector2Int nextGridStep = graphTest.GetDirectionToLowestCostNeighbour(currentGridPosition);

                //// 🛑 AÑADE ESTE DEBUG.LOG AQUÍ 🛑
                //Debug.Log($"[DIJKSTRA DEBUG] Enemigo: {gameObject.name}. Actual: {currentGridPosition}. Siguiente: {nextGridStep}. Target: {aiData.currentTarget.position}");
                //SI la posición de la cuadrícula actual es diferente al mejor vecino
                if (nextGridStep != currentGridPosition)
                {
                    // Calcular el vector de movimiento (NextStep, CurrentPosition)
                    DEBUG_MovementInput = (new Vector2(nextGridStep.x, nextGridStep.y) - new Vector2(currentGridPosition.x, currentGridPosition.y)).normalized;
                }
                else
                {
                    //Si ya estamos en el tile óptimo (congelado), usamos el Steering como fallback
                    //Esto asume que el Steering Behaviour está configurado para mover el enemigo fuera de la celda actual)
                    DEBUG_MovementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                }
            }
            else
            {
                Debug.LogError("Error FATAL de IA: No se encontró el objeto central '_RoomSpawnerController'."); // ⬅️ AQUÍ ESTÁ EL LOG
                //Falback si Graphtest no está asignado, usa solo Steering
                DEBUG_MovementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                Debug.Log($"[SOLVER OUTPUT] Vector: {DEBUG_MovementInput.ToString()}");
            }

            yield return new WaitForSeconds(aiUpdateDelay); //Esperar el tiempo de actualización de la IA
            //// Calcula la dirección directamente al objetivo (persiguiendo)
            //Vector2 directionToTarget = (aiData.currentTarget.position - transform.position).normalized;

            //// Asigna el vector forzado al input
            //DEBUG_MovementInput = directionToTarget;

            //// Debug para verificar que la IA está CALCULANDO el vector
            //Debug.Log($"[FUERZA DEBUG] Vector FORZADO: {DEBUG_MovementInput.ToString()}");

            //// Ya no esperamos el tiempo de actualización de la IA (aiUpdateDelay) en la prueba forzada
            //// El enemigo debe moverse inmediatamente.

            //yield return null; // Espera 1 frame para la recursión

            StartCoroutine(ChaseAndAttack()); //Reiniciar el ciclo
        }
    }
}

