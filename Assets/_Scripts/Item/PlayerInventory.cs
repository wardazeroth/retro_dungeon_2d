using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool HasKey { get; private set; } = false;
    private GameObject keyHUDIcon;

    void Start()
    {
        // Usamos una Corrutina para esperar a que todo el mapa procedural se genere
        StartCoroutine(FindHUDDelayed());
    }

    IEnumerator FindHUDDelayed()
    {
        // Esperamos 0.2 segundos para que el Canvas y el mapa estén listos
        yield return new WaitForSeconds(0.2f);

        // Buscamos el objeto por nombre exacto
        keyHUDIcon = GameObject.Find("Key_Icon");

        if (keyHUDIcon != null)
        {
            keyHUDIcon.SetActive(false); // Lo ocultamos al empezar
            Debug.Log("HUD: Icono encontrado y ocultado.");
        }
        else
        {
            // Si sale este error, revisa que en el Canvas se llame "Key_Icon"
            Debug.LogWarning("HUD: No se encontró 'Key_Icon' al iniciar.");
        }
    }

    public void CollectKey()
    {
        HasKey = true;
        Debug.Log("¡Llave maestra recolectada! Busca la puerta de salida!");

        // Por seguridad, si el Start falló, lo buscamos de nuevo aquí
        if (keyHUDIcon == null)
        {
            keyHUDIcon = GameObject.Find("Key_Icon");
        }

        if (keyHUDIcon != null)
        {
            keyHUDIcon.SetActive(true); // ¡Aquí se muestra en el HUD!
        }
    }
}