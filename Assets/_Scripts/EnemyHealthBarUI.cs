using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [SerializeField]
    private Slider healthSlider;
    private Transform mainCameraTransform;
    // Start is called before the first frame update
    void Start()
    {
        mainCameraTransform = Camera.main.transform;

        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }

        // Conexión inversa: encontrar al padre EnemyHealth y suscribirse
        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged.AddListener(UpdateHealth);

            UpdateHealth(enemyHealth.CurrentHealthValue, enemyHealth.MaxHealth);
        }
    }

    private void LateUpdate()
    {
        //Rotar la barra para que siempre mire a la cámara
        if (mainCameraTransform != null)
        {
            //Apunta hcia el vector opuesto a la dirección de la cámara
            transform.LookAt(transform.position + mainCameraTransform.forward);
        }
    }

    public void UpdateHealth(float currenteHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currenteHealth;

            //Ocultar la barra si la vida está a cero (el objeto se destruirá de todas formas)
            if (currenteHealth <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
