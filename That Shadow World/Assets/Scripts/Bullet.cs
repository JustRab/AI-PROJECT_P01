using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bullet collided with: " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Hit!");
            other.GetComponent<PlayerHealth>()?.TakeDamage(10);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Environment"))
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
