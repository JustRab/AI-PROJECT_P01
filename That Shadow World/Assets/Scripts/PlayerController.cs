using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 movement;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;

    private Animator animator;
    private bool isAttacking = false;

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
    }

    void FixedUpdate()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            transform.position += movement * moveSpeed * Time.deltaTime;
        }
        // Only update rotation when moving
        if (movement.magnitude > 0)
        {
            transform.forward = movement;
        }    
    }

    public void TryToShoot()
    {
        // Check if the "Attack" animation is currently playing
        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            StartCoroutine(ShootWithDelay(0.75f)); // Adjust the delay to match animation timing
        }
    }

    IEnumerator ShootWithDelay(float delay)
    {
        movement = Vector3.zero;
        animator.SetTrigger("Attack01");
        isAttacking = true;

        yield return new WaitForSeconds(delay); // Wait before shooting

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * bulletSpeed;
    }
}
