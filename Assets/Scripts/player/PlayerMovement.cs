using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float acceleration = 20f; 

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
    }
}