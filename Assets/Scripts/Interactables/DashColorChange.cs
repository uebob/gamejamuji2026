using UnityEngine;

public class DashColorChange : MonoBehaviour
{
    [SerializeField] private Color dashColor = Color.red; // Color al que cambia
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobamos si el objeto que entró al trigger es el jugador
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && player.isDashing) // Solo mientras está dashando
        {
            ChangeColor();
        }
    }

    private void ChangeColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = dashColor;
    }

    // Opcional: función para volver al color original
    public void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}
