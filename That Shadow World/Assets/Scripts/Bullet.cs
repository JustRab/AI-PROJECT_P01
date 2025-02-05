using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Wall"))
        {
            Destroy(gameObject); // I can use Object Pooling here too if needed - ILG
        }
    }

    // Destroy after 2 seconds
    void Start()
    {
        Destroy(gameObject, 2f);
    }
}
