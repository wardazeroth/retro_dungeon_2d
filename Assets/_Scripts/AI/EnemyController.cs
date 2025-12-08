using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    // Hacemos la velocidad pública para configurarla fácilmente en el Inspector
    private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 currentMovementInput; // Almacenará el vector que viene de la IA

    void Awake()
    {
        // Usar Awake() para obtener referencias es más seguro que Start()
        rb = GetComponent<Rigidbody2D>();
    }

    // ✅ Este método es llamado por EnemyAI.cs a través del evento OnMovementInput
    public void OnMove(Vector2 movementVector)
    {
        // Solo almacenamos el vector de dirección; la ejecución del movimiento va en FixedUpdate
        currentMovementInput = movementVector;
    }

    private void FixedUpdate()
    {
        // 🛑 APLICACIÓN DE LA FÍSICA 🛑

        // Movemos el Rigidbody usando el input almacenado, asegurando que se haga en el ciclo de física
        if (rb != null && currentMovementInput.sqrMagnitude > 0)
        {
            Vector2 newPosition = rb.position + currentMovementInput * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }

    // NOTA: El método SetSpeed ya no es necesario si moveSpeed es [SerializeField]
}