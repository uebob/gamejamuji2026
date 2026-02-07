using UnityEngine;

public class BounceShakeTrigger : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Comprobamos que el que colisiona sea el jugador
        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player != null)
        {
            // Queremos asegurarnos de que el jugador esté en la fase de rebote
            // (isDashingGracePeriod es true justo después de dash, justo antes del return)
            if (player.isDashingGracePeriod)
            {
                CameraShake2D camShake = Camera.main.GetComponent<CameraShake2D>();
                if (camShake != null)
                {
                    camShake.Shake();
                }
            }
        }
    }
}