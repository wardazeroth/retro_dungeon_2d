using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool ShouldGenerateNewDungeon = false;
    private void Awake()
    {
        // Garantizar que la escala de tiempo sea 1 al inicio de la escena.
        Time.timeScale = 1f;
    }

    [SerializeField] private GameObject winPanel;
    public void EndGameVictory()
    {
        Debug.Log("¡El Juego ha terminado!");
        Time.timeScale= 0f;

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        GameManager.ShouldGenerateNewDungeon = true;

        RoomContentGenerator contentGenerator = FindObjectOfType<RoomContentGenerator>();
        if (contentGenerator != null)
        {
            contentGenerator.DestroyAllSpawnedObjects();
        }

        Time.timeScale = 1f;
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
