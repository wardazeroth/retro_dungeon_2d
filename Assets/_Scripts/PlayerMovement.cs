using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    //Componentes
    private Rigidbody2D rb;
    private Vector2 movementInput;

    private void Start()
    {
        //Obtener la referencia al Rigidbody2D
        rb = GetComponent<Rigidbody2D>();

        if (rb == null )
        {
            Debug.LogError("Rigidbody no encontrado");
        }

    }

    // Update is called once per frame
    void Update()
    {
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        movementInput.Normalize();

        // 🌟 AÑADE ESTO TEMPORALMENTE 🌟
        if (movementInput.magnitude > 0)
        {
            Debug.Log($"Input Detectado: {movementInput}");
        }
        //Debug.Log($"Posición Rigidbody: {rb.position}");
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // Aplicamos la velocidad directamente. 
            // El Rigidbody es Dynamic, por lo que las colisiones lo detendrán.
            Vector2 velocity = movementInput * moveSpeed;

            rb.velocity = velocity; // 🌟 ¡USAR VELOCITY EN LUGAR DE MOVEPOSITION!
        }
    }
}
