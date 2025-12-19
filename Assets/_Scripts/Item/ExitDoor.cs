using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    [SerializeField]  private GameObject objetoAviso;
    private bool mostrandoAviso = false;

    void Start()
    {
        GameObject[] todosLosObjetos = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in todosLosObjetos)
        {
            if (obj.name == "Contenedor_Aviso")
            {
                objetoAviso = obj;
                break;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        
        if (inventory != null && inventory.HasKey)
        {
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                gm.EndGameVictory();
            }
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("La puesrta de salida está bloqueada. Necesitas una llave para escapar");
            
            if (!mostrandoAviso)
            {
                StartCoroutine(MostrarAvisoTemporizado());
            }
        }
    }

    IEnumerator MostrarAvisoTemporizado()
    {
        mostrandoAviso = true;

        if (objetoAviso != null)
        {
            objetoAviso.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            objetoAviso.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No se encotró el objeto 'COntenedor_aviso'");
        }

        mostrandoAviso = false;
    }

}
