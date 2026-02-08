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

    public AudioClip PiedraSFX;

    private Vector2 refillPrefabPosition;
    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 moveInput;
    private Animator animator;

    private SpriteRenderer sr;


    [HideInInspector] public Vector2 fuerzaSuccionExterna;
    [HideInInspector] public float multiplicadorVelocidadExterna = 1f;

    [HideInInspector] public bool isDashing = false;
    [HideInInspector] public bool isDashingGracePeriod = false;
    public bool isReturning = false;
    private bool isDead = false;

    private CameraShake2D camShake;
    private CameraFlash2D camFlash;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (Camera.main != null)
        {
            camShake = Camera.main.GetComponent<CameraShake2D>();
            camFlash = Camera.main.GetComponent<CameraFlash2D>();
        }
    }

    private void Update()
    {
        if (isReturning || isDashing || isDead)
            return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (animator != null)
        {
            bool movingUp = moveInput.y > 0.1f;
            animator.SetBool("IsUp", movingUp);

            bool movingDown = moveInput.y < -0.1f;
            animator.SetBool("IsDown", movingDown);

            bool movingRight = moveInput.x > 0.1f;
            bool movingLeft = moveInput.x < -0.1f;

            animator.SetBool("IsRight", movingRight || movingLeft);

            if (movingRight)
                transform.localScale = new Vector3(0.8f, 0.8f, 1);
            else if (movingLeft)
                transform.localScale = new Vector3(-0.8f, 0.8f, 1);
        }

        if (Input.GetKeyDown(KeyCode.Space) && canDash && moveInput != Vector2.zero)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private void FixedUpdate()
    {
        if (isReturning || isDead)
            return;

        //rb.MovePosition(rb.position + moveInput * (isDashing ? dashSpeed : moveSpeed) * Time.fixedDeltaTime);

        Vector2 velocudadActual = moveInput * (isDashing ? dashSpeed : moveSpeed);
        Vector2 movimientoFinal = velocudadActual + fuerzaSuccionExterna;
        rb.MovePosition(rb.position + movimientoFinal * Time.fixedDeltaTime);
        fuerzaSuccionExterna = Vector2.zero;
        multiplicadorVelocidadExterna = 1f;

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
        camShake?.ShakeSoft();

        // --- EFECTO FLASH SUAVE (Método de superposición) ---
        // 1. Creamos un objeto temporal hijo
        GameObject ghostObj = new GameObject("WhiteFlashOverlay");
        ghostObj.transform.SetParent(transform);
        ghostObj.transform.localPosition = Vector3.zero;
        ghostObj.transform.localScale = Vector3.one;

        // 2. Le ponemos un SpriteRenderer idéntico al nuestro
        SpriteRenderer ghostSr = ghostObj.AddComponent<SpriteRenderer>();
        if (sr != null)
        {
            ghostSr.sprite = sr.sprite;
            ghostSr.flipX = sr.flipX;
            ghostSr.flipY = sr.flipY;
            
            // 3. Le aplicamos el shader BLANCO puro
            ghostSr.material = new Material(Shader.Find("GUI/Text Shader"));
            ghostSr.color = new Color(1, 1, 1, 0); // Empieza invisible
        }

        // 4. Animación rápida de opacidad (Fade In -> Wait -> Fade Out)
        float duration = dashDuration; // 0.2s
        float fadeInTime = duration * 0.3f; // 30% del tiempo apareciendo
        float fadeOutTime = duration * 0.7f; // 70% del tiempo yéndose

        float timer = 0f;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.8f, timer / fadeInTime); // Sube hasta 0.8 de opacidad
            if (ghostSr != null) ghostSr.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0.8f, 0f, timer / fadeOutTime); // Baja a 0
            if (ghostSr != null) ghostSr.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        // 5. Destruimos el efecto
        Destroy(ghostObj);
        // ----------------------------------------------------

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
            SceneChanger.Instance.ChangeScene("Temp_Door");
        else
            SceneManager.LoadScene("Temp_Door");
    }

    private IEnumerator ReturnToRefill()
    {
        isReturning = true;
        isDashing = false;
        col.enabled = false;
        
        // Limpieza de emergencia por si el dash se corta a medias
        Transform existingGhost = transform.Find("WhiteFlashOverlay");
        if (existingGhost != null) Destroy(existingGhost.gameObject);

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

        if (collision.gameObject.layer == LayerMask.NameToLayer("DashObject"))
        {
            Vector2 bounceDir = (rb.position - (Vector2)collision.transform.position).normalized;
            rb.MovePosition(rb.position + bounceDir * 0.2f);
            AudioManager.Instance?.PlaySFX(impactSFX);

            // Limpieza efecto visual
            Transform existingGhost = transform.Find("WhiteFlashOverlay");
            if (existingGhost != null) Destroy(existingGhost.gameObject);

            isDashing = false;
            isDashingGracePeriod = false;
            camShake?.ShakeSoft();

            StartCoroutine(ReturnToRefill());
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("WeakPoint"))
        {

            if (collision.gameObject.CompareTag("Piedra")) 
            {
                AudioManager.Instance?.PlaySFX(PiedraSFX);
            }

            camShake?.Shake();
            camFlash?.Flash();

            AudioClip[] clips = { weakpointSFX, slimeSFX };
            float[] delays = { 0.1f, 0.3f };
            AudioManager.Instance?.PlaySFXSequence(clips, delays);

            if (boss != null)
                StartCoroutine(boss.GetComponent<BossController>().RecibirDano());

            // Limpieza efecto visual
            Transform existingGhost = transform.Find("WhiteFlashOverlay");
            if (existingGhost != null) Destroy(existingGhost.gameObject);

            isDashing = false;
            isDashingGracePeriod = false;

            StartCoroutine(ReturnToRefill());
        }
    }
}