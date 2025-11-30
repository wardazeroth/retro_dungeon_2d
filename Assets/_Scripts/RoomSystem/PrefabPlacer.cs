using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabPlacer : MonoBehaviour
{
    public GameObject CreateObject(GameObject prefab, Vector3 placementPosition)
    {
        if (prefab == null)
            return null;

        GameObject newItem;

        if (Application.isPlaying)
        {
            newItem = Instantiate(prefab, placementPosition, Quaternion.identity);

        }
        else
        {
            newItem = Instantiate(prefab, placementPosition, Quaternion.identity);

        }
        return newItem;

    }
}
