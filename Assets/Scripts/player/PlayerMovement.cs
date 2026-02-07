using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashingGracePeriod = 0.4f;
    public bool canDash = true;

    [Header("Return")]
    [SerializeField] private float returnDuration = 0.35f;
    [SerializeField] private AnimationCurve returnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Refill")]
    public GameObject refillPrefab;

    [Header("Boss")]
    public GameObject boss;

    [Header("Audio Clips")]
    public AudioClip dashSFX;
    public AudioClip impactSFX;
    public AudioClip weakpointSFX;
    public AudioClip slimeSFX;

    private Vector2 refillPrefabPosition;
    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 moveInput;

    [HideInInspector] public bool isDashing = false;
    [HideInInspector] public bool isDashingGracePeriod = false;
    public bool isReturning = false;
    private bool isDead = false; // Nueva flag para controlar la muerte

    private CameraShake2D camShake;
    private CameraFlash2D camFlash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (Camera.main != null)
        {
            camShake = Camera.main.GetComponent<CameraShake2D>();
            camFlash = Camera.main.GetComponent<CameraFlash2D>();
        }
    }

    private void Update()
    {
        // Si está volviendo, haciendo dash o muerto, no procesamos input
        if (isReturning || isDashing || isDead)
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
        if (isReturning || isDead)
            return;

        rb.MovePosition(rb.position + moveInput * (isDashing ? dashSpeed : moveSpeed) * Time.fixedDeltaTime);
    }

    public bool IsReturning() => isReturning;

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        isDashingGracePeriod = true;
        canDash = false;

        AudioManager.Instance?.PlaySFX(dashSFX, 1f, Random.Range(0.95f, 1.05f));

        Instantiate(refillPrefab, rb.position, Quaternion.identity);
        refillPrefabPosition = rb.position;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        yield return new WaitForSeconds(dashingGracePeriod);
        isDashingGracePeriod = false;
    }

    private IEnumerator DieRoutine()
    {
        if (isDead) yield break;

        isDead = true;

        if (col != null) col.enabled = false;

       
        yield return new WaitForSeconds(1f);

        if (SceneChanger.Instance != null)
        {
            SceneChanger.Instance.ChangeScene("Temp_Door");
        }
        else
        {
            SceneManager.LoadScene("Temp_Door");
        }
    }

    private IEnumerator ReturnToRefill()
    {
        isReturning = true;
        isDashing = false;
        col.enabled = false;

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
        if (isDead) return;

        if (collision.gameObject.CompareTag("Hazard") && !isDashingGracePeriod)
        {
            StartCoroutine(DieRoutine()); 
            return;
        }

        if (!isDashingGracePeriod) return;

        // 2. DashObject genérico
        if (collision.gameObject.layer == LayerMask.NameToLayer("DashObject"))
        {
            Vector2 bounceDir = (rb.position - (Vector2)collision.transform.position).normalized;
            rb.MovePosition(rb.position + bounceDir * 0.2f);

            AudioManager.Instance?.PlaySFX(impactSFX);

            isDashing = false;
            isDashingGracePeriod = false;

            StartCoroutine(ReturnToRefill());
        }
        // 3. WeakPoint del boss 
        else if (collision.gameObject.layer == LayerMask.NameToLayer("WeakPoint"))
        {
            camShake?.Shake();
            camFlash?.Flash();

            AudioClip[] clips = { weakpointSFX, slimeSFX };
            float[] delays = { 0.1f, 0.3f };
            AudioManager.Instance?.PlaySFXSequence(clips, delays);

            if (boss != null)
                StartCoroutine(boss.GetComponent<BossController>().RecibirDano());

            isDashing = false;
            isDashingGracePeriod = false;

            StartCoroutine(ReturnToRefill());
        }
    }
}