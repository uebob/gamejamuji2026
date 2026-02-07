using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashingGracePeriod = 0.4f;
    [SerializeField] public bool canDash = true;

    [Header("Return")]
    [SerializeField] private float returnDuration = 0.35f;
    [SerializeField] private AnimationCurve returnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Refill")]
    public GameObject refillPrefab;

    private Vector2 refillPrefabPosition;
    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 moveInput;

    [HideInInspector] public bool isDashing = false;
    [HideInInspector] public bool isDashingGracePeriod = false;
    public bool isReturning = false;

    private CameraShake2D camShake;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        camShake = Camera.main.GetComponent<CameraShake2D>();
    }

    private void Update()
    {
        if (isReturning || isDashing)
            return; 

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && canDash && moveInput != Vector2.zero)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private void FixedUpdate()
    {
        if (isReturning)
            return;

        rb.MovePosition(rb.position + moveInput * (isDashing ? dashSpeed : moveSpeed) * Time.fixedDeltaTime);
    }
    public bool IsReturning()
    {
        return isReturning;
    }   
    private IEnumerator DashRoutine()
    {
        isDashing = true;
        isDashingGracePeriod = true;
        canDash = false;

        // Creamos refill
        Instantiate(refillPrefab, rb.position, Quaternion.identity);
        refillPrefabPosition = rb.position;

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

        Vector2 startPos = rb.position;
        Vector2 targetPos = refillPrefabPosition;
        float timer = 0f;

        yield return new WaitForSeconds(0.02f);

        while (Vector2.Distance(rb.position, targetPos) > 0.01f)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / returnDuration);
            float curveValue = returnCurve.Evaluate(t);

            float step = curveValue * Vector2.Distance(rb.position, targetPos);
            step = Mathf.Max(step, 0.01f);
            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPos, step));

            yield return null;
        }

        rb.MovePosition(targetPos);
        col.enabled = true;
        isReturning = false;
        canDash = true;

        // Limpiamos refills antiguos
        CleanupRefills();
    }

    private void CleanupRefills()
    {
        GameObject[] refills = GameObject.FindGameObjectsWithTag("Refill");
        foreach (GameObject obj in refills)
            Destroy(obj);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
       
        if (collision.gameObject.layer == LayerMask.NameToLayer("DashObject") && isDashingGracePeriod)
        {

            // Rebote visual instantáneo
            Vector2 bounceDir = (rb.position - (Vector2)collision.transform.position).normalized;
            rb.MovePosition(rb.position + bounceDir * 0.2f);

            isDashing = false;
            isDashingGracePeriod = false;

            // Iniciamos return al refill
            StartCoroutine(ReturnToRefill());
        }

        if(collision.gameObject.layer == LayerMask.NameToLayer("WeakPoint") && isDashingGracePeriod)
        {
            // CAMERA SHAKE
            CameraShake2D camShake = Camera.main.GetComponent<CameraShake2D>();
            if (camShake != null)
            {
                camShake.Shake();
            }

            isDashing = false;
            isDashingGracePeriod = false;
            // Iniciamos return al refill
            StartCoroutine(ReturnToRefill());
        }
    }

}
