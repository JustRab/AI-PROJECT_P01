using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemy : Enemy
{
    [Header("Ajustes de Visión")]
    [Tooltip("Ángulo total del cono de visión")]
    public float viewAngle = 90f;
    [Tooltip("Distancia máxima de detección")]
    public float viewDistance = 10f;

    [Header("Ajustes de Rotación")]
    [Tooltip("Grados que rota el cono cada intervalo")]
    public float rotationStep = 45f; // Grados a rotar en cada paso
    [Tooltip("Intervalo (en segundos) entre cada rotación")]
    public float rotationInterval = 2f;

    [Header("Ajustes de Ataque")]
    [Tooltip("Tiempo que tarda en dejar de disparar luego de perder al jugador")]
    public float timeToStopShootingAfterLost = 2f;
    [Tooltip("Prefab de la bala o proyectil")]
    public GameObject projectilePrefab;
    [Tooltip("Punto de disparo (posición desde donde se instancia la bala)")]
    public Transform firePoint;
    [Tooltip("Velocidad del proyectil")]
    public float projectileSpeed = 10f;

    [Header("Visualización del Cono de Visión")]
    [Tooltip("MeshFilter donde se mostrará el cono de visión en juego")]
    public MeshFilter viewMeshFilter;
    [Tooltip("MeshRenderer asociado al cono de visión")]
    public MeshRenderer viewMeshRenderer;
    [Tooltip("Resolución (cantidad de segmentos) del cono de visión")]
    public int meshResolution = 10;

    // Variables internas
    private Mesh viewMesh;
    private Transform player;
    private bool playerDetected = false;
    private float lostDetectionTimer = 0f;
    private float nextRotationTime = 0f;
    private float nextAttackTime = 0f;

    void Start()
    {
        base.Start(); // Inicializa la clase base Enemy

        // Busca al jugador (se asume que tiene la etiqueta "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Inicializa la malla para el cono de visión
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        if (viewMeshFilter != null)
        {
            viewMeshFilter.mesh = viewMesh;
        }
    }

    void Update()
    {
        // Si se encontró al jugador, comprobamos si está en el cono de visión
        if (player != null)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

            bool isInView = (distanceToPlayer <= viewDistance) && (angleToPlayer <= viewAngle / 2f);

            if (isInView)
            {
                // Jugador detectado: reiniciamos el contador de pérdida y activamos el modo de ataque
                playerDetected = true;
                lostDetectionTimer = 0f;
            }
            else if (playerDetected)
            {
                // Si ya estaba en modo ataque, contamos el tiempo sin verlo
                lostDetectionTimer += Time.deltaTime;
                if (lostDetectionTimer >= timeToStopShootingAfterLost)
                {
                    playerDetected = false;
                }
            }
            
        }

        // Si el jugador está detectado, se detiene la rotación y se dispara
        if (playerDetected)
        {
            if (Time.time >= nextAttackTime)
            {
                ShootAtPlayer();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            // En modo patrulla: rota cada cierto intervalo
            if (Time.time >= nextRotationTime)
            {
                RotateVision();
                nextRotationTime = Time.time + rotationInterval;
            }
        }

        // Actualizamos la visualización del cono de visión
        DrawFieldOfView();
    }

    /// <summary>
    /// Rota el objeto (y por lo tanto su cono de visión) en el eje Y.
    /// </summary>
    void RotateVision()
    {
        transform.Rotate(0f, rotationStep, 0f);
    }

    /// <summary>
    /// Instancia un proyectil que se dirige hacia la posición actual del jugador.
    /// </summary>
    void ShootAtPlayer()
    {
        if (projectilePrefab != null && firePoint != null && player != null)
        {
            // Instancia la bala y calcula la dirección desde el firePoint hacia el jugador
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Vector3 direction = (player.position - firePoint.position).normalized;
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed;
            }
            // Aquí puedes agregar efectos, animaciones, etc.
        }
    }

    /// <summary>
    /// Dibuja y actualiza la malla que representa el cono de visión para mostrarla en juego.
    /// </summary>
    void DrawFieldOfView()
    {
        if (viewMesh == null)
            return;

        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        // Se recorren los ángulos del cono desde -viewAngle/2 hasta +viewAngle/2
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -viewAngle / 2f + stepAngleSize * i;
            // Usamos "false" para que se sume la rotación actual del enemigo y el cono se oriente correctamente
            Vector3 dir = DirFromAngle(angle, false);
            Vector3 point = transform.position + dir * viewDistance;
            // Convertimos el punto a espacio local (relativo al enemigo)
            viewPoints.Add(transform.InverseTransformPoint(point));
        }

        // Creamos los vértices y triángulos para la malla
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; // Origen en la posición del enemigo (espacio local)

        for (int i = 0; i < viewPoints.Count; i++)
        {
            vertices[i + 1] = viewPoints[i];
        }

        for (int i = 0; i < vertexCount - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    /// <summary>
    /// Devuelve una dirección a partir de un ángulo (en grados).
    /// Si isGlobal es false, el ángulo se suma a la rotación actual del enemigo.
    /// </summary>
    /// <param name="angleInDegrees">Ángulo en grados.</param>
    /// <param name="isGlobal">Determina si el ángulo es global o relativo al objeto.</param>
    /// <returns>Vector3 con la dirección calculada.</returns>
    public Vector3 DirFromAngle(float angleInDegrees, bool isGlobal)
    {
        if (!isGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        float rad = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
    }
}
