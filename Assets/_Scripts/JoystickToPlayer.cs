using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

public class JoystickToPlayer : MonoBehaviour

{

    public PlayerMovement playerScript;
    private OnScreenStick stick;
    // Start is called before the first frame update
    void Start()
    {
        stick = GetComponent<OnScreenStick>();

        if (playerScript == null)
        {
            playerScript = GameObject.FindObjectOfType<PlayerMovement>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerScript == null)
        {
            playerScript = GameObject.FindObjectOfType<PlayerMovement>();
            if (playerScript == null) return;
        }
        //Calculamos la direccion, basandonos en cuanto se ha desplazado el stick
        Vector2 direction = stick.movementRange > 0
            ? ((RectTransform)transform).anchoredPosition / stick.movementRange
            : Vector2.zero;

        //Se pasa el dato a la funcion pública en el Player
        playerScript.SetMovementInput(direction);
    }
}
