using UnityEngine;

public class BounceShakeTrigger : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player != null && player.isDashingGracePeriod)
        {
            CameraShake2D camShake = FindFirstObjectByType<CameraShake2D>();
            if (camShake != null)
            {
                camShake.Shake();
            }
        }
    }
}
