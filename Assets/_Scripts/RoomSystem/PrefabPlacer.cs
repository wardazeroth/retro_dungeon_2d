using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabPlacer : MonoBehaviour


{
    //prefab necesario para metodo placeItem (ItemData solo tiene la data)
    [SerializeField]
    private GameObject itemPrefab; //Asignar el Prefab base que contiene el script Item.cs

    // 1. INSTANCIACIÓN BASE

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

        //2. SPAWN ENEMIGOS
        //Colocar enemigos basandose en la lista EnemyPlacemntdata
        public List<GameObject> PlaceEnemies(
            List<EnemyPlacementData> enemyPlacementData,
            ItemPlacementHelper itemPlacementHelper)
        {
        List<GameObject> placedObjects = new List<GameObject>();
        foreach (var placementData in enemyPlacementData)
            {
               for (int i= 0; i < placementData.Quantity; i++)
                {
                //Pide al ItemPlacementHelper un punto de spawn
                Vector2? possiblePlacementSpot = itemPlacementHelper.GetItemPlacementPosition(
                    PlacementType.OpenSpace, //tipo: espacio abierto
                    100, //Max. iteraciones
                    placementData.enemySize,
                    false // addOffset (generalmente falso para enemigos)
                    );
                    
                    if (possiblePlacementSpot.HasValue)
                    {
                        placedObjects.Add(CreateObject(placementData.enemyPrefab, possiblePlacementSpot.Value + new Vector2(0.5f, 0.5f))); //Instantiate(placementData.enemyPrefab,possiblePlacementSpot.Value + new Vector2(0.5f, 0.5f), Quaternion.identity)
                    }
                }
            }
            return placedObjects;
         }
    //3. LOGICA DE SPAWN DE ITEMS

    public List<GameObject> PlaceAllItems(
           List<ItemPlacementData> itemPlacementData,
           ItemPlacementHelper itemPlacementHelper)
    {
        List<GameObject> placedObjects = new List<GameObject>();
        //Ordenamios la lista para spawnear primero los items grandes. Esto asegura que 
        //Los espacios grandes se reserven primero y los items pequeños llenen los huecos.
        IEnumerable<ItemPlacementData> sortedList =
        itemPlacementData
        .OrderByDescending(placementData => placementData.itemData.size.x *
        placementData.itemData.size.y);

        foreach (var placementData in sortedList)
        {
            for (int i = 0; i < placementData.Quantity; i++)
            {
                //Pide la posicion usando el PlacementType (NearWall/Openspace)
                Vector2? possiblePlacementSpot = itemPlacementHelper.GetItemPlacementPosition(
                    placementData.itemData.placementType,
                    100,
                    placementData.itemData.size,
                    placementData.itemData.addOffset);

                if (possiblePlacementSpot.HasValue)
                {
                    // Crea el GameObject del ítem y lo inicializa con los datos
                    placedObjects.Add(PlaceItem(placementData.itemData, possiblePlacementSpot.Value));
                }
            }   
        }
        return placedObjects;
    }

    private GameObject PlaceItem(ItemData item, Vector2 placementPosition)
    {
        GameObject newItem = CreateObject(itemPrefab, placementPosition);
        //GameObject newItem = Instantiate(itemPrefab, placementPosition, Quaternion.identity);
        newItem.GetComponent<Item>().Initialize(item);
        return newItem;
    }

}
