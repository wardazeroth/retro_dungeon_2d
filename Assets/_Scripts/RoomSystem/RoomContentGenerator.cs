using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum RoomType { Undefined, Start, Enemy, Treasure, Exit, KeyRoom }

public class RoomContentGenerator : MonoBehaviour
{
    [SerializeField]
    private RoomGenerator playerRoom, defaultRoom;

    List<GameObject> spawnedObjects = new List<GameObject>();

    [SerializeField]
    private GraphTest graphTest;

    public Transform itemParent;

    private Vector2Int playerRoomIndexKey;

    [SerializeField]
    private CinemachineVirtualCamera cinemachineCamera;

    public UnityEvent RegenerateDungeon;

    [Header("Item Spawning")]
    [SerializeField]
    private GameObject itemPrefab;
    [SerializeField]   
    private float itemSpawnProbability = 0.05f;
    [SerializeField]
    private int minDistancePotion = 5;
    [SerializeField]
    private int maxDistancePotion = 100;

    [Header("Exit and Key Spawning")]
    [SerializeField]
    private GameObject exitDoorPrefab;

    [SerializeField]
    private GameObject keyPrefab;
    [SerializeField]
    private GameObject chestPrefab;

    [Header("Porcentajes de clasificación salas")]
    //Porcentajes de costo máximo (0.0 a 1.0)
    [Range(0f, 1f)]
    [SerializeField]
    private float exitRoomPorcentaje = 0.9f;
    [SerializeField]
    private float treasureRoomMinPorcentaje = 0.3f;
    //Diccionario para almacenar la clasificación
    private Dictionary<Vector2Int, RoomType> classifiedRooms = new Dictionary<Vector2Int, RoomType>();
    private Vector2Int keyRoomIndexKey = Vector2Int.zero;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var item in spawnedObjects)
            {
                Destroy(item);
            }
            RegenerateDungeon?.Invoke();
        }
    }

    private void ClassifyRooms(DungeonData dungeonData)
    {
        classifiedRooms.Clear();
        keyRoomIndexKey = Vector2Int.zero;

        if (graphTest == null) return;

        int highestCost = graphTest.GetHighestDijkstraCost();

        if (highestCost == 0) highestCost = 1;

        Vector2Int furthestRoomIndex = Vector2Int.zero;
        float maxAverageCost = -1;

        //Diccionario temporal para almacenar costos de todas las salas
        Dictionary<Vector2Int, float> roomAverageCosts = new Dictionary<Vector2Int, float>();

        Dictionary<Vector2Int, HashSet<Vector2Int>> allRooms = new Dictionary<Vector2Int, HashSet<Vector2Int>>(dungeonData.roomsDictionary);

        foreach (var roomEntry in allRooms)
        {
            Vector2Int roomIndex = roomEntry.Key;
            HashSet<Vector2Int> floorPositions = dungeonData.GetRoomFloorWithoutCorridors(roomIndex);

            if (floorPositions.Count == 0) continue;

            //2. Calcular el costo promedio (o máximo, si es mas fiable) de la Sala
            int totalCost = 0;

            foreach (Vector2Int position in floorPositions)
            {
                totalCost += graphTest.GetDijsktraCost(position);
            }
            float averageCost = (float)totalCost / floorPositions.Count;
            float normalizedCost = averageCost / highestCost;

            RoomType type;

            // 3. Clasificar la sala
            if (normalizedCost >= exitRoomPorcentaje)
            {
                type = RoomType.Exit;
            }
            else if (normalizedCost >= treasureRoomMinPorcentaje)
            {
                type = RoomType.Treasure;
            }
            else
            {
                type = RoomType.Enemy;
            }
            classifiedRooms.Add(roomIndex, type);

            //Rastrear la sala con el costo promedio más alto
            if (averageCost > maxAverageCost)
            {
                maxAverageCost = averageCost;
                furthestRoomIndex = roomIndex;
            }
        }

        // Lígica de garantía de Sala única
        if (classifiedRooms.ContainsKey(furthestRoomIndex))
        {
            //1. Convertir cualquier otra sala clasificada como Exit a Treasure
            var roomsToFix = classifiedRooms.Where(kvp => kvp.Value == RoomType.Exit && kvp.Key != furthestRoomIndex).ToList();
            foreach (var room in roomsToFix)
            {
                classifiedRooms[room.Key] = RoomType.Treasure;
            }
            classifiedRooms[furthestRoomIndex] = RoomType.Exit;
        }
        // Asegurra que la más lejana es la única exist room
        if (classifiedRooms.ContainsKey(playerRoomIndexKey))
        {
            classifiedRooms[playerRoomIndexKey] = RoomType.Start;
        }

        //Selección sala de la llave
        //Sala con el costo mas alto que NO sea la de inicio ni salida
        var keyRoomCandidate = roomAverageCosts
        .Where(kvp => kvp.Key != playerRoomIndexKey && kvp.Key != furthestRoomIndex)
        .OrderByDescending(kvp => kvp.Value)
        .FirstOrDefault();

        if (keyRoomCandidate.Key != Vector2Int.zero)
        {
            keyRoomIndexKey = keyRoomCandidate.Key;

            classifiedRooms[keyRoomIndexKey] = RoomType.KeyRoom;
            Debug.Log($"[CLAVE] Sala de llave seleccionada : {keyRoomIndexKey} con costo promedio : {keyRoomCandidate.Value}");
        }
    }
    public void GenerateRoomContent(DungeonData dungeonData)
    {
        foreach (GameObject item in spawnedObjects.ToList())
        {
            if (item != null)
            {
                if (itemParent != null && !Application.isPlaying)
                {
                    // Limpiar inmediatamente todos los hijos del contenedor en Modo Editor
                    // Esto elimina cualquier objeto acumulado de la sesión anterior.
                    int childCount = itemParent.childCount;
                    for (int i = childCount - 1; i >= 0; i--)
                    {
                        Transform child = itemParent.GetChild(i);
                        if (child != null && child.gameObject != null)
                        {
                            // Desparentar primero para evitar problemas con referencias de Prefab
                            child.SetParent(null);
                            DestroyImmediate(child.gameObject);
                        }
                    }
                }

                // 🌟 ¡SOLUCIÓN! Usar DestroyImmediate en el Editor, Destroy en el juego.
                if (Application.isPlaying)
                {
                    Destroy(item); // Modo Juego
                }
                else
                {
                    if (item.transform.parent != null)
                    {
                        item.transform.SetParent(null);
                    }

                    DestroyImmediate(item); // Modo Editor (para evitar acumulación)
                }
            }

        }
        spawnedObjects.Clear();

        SelectPlayerSpawnPoint(dungeonData);
        ClassifyRooms(dungeonData);
        SelectEnemySpawnPoints(dungeonData);
        SelectItemSpawnPoints(dungeonData);

        foreach (GameObject item in spawnedObjects)
        {
            if (item != null)
                item.transform.SetParent(itemParent, false);
        }
    }

    private void SelectPlayerSpawnPoint(DungeonData dungeonData)
    {
        int roomCount = dungeonData.roomsDictionary.Count;

        // Si no hay salas, salimos para evitar errores
        if (roomCount == 0)
        {
            Debug.LogError("ERROR: El diccionario de salas está vacío. El generador no rellenó las salas.");
            return;
        }

        int randomRoomIndex = UnityEngine.Random.Range(0, roomCount);
        // 🌟 NUEVO: OBTENER LA POSICIÓN SEGURA MÁS CERCANA AL CENTRO
        Vector2Int roomCenter = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);

        Vector2Int playerSpawnPoint = GetValidFloorPosition(roomCenter, dungeonData.floorPositions);

        Vector2Int roomIndexToRemove = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);
        playerRoomIndexKey = roomIndexToRemove;
        // Si GetValidFloorPosition devuelve Vector2Int.zero, algo salió muy mal.
        if (playerSpawnPoint == Vector2Int.zero)
        {
            Debug.LogError("Error fatal: No se encontró un punto de spawn válido en el dungeon.");
            return;
        }

        // 🌟 AÑADE ESTA LÍNEA DE DIAGNÓSTICO:
        Debug.Log($"[SPAWN DIAGNÓSTICO] Salas totales: {roomCount}. Generando jugador en la posición: {playerSpawnPoint}");

        if (graphTest != null)
        {
            graphTest.RunDjiskstraAlgorithm(playerSpawnPoint, dungeonData.floorPositions);

        }


        Vector2Int roomIndex = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);

        List<GameObject> placedPrefabs = playerRoom.ProcessRoom(
            playerSpawnPoint,
            dungeonData.roomsDictionary.Values.ElementAt(randomRoomIndex),
            dungeonData.GetRoomFloorWithoutCorridors(roomIndex)
            );

        FocusCameraOnThePlayer(placedPrefabs[placedPrefabs.Count - 1].transform);

        spawnedObjects.AddRange(placedPrefabs);
        dungeonData.roomsDictionary.Remove(playerRoomIndexKey);
    }

    private void SelectItemSpawnPoints(DungeonData dungeonData)
    {
        if (itemPrefab == null)
        {
            Debug.LogError("El Prefab del Ítem no está asignado en el Inspector de RoomContentGenerator.");
            return;
        }

        if (graphTest == null) return;

        HashSet<Vector2Int> corridorPositions = dungeonData.corridorPositions;

        foreach (Vector2Int position in dungeonData.floorPositions)
        {
            if (corridorPositions != null && corridorPositions.Contains(position))
            {
                continue;
            }
            // 2. Obtner clasificaión de la sala
            Vector2Int roomIndex = dungeonData.GetRoomIndexForTile(position);

            if (classifiedRooms.ContainsKey(roomIndex) && classifiedRooms[roomIndex] == RoomType.Treasure)
            {
                if (UnityEngine.Random.value < itemSpawnProbability)
                {
                    //Instanciar el item, centrándolo en la baldosa (+0.5)
                    GameObject item = Instantiate(itemPrefab, new Vector3(position.x + 0.5f, position.y + 0.5f, 0),
                        Quaternion.identity);
                    spawnedObjects.Add(item);
                }
            }
        }
    }

    private Vector2Int GetValidFloorPosition(Vector2Int desiredCenter, HashSet<Vector2Int> floorPositions)
    {
        // 1. Probar el centro deseado (la forma más rápida)
        if (floorPositions.Contains(desiredCenter))
        {
            return desiredCenter;
        }

        // 2. Si el centro no es suelo (es pared o nulo), buscamos en el área circundante.
        for (int radius = 1; radius < 10; radius++) // Buscar hasta 10 baldosas de distancia
        {
            // Iteramos sobre un área cuadrada alrededor del centro
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2Int potentialPos = desiredCenter + new Vector2Int(x, y);
                    if (floorPositions.Contains(potentialPos))
                    {
                        return potentialPos; // ¡Encontramos un punto válido!
                    }
                }
            }
        }

        // 3. Fallback: Si no se encuentra nada, devolvemos el primer punto de suelo disponible.
        // Esto solo debería ocurrir si la sala no se conectó al dungeon.
        if (floorPositions.Count > 0)
        {
            return floorPositions.ElementAt(UnityEngine.Random.Range(0, floorPositions.Count));
        }

        return Vector2Int.zero; // Fallback extremo
    }

    private void FocusCameraOnThePlayer(Transform playerTransform)
    {
        cinemachineCamera.LookAt = playerTransform;
        cinemachineCamera.Follow = playerTransform;
    }

    private void SelectEnemySpawnPoints(DungeonData dungeonData)
    {
        // Verificar si las herramientas de IA están disponibles en este controlador
        // El ContextSolver y GraphTest deben ser componentes de este GameObject.
        ContextSolver solver = GetComponent<ContextSolver>();
        GraphTest graphRunner = GetComponent<GraphTest>();

        if (dungeonData.roomsDictionary.Count > 0)
        {
            keyRoomIndexKey = dungeonData.roomsDictionary.Keys.Last();
            classifiedRooms[keyRoomIndexKey] = RoomType.KeyRoom; // Asegura que esté clasificado
        }

        // 1. Iterar sobre las salas restantes (las que no son la sala del jugador)
        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> roomData in dungeonData.roomsDictionary)
        {
            Vector2Int roomIndex = roomData.Key;  //Clave de la sala (roomCenter)

            // Logica de clasificación de la sala
            if (classifiedRooms.ContainsKey(roomIndex))
            {
                RoomType roomType = classifiedRooms[roomIndex];
                if (roomType == RoomType.Exit)
                {
                    if (exitDoorPrefab != null)
                    {
                        //Determinar posición de la puerta
                        Vector2Int spawnPosition = roomIndex;

                        // Opcional: Buscar un punto libre en el centro de la sala
                        spawnPosition = GetValidFloorPosition(roomIndex, dungeonData.GetRoomFloorWithoutCorridors(roomIndex));

                        if (spawnPosition != Vector2Int.zero)
                        {
                            GameObject door = Instantiate(exitDoorPrefab, new Vector3(spawnPosition.x + 0.5f, spawnPosition.y + 0.5f, 0),
                                                     Quaternion.identity);
                            spawnedObjects.Add(door);                            
                        }
                        continue;
                    }                    
                }

                //Generación de Cofre con llave
                if (roomType == RoomType.KeyRoom && chestPrefab != null)
                {
                    Vector2Int spawnPosition = GetValidFloorPosition(roomIndex, dungeonData.GetRoomFloorWithoutCorridors(roomIndex));

                    if (spawnPosition != Vector2Int.zero)
                    {
                        GameObject chestInstance = Instantiate(chestPrefab, new Vector3(spawnPosition.x + 0.5f, spawnPosition.y + 0.5f, 0),
                                   Quaternion.identity);
                        spawnedObjects.Add(chestInstance);

                        //if (chestInstance.TryGetComponent<Chest>(out Chest chestComponent))
                        //{
                        //    if (keyPrefab != null)
                        //    {
                        //        chestComponent.InitializeContent(keyPrefab);
                        //        Debug.Log($"[INYECCIÓN] Llave inyectada en cofre.");
                        //    }
                        //    else
                        //    {
                        //        Debug.LogError("La referencia keyPrefab es nula en el Inspector.");
                        //    }
                        //}
                        //else
                        //{
                        //    Debug.LogError($"[FALLO CRÍTICO] La instancia del Cofre ({chestInstance.name}) NO tiene el script Chest.cs adjunto.");
                        //}
                    }
                    //if (keyPrefab == null)
                    //{
                    //    Debug.LogError("FATAL: Key Prefab es nulo en RoomContentGenerator, no se puede inyectar la llave.");
                    //    // Si esto es nulo, la reasignación en el Inspector falló.
                    //}

                    //Vector2Int spawnPosition = GetValidFloorPosition(roomIndex, dungeonData.GetRoomFloorWithoutCorridors(roomIndex));

                    //if (spawnPosition != Vector2Int.zero)
                    //{
                    //    //Instanciar el cofre
                    //    GameObject chestInstance = Instantiate(chestPrefab, new Vector3(spawnPosition.x + 0.5f, spawnPosition.y + 0.5f, 0),
                    //                           Quaternion.identity);
                    //    spawnedObjects.Add(chestInstance);

                    //    if (chestInstance.TryGetComponent<Chest>(out Chest chestComponent))
                    //    {
                    //        GameObject keyToInject = keyPrefab;

                    //        // 🛑 LÓGICA DE RECUPERACIÓN DE REFERENCIA (Si el Inspector falla) 🛑
                    //        if (keyToInject == null)
                    //        {
                    //            // Si la referencia serializada (keyPrefab) es nula, FORZAMOS la recuperación
                    //            // Asume que tu prefab se llama "Key_Prefab" y está en la carpeta Resources/Prefabs
                    //            // (O crea una carpeta Resources en tu proyecto y pon el prefab ahí para esta prueba)
                    //            Debug.LogWarning("Key Prefab es nulo. Intentando cargar desde Resources.");
                    //            keyToInject = Resources.Load<GameObject>("Key_Prefab");
                    //        }
                    //        // 🛑 FIN RECUPERACIÓN 🛑

                    //        if (keyToInject != null)
                    //        {
                    //            chestComponent.InitializeContent(keyToInject);
                    //            Debug.Log($"[INYECCIÓN] Llave inyectada en cofre.");
                    //        }
                    //        else
                    //        {
                    //            Debug.LogError("¡ERROR FATAL DE ASSET! No se pudo inyectar la llave. Revisa la asignación de Key Prefab.");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Debug.LogError($"[FALLO CRÍTICO] La instancia del Cofre ({chestInstance.name}) NO tiene el script Chest.cs adjunto.");
                    //    }

                    //    ////2. Obtener el script e inicializarlo con la llave
                    //    //Chest chestComponent = chestInstance.GetComponent<Chest>();
                    //    //if (chestComponent != null && keyPrefab != null)
                    //    //{
                    //    //    //Inyección de la llave
                    //    //    chestComponent.InitializeContent(keyPrefab);
                    //    //}
                    //    //else if (chestComponent == null)
                    //    //{
                    //    //    Debug.Log($"Falta el script Chest.cs en el Prefab del Cofre");
                    //    //}
                    //}
                }
            }

            // 2. Generar Enemigos e Ítems para esta sala
            // La lista 'placedContent' está declarada dentro del alcance del foreach.
            List<GameObject> placedContent = defaultRoom.ProcessRoom(
                roomData.Key, // roomCenter
                roomData.Value, // roomFloor
                dungeonData.GetRoomFloorWithoutCorridors(roomData.Key) // roomFloorNoCorridors
            );

            // 3. 🌟 PASO CRUCIAL: INYECCIÓN DE REFERENCIAS DE IA 🌟

            foreach (GameObject content in placedContent)
            {
                EnemyAI enemyAI = content.GetComponent<EnemyAI>();

                // Solo inyectar si el objeto es un enemigo y las herramientas existen
                if (enemyAI != null)
                {
                    //// La referencia debe ser pública en EnemyAI.cs para esta inyección (como corregimos)
                    //if (graphRunner != null)
                    //{
                    //    enemyAI.graphTest = graphRunner; // ⬅️ Inyecta el componente GraphTest
                    //}

                    //if (solver != null)
                    //{
                    //    enemyAI.movementDirectionSolver = solver; // ⬅️ Inyecta el Fallback ContextSolver
                    //}
                }
            }

            // 4. Añadir el contenido modificado a la lista global para la limpieza
            spawnedObjects.AddRange(placedContent);
        }
    }

}
