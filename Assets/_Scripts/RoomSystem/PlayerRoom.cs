using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoom : RoomGenerator
{
    public GameObject player; //prefab del jugador

    [SerializeField]
    private PrefabPlacer prefabPlacer;

    
    public override List<GameObject> ProcessRoom(
        Vector2Int roomCenter,
        HashSet<Vector2Int> roomFloor,
        HashSet<Vector2Int> roomFloorNoCorridors)
    {
        List<GameObject> placedObjects = new List<GameObject>();

        //Determinar posición y Spawn
        Vector2Int playerSpawnPoint = roomCenter;

        // CONSTRUCCIÓN DETALLADA DE LA INSTANCIACIÓN:

        // a) Convertir de coordenadas de cuadrícula (Vector2Int) a coordenadas del mundo (Vector3).
        // La posición del centro de la celda de la cuadrícula es (X, Y).
        Vector3 worldPosition = new Vector3(
            playerSpawnPoint.x + 0.5f, // X de la celda + 0.5f (centro)
            playerSpawnPoint.y + 0.5f, // Y de la celda + 0.5f (centro)
            0f                         // Z=0 para 2D
        );
        // b) Invocar la función de instanciación del PrefabPlacer
        GameObject playerObject = prefabPlacer.CreateObject(
             player,
             worldPosition
        );
        // c) Agregar el objeto creado a la lista de objetos de la sala
        placedObjects.Add(playerObject);

        return placedObjects;

    }
}
