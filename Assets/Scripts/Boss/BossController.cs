using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    private int vidaActual;
    [SerializeField] private int vidaMaxima = 3;
    public enum EstadoBoss { Idle, Atacando, RecibiendoDano, Vulnerable, Muerto, EmpezandoCombate, Succionando }
    public EstadoBoss estadoActual;

    [SerializeField] private float tiempoEntreAtaques = 2f;
    private float tiempoUltimoAtaque;

    public GameObject player;
    public GameObject weakPoint;
    public GameObject lightning;
    public GameObject tentacle;
    public GameObject followingTentacle;
    private Animator animator;
    
    // Ya no necesitamos 'sr' aquí solo para el Boss, lo cogeremos localmente en Morir para todos

    private int layerWeakPoint;
    private int layerDashObject;

    [Header("Audio")]
    public AudioClip bossCrySFX;
    public AudioClip startingRoarSFX;
    public AudioClip deathSFX;
    public AudioClip chuparSFX;
    private AudioSource audioSource;

    [Header("Config Succion")]
    [SerializeField] private float duracionSuccion = 5f;
    [SerializeField] private float fuerzaSuccion = 3f;
    [SerializeField] private float radioSuccion = 10f;


    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        layerWeakPoint = LayerMask.NameToLayer("WeakPoint");
        layerDashObject = LayerMask.NameToLayer("DashObject");

        if (weakPoint != null) weakPoint.layer = layerDashObject;
    }

    void Start()
    {
        vidaActual = vidaMaxima;
        estadoActual = EstadoBoss.EmpezandoCombate;
        StartCoroutine(SecuenciaInicioCombate());
    }

    void Update()
    {
        if (estadoActual == EstadoBoss.Idle)
        {
            if (Time.time - tiempoUltimoAtaque >= tiempoEntreAtaques)
            {
                estadoActual = EstadoBoss.Atacando; 
                SeleccionarAtaqueAleatorio();
            }
        }
    }

    private IEnumerator SecuenciaInicioCombate()
    {
        if (startingRoarSFX != null) audioSource.PlayOneShot(startingRoarSFX);
        animator.Play("GREEN_DAMAGE");

        yield return new WaitForSeconds(4f);

        estadoActual = EstadoBoss.Idle;
        tiempoUltimoAtaque = Time.time;
    }

    void SeleccionarAtaqueAleatorio()
    {
        StopAllCoroutines();

        GameObject tentacleAlive = GameObject.FindGameObjectWithTag("FollowingTentacle");

        int ataqueAleatorio;
        if(tentacleAlive!=null) 
        {
            ataqueAleatorio = Random.Range(1, 4);
        }
        else 
        {
            ataqueAleatorio = Random.Range(0, 4);
        }

        
        if (ataqueAleatorio == 0) StartCoroutine(EjecutarAtaqueFollow());
        else if (ataqueAleatorio == 1) StartCoroutine(EjecutarAtaqueTentaculo());
        else if (ataqueAleatorio == 2) StartCoroutine(EjecutarAtaqueLluvia());
        else if (ataqueAleatorio == 3) StartCoroutine(EjecutarAtaqueSuccion());
    }

    private IEnumerator EjecutarAtaqueSuccion()
    {
        estadoActual = EstadoBoss.Succionando;
        animator.Play("GREEN_DAMAGE");
        audioSource.PlayOneShot(chuparSFX);

        CameraShake2D camScript = Camera.main.GetComponent<CameraShake2D>();

        if(camScript != null) camScript.isInducingShake = true;

        yield return new WaitForSeconds(1f); 

        float tiempoPasado = 0f;
        while (tiempoPasado < duracionSuccion)
        {
            AplicarFuerzaSuccion();
            tiempoPasado+= Time.deltaTime;
            yield return null;
        }

        if (camScript != null) camScript.isInducingShake = false;
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
       if(pm!= null)
       {
           pm.fuerzaSuccionExterna= Vector2.zero;
           pm.multiplicadorVelocidadExterna = 1f;
       }

       FinalizarAtaque();
    }   

    private void AplicarFuerzaSuccion()
    {
        if (player == null) return;

        float distancia = Vector2.Distance(transform.position, player.transform.position);

        if (distancia < radioSuccion)
        {
            Vector2 direccion = (transform.position - player.transform.position).normalized;

            float factorFuerza = 1f - (distancia / radioSuccion);
            Vector2 empuje = direccion * (fuerzaSuccion * factorFuerza);

            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.fuerzaSuccionExterna = empuje;
                pm.multiplicadorVelocidadExterna = 0.2f;
            }
        }
    }


    private IEnumerator EjecutarAtaqueFollow()
    {
        Instantiate(followingTentacle, player.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.2f);
        FinalizarAtaque();
    }

    private IEnumerator EjecutarAtaqueTentaculo()
    {
        float posX = Random.Range(3f, 6.5f) * (Random.Range(0, 2) == 0 ? 1 : -1);
        float posY = Random.Range(3f, 3.5f) * (Random.Range(0, 2) == 0 ? 1 : -1);
        Vector2 posicion = new Vector2(posX, posY);

        Instantiate(tentacle, posicion, Quaternion.identity);

        yield return new WaitForSeconds(0f);
        FinalizarAtaque();
    }

    private IEnumerator EjecutarAtaqueLluvia()
    {
        animator.Play("MIRAR_ARRIBA");

        List<Vector2> posicionesUsadas = new List<Vector2>();
        float radioSeguridad = 1.5f; 
        float radioCuadrado = radioSeguridad * radioSeguridad;

        for (int i = 0; i < 20; i++)
        {
            Vector2 posicionCandidata = Vector2.zero;
            bool posicionValida = false;
            int intentos = 0;

            while (!posicionValida && intentos < 10)
            {
                intentos++;

                if (intentos == 1 && Random.value < 0.3f && player != null)
                {
                    posicionCandidata = player.transform.position;
                }
                else
                {
                    float posX = Random.Range(-6f, 6f);
                    float posY = Random.Range(-4f, 4f);
                    posicionCandidata = new Vector2(posX, posY);
                }

                posicionValida = true;
                foreach (Vector2 posExistente in posicionesUsadas)
                {
                    if ((posicionCandidata - posExistente).sqrMagnitude < radioCuadrado)
                    {
                        posicionValida = false;
                        break; 
                    }
                }
            }

            Instantiate(lightning, posicionCandidata, Quaternion.identity);
            
            posicionesUsadas.Add(posicionCandidata);

            yield return new WaitForSeconds(0.1f);
        }

        animator.Play("MIRAR_ABAJO", 0, 0f);
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(VentanaDeAtaqueRoutine());
    }

    private IEnumerator VentanaDeAtaqueRoutine()
    {
        estadoActual = EstadoBoss.Vulnerable;
        if (weakPoint != null) weakPoint.layer = layerWeakPoint;
        animator.Play("GREEN_IDLE");

        yield return new WaitForSeconds(1.5f);

        if (estadoActual == EstadoBoss.Vulnerable)
        {
            CerrarOjo();
            FinalizarAtaque();
        }
    }

    private void CerrarOjo()
    {
        if (weakPoint != null) weakPoint.layer = layerDashObject;
    }

    public IEnumerator RecibirDano()
    {
        if (estadoActual != EstadoBoss.Vulnerable) yield break;

        vidaActual--;
        estadoActual = EstadoBoss.RecibiendoDano;

        if (weakPoint != null) weakPoint.layer = layerDashObject;

        yield return new WaitForSeconds(0.2f);
        if (bossCrySFX != null) audioSource.PlayOneShot(bossCrySFX);

        if (vidaActual <= 0)
        {
            StartCoroutine(Morir());
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            CerrarOjo();
            animator.Play("GREEN_DAMAGE");
            FinalizarAtaque();
        }
    }

    private IEnumerator Morir()
    {
        estadoActual = EstadoBoss.Muerto;
        animator.Play("BOSS_DIE");
        if (deathSFX != null) audioSource.PlayOneShot(deathSFX);

        yield return new WaitForSeconds(3f);

        // --- CAMBIO AQUÍ: Obtenemos el Boss y TODOS sus hijos ---
        SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>();
        
        // Guardamos los colores originales de cada parte
        Color[] startColors = new Color[allSprites.Length];
        for (int i = 0; i < allSprites.Length; i++)
        {
            startColors[i] = allSprites[i].color;
        }

        float fadeDuration = 3f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration; // 0 a 1

            // Iteramos sobre todas las partes (cuerpo + tentáculos)
            for (int i = 0; i < allSprites.Length; i++)
            {
                if (allSprites[i] != null)
                {
                    float currentAlpha = Mathf.Lerp(startColors[i].a, 0f, t);
                    allSprites[i].color = new Color(startColors[i].r, startColors[i].g, startColors[i].b, currentAlpha);
                }
            }
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void FinalizarAtaque()
    {
        estadoActual = EstadoBoss.Idle;
        tiempoUltimoAtaque = Time.time;
    }
}