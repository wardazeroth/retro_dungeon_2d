using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphTest : MonoBehaviour
{
    Graph graph;

    //Indicador si el mapa de distancias está listo
    bool graphReady = false;

    //Diccionario que almacena el resultado: Posición -> Distancia al jugador (costo)
    Dictionary<Vector2Int, int> dijkstraResult;

    int highestValue;
    public IEnumerable<Vector2Int> FloorPositions { get; private set; } // O HashSet<Vector2Int>

    public void RunDjiskstraAlgorithm(Vector2Int playerPosition, IEnumerable<Vector2Int> floorPositions)
    {
        graphReady = false;

        this.FloorPositions = floorPositions;

        //Crear el Graph que contiene todas las baldosas transitables
        graph = new Graph(floorPositions);

        //Ejecutar el algoritmo BFS/Dijsktra y guardar el mapa de distancias
        dijkstraResult = DijkstraAlgorithm.Dijkstra(graph, playerPosition);

        //Obtener el valor más alto del mapa para la escala de color del debug
        highestValue = dijkstraResult.Values.Max();

        // establecer el esatdo como listo
        graphReady = true;
    }

    public Vector2Int GetDirectionToLowestCostNeighbour(Vector2Int currentPosition)
    {
        if (dijkstraResult == null || !graphReady) return currentPosition;

        Vector2Int bestNeighbour = currentPosition;
        int minDistance = int.MaxValue; //valor muy grande para empezar

        // Obtener los vecinos transitables de la posición actual
        foreach (var neighbour in graph.GetNeighbours4Directions(currentPosition))
        {
            //Consultar mapa de distancias precalculado
            if (dijkstraResult.ContainsKey(neighbour))
            {
                int distance = dijkstraResult[neighbour];
                //Elegir el vecino que tiene el costo (distancia) más bajo 
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestNeighbour = neighbour;
                }
            }
        }
        return bestNeighbour;
    }

    public int GetDijsktraCost(Vector2Int position)
    {
        if (graphReady && dijkstraResult != null && dijkstraResult.ContainsKey(position))
        {
            return dijkstraResult[position];
        }
        return highestValue + 1;
    }

    public int GetHighestDijkstraCost()
    {
        return highestValue;
    }
    
    //Debugging color mapa
    private void OnDrawGizmosSelected()
    {
        //Sólo dibujar si el calculo terminó
        if (graphReady && dijkstraResult != null)
        {
            foreach (var item in dijkstraResult)
            {
                //Mapear el valor de distancia a un color: Verde (cerca) -> Rojo (lejos)
                Color color = Color.Lerp(Color.green, Color.red, (float)item.Value / highestValue);
                color.a = 0.5f;
                Gizmos.color = color;

                //Dibujar un cubo en la posición de la baldosa (ajustamos 0.5 para centrar)
                Gizmos.DrawCube(item.Key + new Vector2(0.5f, 0.5f), Vector3.one);
            }       
        }
    }
}
