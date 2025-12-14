using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight= 20;
    [SerializeField]
    [Range(0, 10)]
    private int offset = 1;
    [SerializeField]
    private bool randomWalkRooms = false;

    [Header("Spawning de Entidades")]
    [SerializeField]
    RoomContentGenerator roomContentGenerator;

    //private DungeonData dungeonData = new DungeonData();

    private DungeonData dungeonData;
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

    //    // Método estándar para iniciar la lógica cuando se pulsa Play en Unity
    //    void Start()
    //    {
    //        // Verifica que el visualizador esté listo antes de intentar pintar
    //        if (tilemapVisualizer != null)
    //        {
    //            RunProceduralGeneration();
    //        }
    //        else
    //        {
    //            Debug.LogError("tilemapVisualizer es NULL. No se puede iniciar la generación.");
    //        }
    //}

    void Start() // Se ejecuta una vez al cargar la escena
    {
        // 🛑 1. Suscribir la generación al evento 🛑
        // Esto permite que el RoomContentGenerator (al presionar Espacio) llame a RunProceduralGeneration.
        if (roomContentGenerator != null)
        {
            roomContentGenerator.RegenerateDungeon.AddListener(RunProceduralGeneration);
        }

        RunProceduralGeneration();

        // 3. Desactivar el flag para el futuro
        GameManager.ShouldGenerateNewDungeon = false;
    }


    protected override void RunProceduralGeneration()

    {
        //ClearPreviousGeneration();
        // Esto asegura que la secuencia de números aleatorios sea diferente en cada ejecución.
        if (roomContentGenerator != null)
        {
            // Forzamos la destrucción de los objetos ANTES de dibujar nada nuevo.
            roomContentGenerator.DestroyAllSpawnedObjects();
        }

        // 🛑 2. LIMPIEZA DE TILEMAPS 🛑

        tilemapVisualizer.Clear();


        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        dungeonData = new DungeonData();
        roomsDictionary.Clear();
        CreateRooms();

        tilemapVisualizer.PaintFloorTiles(dungeonData.floorPositions);
        WallGenerator.CreateWalls(dungeonData.floorPositions, tilemapVisualizer);

        // 4. SPAWN E INYECCIÓN DE IA
        if (roomContentGenerator != null)
        {
            // Spawnea y calcula Dijkstra inicial, y la IA se activa.
            roomContentGenerator.GenerateRoomContent(dungeonData);
            roomContentGenerator.ForceAIGraphInjection();
        }
    }

    private HashSet<Vector2Int> IncreaseCorridorBrush2y2(HashSet<Vector2Int> corridor)
    {
        HashSet<Vector2Int> newCorridor = new HashSet<Vector2Int>();

        // Offsets para una brocha 3x3
        List<Vector2Int> offsets = new List<Vector2Int>
    {
        new Vector2Int(0, 0), // La baldosa original
        new Vector2Int(1, 0), // La baldosa a la derecha
        new Vector2Int(0, 1), // La baldosa de arriba
        new Vector2Int(1, 1), // La baldosa diagonal (arriba-derecha)
    };

        foreach (var position in corridor)
        {
            foreach (var offset in offsets)
            {
                newCorridor.Add(position + offset);
            }
        }
        return newCorridor;
    }

    private void CreateRooms()
    {

        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int
            (dungeonWidth, dungeonHeight)), minRoomWidth, minRoomHeight);


        //LLenar el diccionario de salas y obtener los centros
        foreach (var roomBounds in roomsList)
        {
            HashSet<Vector2Int> currentRoomFloor;
            Vector2Int roomCenter = (Vector2Int)Vector3Int.RoundToInt(roomBounds.center);

            if (randomWalkRooms)
            {
                currentRoomFloor = CreateSingleRandomRoom(roomBounds);
                //floor = CreateRoomsRandomly(roomsList);
            }
            else
            {
                currentRoomFloor = CreateSingleSimpleRoom(roomBounds);
                //floor = CreateSimpleRooms(roomsList);
            }

            if (currentRoomFloor.Count > 0)
            {
                roomsDictionary.Add(roomCenter, currentRoomFloor);
            }

        }

        HashSet<Vector2Int> floor = System.Linq.Enumerable.ToHashSet(roomsDictionary.Values.SelectMany(x => x));
        List<Vector2Int> roomCenters = roomsDictionary.Keys.ToList();

        HashSet<Vector2Int> initialCorridors = ConnectRooms(roomCenters);
        HashSet<Vector2Int> wideCorridors = IncreaseCorridorBrush2y2(initialCorridors);

        floor.UnionWith(wideCorridors);

        dungeonData.tileToRoomMap = new Dictionary<Vector2Int, Vector2Int>();

        foreach (var roomEntry in roomsDictionary)
        {
            Vector2Int roomIndex = roomEntry.Key;
            HashSet<Vector2Int> roomTiles = roomEntry.Value;

            foreach (Vector2Int tilePosition in roomTiles)
            {
                //Mapear la posición de la baldosa a la clave de su sala (roomCenter)
                dungeonData.tileToRoomMap.Add(tilePosition, roomIndex);
            }
        }

        foreach (Vector2Int corridorPosition in wideCorridors)
        {
            if (!dungeonData.tileToRoomMap.ContainsKey(corridorPosition))
            {
                // dungeonData.tileToRoomMap.Add(corridorPosition, Vector2Int.zero);
            }
        }

        // LLenar el objeto DungeonData
        dungeonData.roomsDictionary = roomsDictionary;
        dungeonData.floorPositions = floor;
        dungeonData.corridorPositions = wideCorridors;

        //tilemapVisualizer.PaintFloorTiles(floor);
        //WallGenerator.CreateWalls(floor, tilemapVisualizer);

        ////Invocar controlador de spawn
        //if (roomContentGenerator != null)
        //{
        //    // Pasamos el objeto DungeonData COMPLETO al spawner.
        //    roomContentGenerator.GenerateRoomContent(dungeonData);
        //}
    }

    private HashSet<Vector2Int> CreateSingleRandomRoom(BoundsInt roomBounds)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
        var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);

        // Aplicar la restricción de los límites (offset) de la sala
        foreach (var position in roomFloor)
        {
            if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) &&
                position.y >= (roomBounds.yMin + offset) && position.y <= (roomBounds.yMax - offset))
            {
                floor.Add(position);
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)
        {
            if(destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if(destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
        }
        while (position.x != destination.x)
        {
            if(destination.x > position.x)
            {
                position += Vector2Int.right;
            }
            else if(destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
        }
        return corridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if(currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        } 
        return closest;
    }

    private HashSet<Vector2Int> CreateSingleSimpleRoom(BoundsInt roomBounds)
    {
        HashSet<Vector2Int> roomFloor = new HashSet<Vector2Int>();
        for (int col = offset; col < roomBounds.size.x - offset; col++)
        {
            for (int row = offset; row < roomBounds.size.y - offset; row++)
            {
                Vector2Int position = (Vector2Int)roomBounds.min + new Vector2Int(col, row);
                roomFloor.Add(position);
            }
        }
        return roomFloor;
    }

}
