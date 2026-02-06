using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float acceleration = 20f;

    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashAcceleration = 60f;
    [SerializeField] private float dashDeceleration = 100f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] public bool canDash = true;

    public GameObject refillPrefab;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private bool isDashing = false;
    private float lastDashTime = -100f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing)
        {
            if (canDash)
            {
                if (moveInput != Vector2.zero)
                {
                    canDash = false;
                    StartCoroutine(DashRoutine());
                }
            }
        }
    }

    private void FixedUpdate()
    {
        float currentSpeed = isDashing ? dashSpeed : moveSpeed;
        float currentAcceleration = acceleration;

        if (isDashing)
        {
            currentAcceleration = dashAcceleration;
        }
        else if (rb.linearVelocity.magnitude > moveSpeed + 0.5f)
        {
            currentAcceleration = dashDeceleration;
        }

        Vector2 targetVelocity = moveInput * currentSpeed;

        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, currentAcceleration * Time.fixedDeltaTime);
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;
        Instantiate(refillPrefab, transform.position, transform.rotation);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }
}