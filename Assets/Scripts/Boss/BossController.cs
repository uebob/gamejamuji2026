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
    public GameObject followingTentacle;
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

        //si hay tentaculo :3
        GameObject tentacleAlive = GameObject.FindGameObjectWithTag("FollowingTentacle");

        int ataqueAleatorio;
        if(tentacleAlive!=null) 
        {
            ataqueAleatorio = Random.Range(1, 3);
        }
        else 
        {
            ataqueAleatorio = Random.Range(0, 3);
        }

        
        if (ataqueAleatorio == 0) StartCoroutine(EjecutarAtaqueFollow());
        else if (ataqueAleatorio == 1) StartCoroutine(EjecutarAtaqueTentaculo());
        else if (ataqueAleatorio == 2) StartCoroutine(EjecutarAtaqueLluvia());
    }

    private IEnumerator EjecutarAtaqueFollow()
    {
        Instantiate(followingTentacle, player.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.2f);
        FinalizarAtaque();
    }

    private IEnumerator EjecutarAtaqueTentaculo()
    {
        // Sin bucles: Calculamos una posici�n y listo
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

        // Lista para recordar dónde han caído los rayos en esta tanda
        List<Vector2> posicionesUsadas = new List<Vector2>();
        float radioSeguridad = 1.5f; // Ajusta esto según el ancho de tu sprite del rayo
        float radioCuadrado = radioSeguridad * radioSeguridad; // Optimización: evitamos raíces cuadradas

        for (int i = 0; i < 20; i++)
        {
            Vector2 posicionCandidata = Vector2.zero;
            bool posicionValida = false;
            int intentos = 0;

            // Intentamos encontrar una posición válida (máximo 10 intentos para no colgar el juego)
            while (!posicionValida && intentos < 10)
            {
                intentos++;

                // 30% probabilidad de ir al jugador (solo en el primer intento para no sesgar)
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

                // Comprobamos si esta posición choca con alguna anterior
                posicionValida = true;
                foreach (Vector2 posExistente in posicionesUsadas)
                {
                    // Usamos sqrMagnitude que es mucho más rápido que Distance
                    if ((posicionCandidata - posExistente).sqrMagnitude < radioCuadrado)
                    {
                        posicionValida = false;
                        break; // Ya choca con una, descartamos y probamos otra vez
                    }
                }
            }

            // Instanciamos en la posición encontrada (o la última probada si fallaron los 10 intentos)
            Instantiate(lightning, posicionCandidata, Quaternion.identity);
            
            // Guardamos la posición en la lista para que los siguientes rayos la eviten
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

        yield return new WaitForSeconds(1.5f); // Un poco m�s de tiempo para castigarle

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
            yield return new WaitForSeconds(0.2f);
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