using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool HasKey { get; private set; } = false;
    
    public void CollectKey()
    {
        HasKey = true;
        Debug.Log("LLave maestra recolectada! Busca la puerta de salida!");
    }
}
