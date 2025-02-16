using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    protected float currentHealth;

    public int damage = 10;
    public float attackCooldown = 1.5f;
    
    [Header("Components")]
     protected Animator animator;
    protected Renderer[] enemyRenderers; 
    protected Color[] originalColors;    
    protected bool isDead = false;

    public void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        enemyRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[enemyRenderers.Length];
    }


    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        // Llama a la coroutine para el parpadeo en rojo
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    IEnumerator FlashRed()
    {
        // Cambia el color de todos los renderers a rojo
        foreach (Renderer rend in enemyRenderers)
        {
            rend.material.color = Color.red;
        }
        yield return new WaitForSeconds(0.3f);
        // Vuelve a asignar el color original a cada renderer
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            enemyRenderers[i].material.color = originalColors[i];
        }
    }



    public virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("Die"); // Activa la animación de muerte
        Destroy(gameObject, 2f); // Destruye el enemigo después de la animación
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                animator.SetTrigger("Attack"); // Animación de ataque
            }
        }
    }
}
