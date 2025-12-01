using System.Collections.Generic;
using UnityEngine;

// Asegúrate de que las clases EnemyPlacementData, ItemPlacementData y PrefabPlacer existan,
// aunque su lógica esté comentada aquí.

[RequireComponent(typeof(PrefabPlacer))] // ⬅️ ¡AÑADE ESTO!
public class FightingPitRoom : RoomGenerator
{

    [SerializeField]
    private PrefabPlacer prefabPlacer;

    public List<EnemyPlacementData> enemyPlacementData;
    public List<ItemPlacementData> itemData;

    // ----------------------------------------------------
    // LÓGICA DE PROCESAMIENTO
    // ----------------------------------------------------

    public override List<GameObject> ProcessRoom(
    Vector2Int roomCenter,
    HashSet<Vector2Int> roomFloor,
    HashSet<Vector2Int> roomFloorNoCorridors)
    {
        if (prefabPlacer == null)
        {
            // Busca el componente PrefabPlacer en este mismo GameObject (la instancia clonada).
            prefabPlacer = GetComponent<PrefabPlacer>();
            if (prefabPlacer == null)
            {
                Debug.LogError("FATAL ERROR: No se encontró el componente PrefabPlacer en FightingPitRoomPrefab.");
                return new List<GameObject>(); // Detener el spawn si no hay herramienta.
            }
        }
        //Inicializa el ItemPlacementHelper, que clasifica todas las baldosa de la sala
        // en OpenSpace(centro) o NearWall (cerca de paredes)
        ItemPlacementHelper itemPlacementHelper =
            new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);

        List<GameObject> placedObjects = new List<GameObject>();
        // 2. COLOCAR ITEMS:
        //Llama al perefabPlacer para instanciar todos los items definidos en 'itemData'.
        //El PrefabPlacer utilizará el 'itemPlacemenbtHelper' para encontrar posiciones válidas.
        placedObjects.AddRange(
            prefabPlacer.PlaceAllItems(itemData, itemPlacementHelper));

        // 3. COLOCAR ENEMIGOS:
        // 🌟 CAPTURA LA LISTA DE ENEMIGOS INSTANCIADOS 🌟
        List<GameObject> spawnedEnemies = prefabPlacer.PlaceEnemies(enemyPlacementData, itemPlacementHelper);

        // Añade los enemigos a la lista principal de objetos colocados.
        placedObjects.AddRange(spawnedEnemies);
        // -----------------------------------------------------------------

        // 🌟 NUEVA LÍNEA DE DEPURACIÓN (Usando el contador capturado)
        Debug.Log($"[DEBUG SPAWN] Sala {roomCenter} intentó spawnear {spawnedEnemies.Count} enemigos.");

        // Esto se ejecuta para todas las salas que NO son la sala del jugador.

        // Console log para confirmar que la sala fue procesada.
        Debug.Log($"Sala de combate en {roomCenter} omitida temporalmente (Placeholder).");

        // Importante: Devolver una lista vacía para evitar NullReferenceException.
        return placedObjects;
    }
}