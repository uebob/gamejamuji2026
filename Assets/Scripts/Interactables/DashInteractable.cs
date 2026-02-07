using UnityEngine;
using System.Collections;

public class DashInteractable : MonoBehaviour
{
    private bool hasDeactivated = false;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasDeactivated) return;

        PlayerMovement player = collision.collider.GetComponent<PlayerMovement>();

        if (player != null && player.isDashing)
        {
            hasDeactivated = true;
            StartCoroutine(HideAndDestroy());
        }
    }

    private IEnumerator HideAndDestroy()
    {
        if (animator != null)
        {
            animator.Play("tentaculo_escondiendose");
        }

        yield return new WaitForSeconds(0.4f);

        Destroy(gameObject);
    }
}