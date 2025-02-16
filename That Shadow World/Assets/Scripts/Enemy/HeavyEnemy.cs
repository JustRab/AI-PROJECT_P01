using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyEnemy : Enemy
{
    [Header("Heavy Enemy Settings")]
    [Tooltip("Aceleración baja para simular peso")]
    public float acceleration = 2f;
    [Tooltip("Velocidad máxima alta para alcanzar al jugador")]
    public float maxSpeed = 10f;
    [Tooltip("Factor de predicción para calcular la posición futura del jugador")]
    public float predictionFactor = 1f;

    private Rigidbody rb;
    private Transform player;
    private Rigidbody playerRb;
    private bool chasing = false;

    void Start()
    {
        base.Start(); // Inicializa la clase base Enemy (configura salud, daño, etc.)
        rb = GetComponent<Rigidbody>();

        // Buscamos al jugador por su etiqueta "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        // Si el jugador ya ha sido detectado y sigue vivo...
        if (chasing && player != null)
        {
            // --- STEERING BEHAVIOR: PURSUIT ---

            // Calculamos la distancia al jugador
            float distance = Vector3.Distance(transform.position, player.position);

            // Obtenemos la velocidad del jugador (si tiene Rigidbody)
            float playerSpeed = (playerRb != null) ? playerRb.linearVelocity.magnitude : 0f;

            // Tiempo de predicción: cuanto más lejos esté el jugador, mayor será el tiempo
            float predictionTime = (maxSpeed + playerSpeed) > 0 ? distance / (maxSpeed + playerSpeed) : 0f;
            predictionTime *= predictionFactor; // Ajustamos con un factor (si es necesario)

            // Calculamos la posición futura del jugador
            Vector3 predictedPos = player.position;
            if (playerRb != null)
            {
                predictedPos += playerRb.linearVelocity * predictionTime;
            }

            // Velocidad deseada: dirección al jugador (predicho) * velocidad máxima
            Vector3 desiredVelocity = (predictedPos - transform.position).normalized * maxSpeed;

            // Fuerza de steering: diferencia entre la velocidad deseada y la actual
            Vector3 steering = desiredVelocity - rb.linearVelocity;
            // Limitamos la magnitud del steering a la aceleración máxima permitida
            steering = Vector3.ClampMagnitude(steering, acceleration);

            // Actualizamos la velocidad (integrando la aceleración)
            Vector3 newVelocity = rb.linearVelocity + steering * Time.fixedDeltaTime;
            // Limitamos la velocidad a maxSpeed
            newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
            rb.linearVelocity = newVelocity;

            // Rotamos el enemigo para que mire en la dirección en la que se mueve
            if (newVelocity.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(newVelocity);
                // Suavizamos la rotación para que no gire bruscamente
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
            }
        }
        
        // Actualiza el parámetro "Speed" en el Animator según la velocidad actual para transicionar entre Idle, Walk y Run.
        // Usamos un dampTime para lograr una transición suave.
        float currentSpeed = rb.linearVelocity.magnitude;
        animator.SetFloat("Speed", currentSpeed, 0.1f, Time.fixedDeltaTime);
    }

    // Detecta cuando el jugador entra al área (o "cuarto") del enemigo.
    // Se asume que el enemigo (o un objeto hijo) tiene un Collider marcado como "Is Trigger".
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            chasing = true;
            Debug.Log("Player detected!");
        }
    }

    // Controla las colisiones físicas
    private void OnCollisionEnter(Collision collision)
    {
        // Si choca con una pared, la velocidad se reinicia a 0
        if (collision.gameObject.CompareTag("Obstacles"))
        {
            rb.linearVelocity = Vector3.zero;
        }

        // Si choca con el jugador, se le causa daño multiplicado por la velocidad de impacto
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                animator.SetTrigger("Attack"); // Animación de ataque
                // El daño aumenta según la magnitud de la velocidad en el momento del choque
                float impactDamage = damage * rb.linearVelocity.magnitude;
                playerHealth.TakeDamage(impactDamage);
            }
        }
    }
}
