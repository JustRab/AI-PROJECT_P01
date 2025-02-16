using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public int damageAmount = 10;

    void OnTriggerEnter(Collider other)
    { 
        // If the bullet hits an enemy, determine which type it is.
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Hit!");

            // Check for TurretEnemy first.
            TurretEnemy turret = other.GetComponent<TurretEnemy>();
            if (turret != null)
            {
                turret.TakeDamage(damageAmount);
                Destroy(gameObject);
                return;
            }

            // Check for HeavyEnemy next.
            HeavyEnemy heavy = other.GetComponent<HeavyEnemy>();
            if (heavy != null)
            {
                heavy.TakeDamage(damageAmount);
                Destroy(gameObject);
                return;
            }

            // Check for FleeEnemy.
             FleeEnemy flee = other.GetComponent<FleeEnemy>();
             if (flee != null)
             {
                 flee.TakeDamage(damageAmount);
                 Destroy(gameObject);
                 return;
             }
        }
        // If the bullet hits an object tagged as Environment, destroy it.
        else if (other.CompareTag("Environment") || other.CompareTag("Obstacles"))
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
