using UnityEngine;
using System.Collections;

public class DashInteractable : MonoBehaviour
{
    private bool hasDeactivated = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasDeactivated) return;

        PlayerMovement player = collision.collider.GetComponent<PlayerMovement>();
        if (player != null && player.isDashing)
        {
            gameObject.SetActive(false); 
            hasDeactivated = true;    
        }
    }
}

  
    