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
    public GraphTest graphTest;

    [Header("Control de Persecución")]
    [SerializeField]
    private float maxChaseDistance = 15f;

    //Inputs enviados desde la IA al controlador de miovimiento del enemigo
    public UnityEvent OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;

    [SerializeField]
    public Vector2 DEBUG_MovementInput;

    [SerializeField]
    public ContextSolver movementDirectionSolver;

    bool following = false;

    bool isInitialized = false;

    // Start is called before the first frame update
    void Start()
    {
        //// 🛑 IMPORTANTE: Busca el objeto central por su nombre EXACTO 
        //GameObject spawnerObject = GameObject.Find("_RoomSpawnerController");

        //if (spawnerObject != null)
        //{
        //    // Obtener las herramientas directamente del objeto encontrado
        //    graphTest = spawnerObject.GetComponent<GraphTest>();
        //    movementDirectionSolver = spawnerObject.GetComponent<ContextSolver>();
        //}
        //else
        //{
        //    Debug.LogError("Error FATAL de IA: No se encontró el objeto central '_RoomSpawnerController'.");
        //}

        //if (graphTest == null)
        //{
        //    graphTest = FindObjectOfType<GraphTest>();
        //}
        //if (movementDirectionSolver == null)
        //{
        //    movementDirectionSolver = FindObjectOfType<ContextSolver>();
        //}

        if (graphTest == null)
        {
            Debug.LogWarning($"[IA WARNING] Enemigo {gameObject.name}: GraphTest es nulo en Start. Esperando inyección...");
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

    private void InitializeAI()
    {
        // Solo se llama una vez para iniciar la detección periódica
        InvokeRepeating("PerformDetection", 0, detectionDelay);
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized)
        {
            if (graphTest != null)
            {
                InitializeAI();
            }
            else
            {
                return;
            }
        }

        if (graphTest == null)
        {
            DEBUG_MovementInput = Vector2.zero;
            return;
        }

        // 1. Adquisición del objetivo (por Detectors)
        if (aiData.currentTarget != null)
        {
            // 🛑 LÍMITE DE ALCANCE Y RENDIMIENTO 🛑
            float distanceToTarget = Vector2.Distance(aiData.currentTarget.position, transform.position);

            if (distanceToTarget < maxChaseDistance)
            {
                // Dentro de rango: Perseguir
                OnPointerInput?.Invoke(aiData.currentTarget.position);

                if (following == false)
                {
                    following = true;
                    StartCoroutine(ChaseAndAttack());
                }
            }
            else
            {
                // Fuera de rango: Detener la persecución y el movimiento
                if (following == true)
                {
                    following = false;
                    // Detenemos la corrutina de persecución para ahorrar CPU y dejar de seguir infinitamente.
                    StopCoroutine(ChaseAndAttack());
                    DEBUG_MovementInput = Vector2.zero;
                }
            }
        }
        else if (aiData.GetTargetsCount() > 0)
        {
            aiData.currentTarget = aiData.targets[0];
        }
        else if (following == true)
        {
            // Si target se pierde (ej. se esconde detrás de un muro), detenemos.
            following = false;
            StopCoroutine(ChaseAndAttack());
            DEBUG_MovementInput = Vector2.zero;
        }

        OnMovementInput?.Invoke(DEBUG_MovementInput);
    }

    //LOGICA DE PERSECUCION Y ATAQUE
    private IEnumerator ChaseAndAttack()
    {
        if (aiData.currentTarget == null || following == false)
        {
            DEBUG_MovementInput = Vector2.zero;
            following = false;
            yield break;
        }

        // Determinar la acción (atacar o perseguir)
        float distance = Vector2.Distance(aiData.currentTarget.position, transform.position);

        if (distance < attackDistance)
        {
            // FASE 1: ATAQUE
            DEBUG_MovementInput = Vector2.zero;
            OnAttackPressed?.Invoke();
            yield return new WaitForSeconds(attackDelay);
            StartCoroutine(ChaseAndAttack());
        }
        else
        {
            // FASE 2: PERSECUCIÓN

            if (graphTest != null)
            {
                var currentFloorMap = graphTest.FloorPositions;

                if (currentFloorMap != null)
                {
                    // 🛑 ELIMINAMOS LA LINEA COSTOSA DE RECALCULO DE DIJKSTRA DE AQUÍ 🛑
                    // graphTest.RunDjiskstraAlgorithm(currentPlayerGridPosition, currentFloorMap); 

                    Vector2Int currentGridPosition = Vector2Int.RoundToInt(transform.position);
                    Vector2Int nextGridStep = graphTest.GetDirectionToLowestCostNeighbour(currentGridPosition);

                    if (nextGridStep != currentGridPosition)
                    {
                        DEBUG_MovementInput = (new Vector2(nextGridStep.x, nextGridStep.y) - new Vector2(currentGridPosition.x, currentGridPosition.y)).normalized;
                    }
                    else
                    {
                        DEBUG_MovementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                    }
                }
                else
                {
                    DEBUG_MovementInput = Vector2.zero;
                    Debug.LogError("[IA FATAL] El mapa de suelo almacenado en GraphTest es nulo. Fallo al iniciar persecución.");
                }
            }
            else
            {
                // Fallback si Graphtest es null
                DEBUG_MovementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
            }

            // El enemigo sigue consultando el mapa a la velocidad de 0.06s, pero el mapa solo cambia cada 0.2s (PlayerUpdater)
            yield return new WaitForSeconds(aiUpdateDelay);
            StartCoroutine(ChaseAndAttack());
        }
    }
}