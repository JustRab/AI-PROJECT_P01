using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{

    public int maxHealth = 100;
    [SerializeField] private float currentHealth;
    private Animator animator;
    private PlayerController playerController;
    protected Renderer[] playerRenderers; 
    protected Color[] originalColors;    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        playerRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[playerRenderers.Length];
    }

    void Update()
    {
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("Player has taken damage: " + damage);
        currentHealth -= damage;
        StartCoroutine(FlashRed());
        animator.SetTrigger("Hurt");
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        Debug.Log("Player has died");
        animator.SetTrigger("Die");
        playerController.enabled = false;
        yield return new WaitForSeconds(2f);
        
        // Example: Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator FlashRed()
    {
        // Cambia el color de todos los renderers a rojo
        foreach (Renderer rend in playerRenderers)
        {
            rend.material.color = Color.red;
        }
        yield return new WaitForSeconds(0.2f);
        // Vuelve a asignar el color original a cada renderer
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            playerRenderers[i].material.color = originalColors[i];
        }
    }
}
