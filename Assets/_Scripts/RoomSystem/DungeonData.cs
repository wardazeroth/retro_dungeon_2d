using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonData
{
    public Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary;
    public HashSet<Vector2Int> floorPositions;
    public HashSet<Vector2Int> corridorPositions;
    public Dictionary<Vector2Int, Vector2Int> tileToRoomMap;

    public HashSet<Vector2Int> GetRoomFloorWithoutCorridors(Vector2Int dictionaryKey)
    {
        HashSet<Vector2Int> roomFloorNoCorridors = new HashSet<Vector2Int>(roomsDictionary[dictionaryKey]);
        roomFloorNoCorridors.ExceptWith(corridorPositions);
        return roomFloorNoCorridors;
    }

    public Vector2Int GetRoomIndexForTile(Vector2Int position)
    {
        if (tileToRoomMap.ContainsKey(position))
            {
            return tileToRoomMap[position];
            }
        //Devuelve Vector2int.zero o valor seguro si el tile no está en ninguna sala (ej. está en un  corredor o es una pared) 
        return Vector2Int.zero;
    }   
}
