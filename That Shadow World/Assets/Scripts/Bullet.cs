using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(10);
            Debug.Log("Player Hit");
            Destroy(gameObject);
        }
        if(other.CompareTag("Environment"))
        {
            Destroy(gameObject);
        }

    }

    // Destroy after 2 seconds
    void Start()
    {
        Destroy(gameObject, 2f);
    }
}
