using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    private int vidaActual;
    [SerializeField] private int vidaMaxima = 3;
    public enum EstadoBoss { Idle, Atacando, RecibiendoDano, Vulnerable, Muerto }
    public EstadoBoss estadoActual;

    [SerializeField] private float tiempoEntreAtaques = 2f;
    private float tiempoUltimoAtaque;

    public GameObject player;
    public GameObject weakPoint;
    public GameObject lightning;
    public GameObject tentacle;
    private Animator animator;

    private int layerWeakPoint;
    private int layerDashObject;

    [Header("Audio")]
    public AudioClip bossCrySFX;
    private AudioSource audioSource;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        layerWeakPoint = LayerMask.NameToLayer("WeakPoint");
        layerDashObject = LayerMask.NameToLayer("DashObject");

        if (weakPoint != null)
            weakPoint.layer = layerDashObject;
    }

    void Start()
    {
        vidaActual = vidaMaxima;
        estadoActual = EstadoBoss.Idle;
        tiempoUltimoAtaque = -tiempoEntreAtaques;
    }

    void Update()
    {
        if (estadoActual == EstadoBoss.Idle && Time.time - tiempoUltimoAtaque >= tiempoEntreAtaques)
        {
            SeleccionarAtaqueAleatorio();
            estadoActual = EstadoBoss.Atacando;
        }
    }

    void SeleccionarAtaqueAleatorio()
    {
        int ataqueAleatorio = Random.Range(0, 2);

        if (ataqueAleatorio == 0)
        {
            StartCoroutine(EjecutarAtaqueLluvia());
        }
        else
        {
            StartCoroutine(EjecutarAtaqueTentaculo());
        }
    }

    private IEnumerator EjecutarAtaqueTentaculo()
    {
        Vector2 posicion;
        int intentos = 0;

        do
        {
            float posX = Random.Range(3f, 6.5f) * (Random.Range(0, 2) == 0 ? 1 : -1);
            float posY = Random.Range(3f, 3.5f) * (Random.Range(0, 2) == 0 ? 1 : -1);
            posicion = new Vector2(posX, posY);
            intentos++;
        }
        while (Vector2.Distance(posicion, player.transform.position) < 2f && intentos < 15);

        Instantiate(tentacle, posicion, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
        FinalizarAtaque();
    }

    private IEnumerator EjecutarAtaqueLluvia()
    {
        // 1. Iniciamos ataque
        animator.Play("MIRAR_ARRIBA");

        List<Vector2> posicionesUsadas = new List<Vector2>();
        float distanciaMinima = 1.5f;

        for (int i = 0; i < 20; i++)
        {
            Vector2 posicionCandidata = Vector2.zero;
            bool encontradaPosicionLibre = false;
            int intentos = 0;

            while (!encontradaPosicionLibre && intentos < 10)
            {
                intentos++;
                if (Random.Range(0, 3) == 0)
                {
                    posicionCandidata = player.transform.position;
                }
                else
                {
                    float posX = Random.Range(2.5f, 6f) * (Random.Range(0, 2) == 0 ? 1 : -1);
                    float posY = Random.Range(2f, 4f) * (Random.Range(0, 2) == 0 ? 1 : -1);
                    posicionCandidata = new Vector2(posX, posY);
                }

                bool choca = false;
                foreach (Vector2 pos in posicionesUsadas)
                {
                    if (Vector2.Distance(posicionCandidata, pos) < distanciaMinima)
                    {
                        choca = true;
                        break;
                    }
                }
                if (!choca) encontradaPosicionLibre = true;
            }

            if (encontradaPosicionLibre)
            {
                Instantiate(lightning, posicionCandidata, Quaternion.identity);
                posicionesUsadas.Add(posicionCandidata);
            }

            yield return new WaitForSeconds(0.05f);
        }

        // 2. Transición a MIRAR_ABAJO antes de la vulnerabilidad
        // Forzamos la animación. Asegúrate de que no haya transiciones que la saquen de aquí por error.
        animator.Play("MIRAR_ABAJO", 0, 0f);

        // Esperamos un segundo para que se vea al Boss bajando la mirada
        yield return new WaitForSeconds(1f);

        // 3. Entrar en vulnerabilidad
        yield return StartCoroutine(VentanaDeAtaqueRoutine());
    }

    private IEnumerator VentanaDeAtaqueRoutine()
    {
        estadoActual = EstadoBoss.Vulnerable;

        if (weakPoint != null)
            weakPoint.layer = layerWeakPoint;

        animator.Play("GREEN_IDLE");

        yield return new WaitForSeconds(1.0f);

        if (estadoActual == EstadoBoss.Vulnerable)
        {
            CerrarOjo();
            FinalizarAtaque();
        }
    }

    private void CerrarOjo()
    {
        if (weakPoint != null)
            weakPoint.layer = layerDashObject;

        animator.Play("CLOSED_IDLE");
    }

    public IEnumerator RecibirDano()
    {
        if (estadoActual != EstadoBoss.Vulnerable) yield break;

        vidaActual--;
        estadoActual = EstadoBoss.RecibiendoDano;

        if (weakPoint != null)
            weakPoint.layer = layerDashObject;

        yield return new WaitForSeconds(0.2f);

        if (bossCrySFX != null)
            audioSource.PlayOneShot(bossCrySFX);

        animator.Play("GREEN_DAMAGE");

        if (vidaActual <= 0)
        {
            Morir();
        }
        else
        {
            yield return new WaitForSeconds(1f);
            CerrarOjo();
            FinalizarAtaque();
        }
    }

    void Morir()
    {
        estadoActual = EstadoBoss.Muerto;
        gameObject.SetActive(false);
    }

    public void FinalizarAtaque()
    {
        estadoActual = EstadoBoss.Idle;
        tiempoUltimoAtaque = Time.time;
    }
}