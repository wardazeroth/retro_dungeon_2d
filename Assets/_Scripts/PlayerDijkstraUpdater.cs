using UnityEngine;
using System.Linq;

public class PlayerDijkstraUpdater : MonoBehaviour
{
    // Asigna el objeto RoomContentGenerator/GraphTest en el Inspector.
    // Opcional: Si GraphTest está en un objeto fijo, puedes usar FindObjectOfType.
    [SerializeField]
    private GraphTest graphTest;

    // Velocidad de actualización de Dijkstra (más lento que 0.06s)
    [SerializeField]
    private float dijkstraUpdateDelay = 0.2f; // Actualizar 5 veces por segundo

    void Start()
    {
        // 🛑 CRÍTICO: BUSCAR EL GRAPHTEST SI NO ESTÁ ASIGNADO 🛑
        if (graphTest == null)
        {
            graphTest = FindObjectOfType<GraphTest>();
        }

        // 🛑 INICIAR EL RECALCULO CENTRALIZADO Y REPETITIVO 🛑
        if (graphTest != null)
        {
            InvokeRepeating("RecalculateDijkstra", 0.0f, dijkstraUpdateDelay);
        }
        else
        {
            Debug.LogError("[DIJKSTRA UPDATER] No se encontró GraphTest en la escena. La IA funcionará mal.");
        }
    }

    private void RecalculateDijkstra()
    {
        if (graphTest != null && graphTest.FloorPositions != null)
        {
            Vector2Int playerGridPosition = Vector2Int.FloorToInt(transform.position);

            // 🛑 ÚNICO LUGAR DONDE SE EJECUTA EL CALCULO PESADO 🛑
            graphTest.RunDjiskstraAlgorithm(playerGridPosition, graphTest.FloorPositions);

            // Debug.Log($"[DIJKSTRA CENTRAL] Recálculo ejecutado en posición: {playerGridPosition}");
        }
    }
}