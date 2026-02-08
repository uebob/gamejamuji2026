using UnityEngine;

public class CamaraIntro : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float damping;
    [SerializeField] private SpriteRenderer limitSprite;

    public Transform target;
    private Vector3 vel = Vector3.zero;
    private float cameraHalfHeight;
    private float cameraHalfWidth;

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        cameraHalfHeight = cam.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * cam.aspect;
    }

    private void FixedUpdate()
    {
        Vector3 targetPosition = target.position + offset;

        targetPosition.x = transform.position.x;
        targetPosition.z = transform.position.z;

        Vector3 smoothPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref vel, damping);

        // Obtener límites del sprite
        Bounds spriteBounds = limitSprite.bounds;

        // Clamp de la posición de la cámara
        float clampedY = Mathf.Clamp(smoothPosition.y, 
            spriteBounds.min.y + cameraHalfHeight, 
            spriteBounds.max.y - cameraHalfHeight);

        smoothPosition.y = clampedY;

        transform.position = smoothPosition;
    }
}