using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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
        Time.timeScale = 1f;
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
