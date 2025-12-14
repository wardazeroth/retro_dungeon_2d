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
            // El grafo está asignado? (Significa que ForceAIGraphInjection ya se ejecutó)
            if (graphTest != null)
            {
                InitializeAI(); // Iniciar la detección de una vez por todas.
            }
            else
            {
                // Aún no hemos recibido la referencia. Esperamos en el próximo frame.
                return;
            }
        }

        // 🛑 2. CHEQUEO DE NULL DURANTE LA EJECUCIÓN 🛑
        if (graphTest == null)
        {
            // Si el grafo se pierde después de la inicialización, nos detenemos.
            DEBUG_MovementInput = Vector2.zero;
            return;
        }
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
            // Lógica de detención si el objetivo se pierde
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
            StartCoroutine(ChaseAndAttack()); // Reinicia el ciclo de ataque/persecucion
        }
        else
        {
            // FASE 2: PERSECUCIÓN

            if (graphTest != null)
            {
                // 🛑 1. OBTENER EL MAPA DE SUELO ALMACENADO Y POSICIÓN DEL JUGADOR 🛑
                var currentFloorMap = graphTest.FloorPositions;

                if (currentFloorMap != null)
                {
                    Vector2Int currentPlayerGridPosition = Vector2Int.FloorToInt(aiData.currentTarget.position);

                    // 🛑 2. FORZAR RECALCULO DE DIJKSTRA (CRÍTICO para persecución dinámica) 🛑
                    // Esto es necesario en cada tick ya que el jugador no lo hace.
                    graphTest.RunDjiskstraAlgorithm(currentPlayerGridPosition, currentFloorMap);

                    // 3. Lógica de Pathfinding (Usando el mapa recién actualizado)
                    Vector2Int currentGridPosition = Vector2Int.RoundToInt(transform.position);

                    // Consultar el mapa de Djikstra: ¿Cuál es el siguiente paso más cercano al jugador?
                    Vector2Int nextGridStep = graphTest.GetDirectionToLowestCostNeighbour(currentGridPosition);

                    // SI la posición de la cuadrícula actual es diferente al mejor vecino
                    if (nextGridStep != currentGridPosition)
                    {
                        // Calcular el vector de movimiento (NextStep, CurrentPosition)
                        DEBUG_MovementInput = (new Vector2(nextGridStep.x, nextGridStep.y) - new Vector2(currentGridPosition.x, currentGridPosition.y)).normalized;
                    }
                    else
                    {
                        // Si ya estamos en el tile óptimo (o estancado), usamos el Steering como fallback
                        DEBUG_MovementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                    }
                }
                else
                {
                    // Si FloorPositions es null, significa que RunDjiskstraAlgorithm nunca se ejecutó con datos válidos.
                    DEBUG_MovementInput = Vector2.zero;
                    // Este error indica que el flujo de regeneración/inyección es incorrecto o la escena está corrupta.
                    Debug.LogError("[IA FATAL] El mapa de suelo almacenado en GraphTest es nulo. Fallo al iniciar persecución.");
                }
            }
            else
            {
                // Fallback si Graphtest es null (la inyección falló)
                DEBUG_MovementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                Debug.Log($"[SOLVER OUTPUT] Vector: {DEBUG_MovementInput.ToString()}");
            }

            yield return new WaitForSeconds(aiUpdateDelay); // Esperar el tiempo de actualización de la IA
            StartCoroutine(ChaseAndAttack()); // Reiniciar el ciclo
        }
    }
}

