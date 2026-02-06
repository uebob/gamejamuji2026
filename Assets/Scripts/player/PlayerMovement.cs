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
    [SerializeField] private float returnSpeed = 15f; 
    [SerializeField] private float dashingGracePeriod = 0.4f;
    public GameObject refillPrefab;
    private Vector2 refillPrefabPosition;
    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 moveInput;

    private bool isDashing = false;
    private bool isDashingGracePeriod = false;
    private bool isReturning = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (isReturning) return;

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
        if (isReturning) return;

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
        isDashingGracePeriod = true;

        Instantiate(refillPrefab, transform.position, transform.rotation);
        refillPrefabPosition = transform.position;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        yield return new WaitForSeconds(dashingGracePeriod);

        isDashingGracePeriod = false;
    }

    private IEnumerator ReturnToRefill()
    {
        isReturning = true;
        isDashing = false;
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;

        while (Vector2.Distance(rb.position, refillPrefabPosition) > 0.1f)
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, refillPrefabPosition, returnSpeed * Time.deltaTime);
            rb.MovePosition(newPos);
            yield return null;
        }

        rb.MovePosition(refillPrefabPosition);
        col.enabled = true;
        isReturning = false;

        // Destruye los refills si aún quedan (causado por el cooldown que hay para coger el refill)
        GameObject[] refills = GameObject.FindGameObjectsWithTag("Refill");
        foreach (GameObject obj in refills)
        {
            Destroy(obj);
        }
        canDash = true; //recarga por si el refill ha sido destruido
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("WeakPoint") && isDashingGracePeriod)
        {
            isDashing = false;
            isDashingGracePeriod = false;
            StartCoroutine(ReturnToRefill());
        }
    }
}