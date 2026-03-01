using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    //Componentes
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 lastMoveDirection = Vector2.down;
    private Vector2 keyboardInput;
    private Vector2 joystickInput;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public UnityEngine.UI.Image fondoEspadaImage;

    public void SetMovementInput(Vector2 input)
    {
        joystickInput = input;
    }

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

        // RESETEAR TODO AL NACER
        movementInput = Vector2.zero;
        joystickInput = Vector2.zero;
        keyboardInput = Vector2.zero;
        if (rb != null) rb.velocity = Vector2.zero;

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
        keyboardInput.x= Input.GetAxisRaw("Horizontal");
        keyboardInput.y = Input.GetAxisRaw("Vertical");

        movementInput = keyboardInput + joystickInput;

        if (movementInput.magnitude > 0.05f)
        {
            movementInput.Normalize();
            lastMoveDirection = movementInput;
            Debug.Log($"Input Detectado: {movementInput}");
        }
        else
        {
            movementInput = Vector2.zero;
        }
            //Debug.Log($"Posición Rigidbody: {rb.position}");

            //Lógica de animación y flipX
        animator?.SetFloat("MoveX", movementInput.magnitude > 0.05f ? movementInput.x : lastMoveDirection.x);
        animator?.SetFloat("MoveY", movementInput.magnitude > 0.05f ? movementInput.y : lastMoveDirection.y);
        animator?.SetBool("isMoving", movementInput.magnitude > 0.05f);

        //Lógica de Volteo
        if (movementInput.x > 0.05f)
        {
            spriteRenderer.flipX = false;
        }
        else if (movementInput.x < -0.05f)
        {
            spriteRenderer.flipX = true;
        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                // En táctil, SOLO atacamos si toca la espada
                if (EsClicEnEspada())
                {
                    RealizarAtaque();
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            RealizarAtaque();
        }

        // Lógica de ataque
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            RealizarAtaque();
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // Aplicamos la velocidad directamente. 
            // El Rigidbody es Dynamic, por lo que las colisiones lo detendrán.
            Vector2 velocity = movementInput * moveSpeed;

            rb.velocity = velocity;
        }
    }

    public void RealizarAtaque()
    {
        Debug.Log("¡CONEXIÓN EXITOSA!");

        if (animator != null)
        {
            // Feedback visual manual (reemplaza al componente Button)

            animator.ResetTrigger("AttackTrigger");
            animator.SetTrigger("AttackTrigger");

            if (fondoEspadaImage != null) StartCoroutine(FlashRojo());
            Debug.Log("¡Ataque detectado por Event Trigger!");
        }
    }

    IEnumerator FlashRojo()
    {
        fondoEspadaImage.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        fondoEspadaImage.color = Color.white;
    }

    bool EsClicEnEspada()
{
    PointerEventData eventData = new PointerEventData(EventSystem.current);
    eventData.position = Input.mousePosition;
    List<RaycastResult> results = new List<RaycastResult>();
    EventSystem.current.RaycastAll(eventData, results);

    foreach (var result in results)
    {
        if (result.gameObject.name == "FondoEspada") return true;
    }
    return false;
    }

}
