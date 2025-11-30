using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class RoomContentGenerator : MonoBehaviour
{
    [SerializeField]
    private RoomGenerator playerRoom, defaultRoom;

    List<GameObject> spawnedObjects = new List<GameObject>();

    [SerializeField]
    //private GraphTest graphTest;


    public Transform itemParent;

    [SerializeField]
    private CinemachineVirtualCamera cinemachineCamera;

    public UnityEvent RegenerateDungeon;

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
    public void GenerateRoomContent(DungeonData dungeonData)
    {
        foreach (GameObject item in spawnedObjects)
        {
            if (item != null)
            {
                // 🌟 ¡SOLUCIÓN! Usar DestroyImmediate en el Editor, Destroy en el juego.
                if (Application.isPlaying)
                {
                    Destroy(item); // Modo Juego
                }
                else
                {
                    DestroyImmediate(item); // Modo Editor (para evitar acumulación)
                }
            }

        }
        spawnedObjects.Clear();

        SelectPlayerSpawnPoint(dungeonData);
        SelectEnemySpawnPoints(dungeonData);

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

        // Si GetValidFloorPosition devuelve Vector2Int.zero, algo salió muy mal.
        if (playerSpawnPoint == Vector2Int.zero)
        {
            Debug.LogError("Error fatal: No se encontró un punto de spawn válido en el dungeon.");
            return;
        }

        // 🌟 AÑADE ESTA LÍNEA DE DIAGNÓSTICO:
        Debug.Log($"[SPAWN DIAGNÓSTICO] Salas totales: {roomCount}. Generando jugador en la posición: {playerSpawnPoint}");

        //graphTest.RunDijkstraAlgorithm(playerSpawnPoint, dungeonData.floorPositions);

        Vector2Int roomIndex = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);

        List<GameObject> placedPrefabs = playerRoom.ProcessRoom(
            playerSpawnPoint,
            dungeonData.roomsDictionary.Values.ElementAt(randomRoomIndex),
            dungeonData.GetRoomFloorWithoutCorridors(roomIndex)
            );

        FocusCameraOnThePlayer(placedPrefabs[placedPrefabs.Count - 1].transform);

        spawnedObjects.AddRange(placedPrefabs);

        dungeonData.roomsDictionary.Remove(playerSpawnPoint);
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
        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> roomData in dungeonData.roomsDictionary)
        {
            spawnedObjects.AddRange(
                defaultRoom.ProcessRoom(
                    roomData.Key,
                    roomData.Value,
                    dungeonData.GetRoomFloorWithoutCorridors(roomData.Key)
                    )
            );

        }
    }

}
