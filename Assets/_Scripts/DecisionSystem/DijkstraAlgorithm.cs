using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstraAlgorithm
{
    public static Dictionary<Vector2Int, int> Dijkstra(Graph graph, Vector2Int startposition)
    {
        Queue<Vector2Int> unfinishedVertices = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distanceDictionary = new Dictionary<Vector2Int, int>();

        Dictionary<Vector2Int, Vector2Int> parentDictionary = new Dictionary<Vector2Int, Vector2Int>();

        distanceDictionary[startposition] = 0;
        parentDictionary[startposition] = startposition;

        //Añadir los vecinos del punto de inicio a la cola para empézar la exploración
        foreach (Vector2Int vertex in graph.GetNeighbours4Directions(startposition))
        {
            unfinishedVertices.Enqueue(vertex);
            parentDictionary[vertex] = startposition;
        }

        //Bucle de exploración
        while(unfinishedVertices.Count > 0)
        {
            Vector2Int vertex = unfinishedVertices.Dequeue();

            // La distancia actual al nodo es la distancia de su padre más 1 (costo del paso).
            int newDistance = distanceDictionary[parentDictionary[vertex]] + 1;

            // Si ya encontramos una ruta más corta o igual a este vértice, lo ignoramos.
            if (distanceDictionary.ContainsKey(vertex) && distanceDictionary[vertex] <= newDistance)
                continue;
            //Si es una ruta más corta, actualizamos la distancia
            distanceDictionary[vertex] = newDistance;

            //Explñorar los vecinos de este vértice
            foreach (Vector2Int neighbour in graph.GetNeighbours4Directions(vertex))
            {
                //Sie el vecino aún no tiene una distancia calculada, lo añadimos a la cola
                if (!distanceDictionary.ContainsKey(neighbour))
                {
                    unfinishedVertices.Enqueue(neighbour);
                    parentDictionary[neighbour] = vertex;
                }
            }
        }
        return distanceDictionary;
    }
}
