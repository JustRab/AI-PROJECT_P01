using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 movement;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        movement = new Vector3(moveX, 0, moveZ).normalized;

        animator.SetFloat("Speed", movement.magnitude);

        if (Input.GetButtonDown("Fire1"))
        {
            TryToShoot();
        }
        //If player presses R key, the scene will be reloaded
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene(); 
            SceneManager.LoadScene(scene.name);
        }
    }

void FixedUpdate()
{
    Rigidbody rb = GetComponent<Rigidbody>();
    if(rb != null && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Hurt"))
    {
        Vector3 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    // Solo actualizamos la rotación cuando hay movimiento
    if (movement.magnitude > 0)
    {
        Quaternion targetRotation = Quaternion.LookRotation(movement);
        rb.MoveRotation(targetRotation);
    }    
}

    public void TryToShoot()
    {
        // Check if the "Attack" animation is currently playing
        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            StartCoroutine(ShootWithDelay(0.365f)); // Adjust the delay to match animation timing
        }
    }

    IEnumerator ShootWithDelay(float delay)
    {
        movement = Vector3.zero;
        animator.SetTrigger("Attack01");

        yield return new WaitForSeconds(delay); // Wait before shooting

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * bulletSpeed;
    }

}
