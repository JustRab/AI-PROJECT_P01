using System.Collections;
using UnityEngine;

public class FleeEnemy : Enemy
{
    [Header("Flee Enemy Settings")]
    public float acceleration = 10f; // Acelera rápido
    public float maxSpeed = 5f; // Velocidad baja
    public float fleeDuration = 3f; // Tiempo de huida
    public float restDuration = 2f; // Tiempo de descanso
    public float predictionFactor = 1f; // Factor de predicción del disparo
    public float fireRate = 1.5f; // Cada cuánto dispara
    public GameObject bulletPrefab;
    public Transform firePoint;
    public LayerMask obstacleLayer;
    public float avoidanceStrength = 5f; // Fuerza para evitar obstáculos

    private Rigidbody rb;
    private Transform player;
    private Rigidbody playerRb;
    private bool isFleeing = false;
    private bool isResting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody>();
        }

        StartCoroutine(FleeCycle());
        StartCoroutine(ShootAtPlayer());
    }

    IEnumerator FleeCycle()
    {
        while (true)
        {
            isFleeing = true;
            isResting = false;
            yield return new WaitForSeconds(fleeDuration);

            isFleeing = false;
            isResting = true;
            rb.linearVelocity = Vector3.zero; // Se detiene completamente
            yield return new WaitForSeconds(restDuration);
        }
    }

    IEnumerator ShootAtPlayer()
    {
        while (true)
        {
            if (player != null)
            {
                yield return new WaitForSeconds(fireRate);
                PredictiveShoot();
            }
            yield return null;
        }
    }

    void FixedUpdate()
    {
        if (isFleeing && player != null)
        {
            Vector3 fleeDirection = (transform.position - player.position).normalized;
            Vector3 desiredVelocity = fleeDirection * maxSpeed;
            Vector3 steering = desiredVelocity - rb.linearVelocity;
            steering = Vector3.ClampMagnitude(steering, acceleration);
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity + steering * Time.fixedDeltaTime, maxSpeed);

            // Obstacle Avoidance
            RaycastHit hit;
            if (Physics.Raycast(transform.position, rb.linearVelocity.normalized, out hit, 2f, obstacleLayer))
            {
                Vector3 avoidanceDirection = Vector3.Reflect(rb.linearVelocity.normalized, hit.normal);
                rb.linearVelocity += avoidanceDirection * avoidanceStrength * Time.fixedDeltaTime;
            }

            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
        }
        else if (isResting)
        {
            animator.SetFloat("Speed", 0f);
        }

        // Rotar en la dirección del movimiento
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rb.linearVelocity), 0.1f);
        }
        if (rb.linearVelocity.magnitude < 0.1f) // If it's stuck
        {
            rb.AddForce(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }
    }

    void PredictiveShoot()
    {
        if (player == null || bulletPrefab == null || firePoint == null) return;

        float playerSpeed = playerRb != null ? playerRb.linearVelocity.magnitude : 0f;
        float predictionTime = (maxSpeed + playerSpeed) > 0 ? Vector3.Distance(transform.position, player.position) / (maxSpeed + playerSpeed) : 0f;
        predictionTime *= predictionFactor;

        Vector3 predictedPosition = player.position + (playerRb != null ? playerRb.linearVelocity * predictionTime : Vector3.zero);
        Vector3 directionToTarget = (predictedPosition - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(directionToTarget));
        bullet.GetComponent<Rigidbody>().linearVelocity = directionToTarget * 15f;
    }
}
