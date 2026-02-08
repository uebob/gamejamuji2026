using UnityEngine;
using System.Collections;

public class DashInteractable : MonoBehaviour
{
    private bool hasDeactivated = false;
    private Animator animator;
    private Collider2D col;
    [SerializeField] float tiempoAutodestruccion = 5f; // Para que no se acumulen

    private void Awake()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Si el jugador no lo mata Y ES TENTACULO, se destruye solo para limpiar la escena
        if(gameObject.CompareTag("Hazard")) StartCoroutine(AutoLimpieza());

        if (col != null) col.enabled = false;
        StartCoroutine(ActivateCollision());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasDeactivated) return;

        PlayerMovement player = collision.collider.GetComponent<PlayerMovement>();

        if (player != null && player.isDashing)
        {
            hasDeactivated = true;
            StopAllCoroutines(); // Para que no se autodestruya mientras se esconde
            StartCoroutine(HideAndDestroy());
        }
    }

    private IEnumerator AutoLimpieza()
    {
        yield return new WaitForSeconds(tiempoAutodestruccion);
        if (!hasDeactivated) StartCoroutine(HideAndDestroy());
    }

    private IEnumerator HideAndDestroy()
    {
        hasDeactivated = true;
        if (animator != null)
        {
            animator.Play("tentaculo_escondiendose");
        }

        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject);
    }

    private IEnumerator ActivateCollision()
    {
       yield return new WaitForSeconds(0.5f);
       if (col != null) col.enabled = true;
    }
}