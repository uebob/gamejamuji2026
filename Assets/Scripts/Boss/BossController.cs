using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    private int vidaActual;
    [SerializeField] private int vidaMaxima = 3;
    public enum EstadoBoss { Idle, Atacando, RecibiendoDano, Vulnerable, Muerto, EmpezandoCombate }
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
    public AudioClip startingRoarSFX;
    private AudioSource audioSource;

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
        // Solo si estamos en Idle Y ha pasado el tiempo, atacamos
        if (estadoActual == EstadoBoss.Idle)
        {
            if (Time.time - tiempoUltimoAtaque >= tiempoEntreAtaques)
            {
                estadoActual = EstadoBoss.Atacando; // Bloqueo inmediato del estado
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
        // Limpiamos cualquier corrutina previa por seguridad
        StopAllCoroutines();

        int ataqueAleatorio = Random.Range(0, 2);
        if (ataqueAleatorio == 0) StartCoroutine(EjecutarAtaqueLluvia());
        else StartCoroutine(EjecutarAtaqueTentaculo());
    }

    private IEnumerator EjecutarAtaqueTentaculo()
    {
        // Sin bucles: Calculamos una posición y listo
        float posX = Random.Range(3f, 6.5f) * (Random.Range(0, 2) == 0 ? 1 : -1);
        float posY = Random.Range(3f, 3.5f) * (Random.Range(0, 2) == 0 ? 1 : -1);
        Vector2 posicion = new Vector2(posX, posY);

        Instantiate(tentacle, posicion, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
        FinalizarAtaque();
    }

    private IEnumerator EjecutarAtaqueLluvia()
    {
        animator.Play("MIRAR_ARRIBA");

        // Bajamos la cantidad a 15 para ser más ligeros
        for (int i = 0; i < 15; i++)
        {
            Vector2 posicion;
            // 30% de probabilidad de ir al jugador, 70% aleatorio
            if (Random.value < 0.3f && player != null)
            {
                posicion = player.transform.position;
            }
            else
            {
                float posX = Random.Range(-6f, 6f);
                float posY = Random.Range(-4f, 4f);
                posicion = new Vector2(posX, posY);
            }

            Instantiate(lightning, posicion, Quaternion.identity);

            // Esperar un poquito entre rayos es vital para que no pete
            yield return new WaitForSeconds(0.1f);
        }

        animator.Play("MIRAR_ABAJO", 0, 0f);
        yield return new WaitForSeconds(1f);

        // Iniciamos la vulnerabilidad
        yield return StartCoroutine(VentanaDeAtaqueRoutine());
    }

    private IEnumerator VentanaDeAtaqueRoutine()
    {
        estadoActual = EstadoBoss.Vulnerable;
        if (weakPoint != null) weakPoint.layer = layerWeakPoint;
        animator.Play("GREEN_IDLE");

        yield return new WaitForSeconds(1.5f); // Un poco más de tiempo para castigarle

        if (estadoActual == EstadoBoss.Vulnerable)
        {
            CerrarOjo();
            FinalizarAtaque();
        }
    }

    private void CerrarOjo()
    {
        if (weakPoint != null) weakPoint.layer = layerDashObject;
        animator.Play("CLOSED_IDLE");
    }

    public IEnumerator RecibirDano()
    {
        if (estadoActual != EstadoBoss.Vulnerable) yield break;

        vidaActual--;
        estadoActual = EstadoBoss.RecibiendoDano;

        if (weakPoint != null) weakPoint.layer = layerDashObject;

        yield return new WaitForSeconds(0.2f);
        if (bossCrySFX != null) audioSource.PlayOneShot(bossCrySFX);
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