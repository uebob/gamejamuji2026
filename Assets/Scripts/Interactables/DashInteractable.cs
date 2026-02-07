using UnityEngine;

public class DashInteractable : MonoBehaviour
{
    private bool used = false;

    public void OnDashHit()
    {
        if (used) return;

        used = true;
        gameObject.SetActive(false);
    }
   
}

  
    