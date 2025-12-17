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

    private Vector2 lastMoveDirection = Vector2.down;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        //Obtener la referencia al Rigidbody2D
        rb = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator no encontrado en el Player");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer no encontrado en el Player");
        }

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

        if (movementInput.magnitude > 0)
        {
            Debug.Log($"Input Detectado: {movementInput}");
        }
        //Debug.Log($"Posición Rigidbody: {rb.position}");

        //Lógica de animación y flipX
        animator?.SetFloat("MoveX", movementInput.x);
        animator?.SetFloat("MoveY", movementInput.y);

        //Control de Bool para Idle/Run
        bool isMoving = movementInput.magnitude > 0.05f;
        animator?.SetBool("isMoving", isMoving);

        //L+ogica para guardar la última dirección
        if (isMoving)
        {
            //Si nos movemos, actalizamos la última dirección y la enviamos al Blend Tree
            lastMoveDirection = movementInput;
            animator.SetFloat("MoveX", movementInput.x);
            animator.SetFloat("MoveY", movementInput.y);
        }
        else
        {
            //Si no nos movemos, enviamos la última dirección guardada
            animator.SetFloat("MoveX", lastMoveDirection.x);
            animator.SetFloat("MoveY", lastMoveDirection.y);
        }

        //Lógica de Volteo
        if (movementInput.x > 0.05f)
        {
            spriteRenderer.flipX = false;
        }
        else if (movementInput.x < -0.05f)
        {
            spriteRenderer.flipX = true;
        }

        // Lógica de ataque
        if (Input.GetButtonDown("Fire1"))
        {
            //Forzar que los valores de MoveX/MoveY al Blend Tree sean la última dirección
            //(Vital para que el AttackBlend sepa qué clip de ataque ejecutar)
            animator.SetFloat("MoveX", lastMoveDirection.x);
            animator.SetFloat("MoveY", lastMoveDirection.y);

            //Activamos el trigger para ir del MovementBlend al AttackBlend
            animator.SetTrigger("AttackTrigger");
        }
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
