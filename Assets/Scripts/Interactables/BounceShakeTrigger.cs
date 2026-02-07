using UnityEngine;

public class BounceShakeTrigger : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Este objeto DEBE ser WeakPoint
        if (gameObject.layer != LayerMask.NameToLayer("WeakPoint"))
            return;

        // Comprobamos que el que colisiona sea el jugador
        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player == null)
            return;

        // Solo durante el grace period
        if (!player.isDashingGracePeriod)
            return;
    }
}
