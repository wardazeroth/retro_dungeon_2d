using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class Chest : MonoBehaviour
{
    //Referencia al objeto que contiene la llave unica
    private GameObject contentPrefab;

    //Estado para evitar reapertura
    private bool isOpen = false;

    [Header("Visuales")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Sprite openSprite;

    //Distancia máxima de interacción
    [SerializeField]
    private float interactionRange = 2f;

    //método llamado por RoomContentGenerator al spawnear el cofre único
    public void InitializeContent(GameObject content)
    {
        contentPrefab = content;
    }

    //Unity llama a este método cuando se hace click en el collider 2d
    private void OnMouseDown()
    {
        if (isOpen) return;
        //1. Verificar la distancia del jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= interactionRange)
        {
            OpenChest();
        }
    }
    
    private void OpenChest()
    {
        isOpen = true;

        //1. Cambiar la apariencia
        if (contentPrefab != null)
        {
            //Instancia la llave encima del cofre
            Instantiate(contentPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
            Debug.Log("Cofre abierto. La llave ha aprecido");
        }
        else
        {
            Debug.Log("Cofre abierto, estaba vacío");
        }
        //3. Desactivar el COllider 2D (para que no se pueda volver a clicar)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
