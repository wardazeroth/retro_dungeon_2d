using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicaMenu : MonoBehaviour
{
    public void EmpezarJuego()
    {
        SceneManager.LoadScene(1);
    }

    public void SalirJuego()
    {
        Debug.Log("Cerrando...");
        Application.Quit(); 
    }
}
