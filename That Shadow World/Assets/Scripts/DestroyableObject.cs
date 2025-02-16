using UnityEngine;

public class DestroyableObject : MonoBehaviour
{
    [Header("Settings")]
    public int maxHits = 5; // Number of hits before destruction
    private int currentHits = 0;
    
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeHit();
            Destroy(other.gameObject); // Destroy the bullet on impact
        }
    }

    void TakeHit()
    {
        currentHits++;
        if (!isFlashing)
        {
            StartCoroutine(FlashRed());
        }
        
        if (currentHits >= maxHits)
        {
            Destroy(gameObject);
        }
    }

    System.Collections.IEnumerator FlashRed()
    {
        isFlashing = true;
        objectRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        objectRenderer.material.color = originalColor;
        isFlashing = false;
    }
}
