using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ItemPlacementHelper
{
    //Diccionario para almacenar las baldosas clasificadas por su tipo de entorno
    Dictionary<PlacementType, HashSet<Vector2Int>> tileByType =
        new Dictionary<PlacementType, HashSet<Vector2Int>>();

    HashSet<Vector2Int> roomFloorNoCorridor;

    //CONSTRUCTOR: Clasifica el suelo
    public ItemPlacementHelper(HashSet<Vector2Int> roomFloor, HashSet<Vector2Int> roomFloorNoCorridor)
    {
        //Creamos el grafo usando TODO el suelo (incluiyendo pasillos) para buscar vecinos en general
        Graph graph = new Graph(roomFloor);
        this.roomFloorNoCorridor = roomFloorNoCorridor; 

        //Iteramos sobre el suelo de la sala excluyendo corredores (para el spawning)
        foreach (var position in roomFloorNoCorridor)
        {
            // Lógica de Clasificación:
            // -----------------------
            // Si tiene 8 vecinos de suelo (en 8 direcciones), está rodeado por suelo = OpenSpace.
            // Si tiene menos de 8, está cerca de un muro = NearWall.
            int neighboursCount8Dir = graph.GetNeighbours8Directions(position).Count;
            PlacementType type = neighboursCount8Dir < 8 ? PlacementType.NearWall : PlacementType.OpenSpace;

            //Inicializamos el HashSet si es la primera vez que encontramos de ese tipo
            if (!tileByType.ContainsKey(type))
                tileByType[type] = new HashSet<Vector2Int>();

            // Exclusión de Pasillos: 
            // Si la baldosa es NearWall, pero tiene 4 vecinos cardinales (lo que sucede en un pasillo 1x1), la ignoramos.
            // *Esta lógica es una protección para evitar que las baldosas de pasillo se clasifiquen como NearWall*
            if (type == PlacementType.NearWall && graph.GetNeighbours4Directions(position).Count == 4)
                continue;

            tileByType[type].Add(position);
        }
    }

    // METODO : Selecciona y reserva una posición de spawn
    public Vector2? GetItemPlacementPosition(PlacementType placementType, int iterationsMax, Vector2Int size, bool addOffset)
    {
        int itemArea = size.x * size.y;

        //Verificamos que haya espacio disponible para el tipo de colocación solicitado
        if (!tileByType.ContainsKey(placementType) || tileByType[placementType].Count < itemArea)
            return null;

        int iteration = 0;
        while (iteration < iterationsMax)
        {
            iteration++;

            //Seleccion aleatoria de un putno de inicio
            int index = UnityEngine.Random.Range(0, tileByType[placementType].Count);
            Vector2Int position = tileByType[placementType].ElementAt(index);

            // Si el objeto es grande (ej 2x2, 3x3), verificamos si cabe y reservamos el area
            if (itemArea > 1)
            {
                var (result, placementPositions) = PlaceBigItem(position, size, addOffset);

                if (result == false)
                    continue;
                // 3. RESERVA! Removemos las baldosas ocupadas de todas lasmlistas disponibles
                tileByType[placementType].ExceptWith(placementPositions);
                tileByType[PlacementType.NearWall].ExceptWith(placementPositions);
            }
            else //item 1x1, solo se remueve la baldosa seleccionada
                {
                    tileByType[placementType].Remove(position);
                }
                return position;
            }
            return null;
        }

        // METODO AUXILIAR: Verifica si un item grande cabe en el area sin superponerse a paredes o corredores
     private (bool, List<Vector2Int>) PlaceBigItem(Vector2Int originPosition, Vector2Int size, bool addOffset)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        //Ajustes para el tamaño del area a revisar (si el objeto tiene un offset extra)
        int maxX = addOffset ? size.x + 1 : size.x;
        int maxY = addOffset ? size.y + 1 : size.y;
        int minX = addOffset ? -1 : 0;
        int minY = addOffset ? -1 : 0;

        //Iteramos sobre todas las badosas que el objeto de tamaño X*Y ocuparía
        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                Vector2Int newPosToCheck = originPosition + new Vector2Int(x, y);
                // Si alguna de las baldosas necesarias NO es suelo válido, retornamos falso
                if (roomFloorNoCorridor.Contains(newPosToCheck) == false)
                    return (false, positions);

                positions.Add(newPosToCheck);
            }
        }
        return (true, positions);
    }
}

    public enum PlacementType
    {
        OpenSpace,
        NearWall
    }



