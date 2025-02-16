using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damageAmount = 10;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bullet collided with: " + other.gameObject.name);

        // If the bullet hits the player, apply damage and destroy the bullet.
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Hit!");
            other.GetComponent<PlayerHealth>()?.TakeDamage(damageAmount);
            Destroy(gameObject);
            return;
        }
        
        // If the bullet hits an object tagged as Environment, destroy it.
        else if (other.CompareTag("Obstacles"))
        {
            Debug.Log("Bullet hit the environment!");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Destroy(gameObject, 2f); // Bullet destroys itself after 2 seconds
    }
}
