using UnityEngine;
using System.Collections;

public class PiedraDashInteractable : MonoBehaviour
{

    private int vidapiedra = 3;
    private bool hasDeactivated = false;
    private Animator animator;
    private Collider2D col;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (col != null) col.enabled = false;
        StartCoroutine(ActivateCollision());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (hasDeactivated) return;

        PlayerMovement player = collision.collider.GetComponent<PlayerMovement>();

        if (player != null && player.isDashing)
        {
            vidapiedra -=1;
        }

        if (player != null && player.isDashing && vidapiedra == 0)
        {
            hasDeactivated = true;
            StopAllCoroutines(); // Para que no se autodestruya mientras se esconde
            StartCoroutine(HideAndDestroy());
        }
    }


    private IEnumerator HideAndDestroy()
    {
        hasDeactivated = true;
        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject);
    }

    private IEnumerator ActivateCollision()
    {
       yield return new WaitForSeconds(0.5f);
       if (col != null) col.enabled = true;
    }
}