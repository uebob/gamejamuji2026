using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class BossController : MonoBehaviour
{
    private int vidaActual;
    [SerializeField] private int vidaMaxima = 3;
    private enum EstadoBoss { Idle, Atacando, RecibiendoDano, Muerto }
    private EstadoBoss estadoActual;

    [SerializeField] private float tiempoEntreAtaques = 2f;
    private float tiempoUltimoAtaque;

    public GameObject player;
    public GameObject weakPoint;
    public GameObject lightning;
    private Animator animator;

    [Header("Audio")]
    public AudioClip bossCrySFX; // arrastrar clip en inspector
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D
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
        int ataqueAleatorio = 0;

        if (ataqueAleatorio == 0)
        {
            StartCoroutine(EjecutarAtaqueLluvia());
        }
        else if (ataqueAleatorio == 1)
        {
            EjecutarAtaqueTentaculo(Random.Range(0, 2) == 0);
        }
        else if (ataqueAleatorio == 2)
        {
            EjecutarAtaqueOjo();
        }
    }

    void EjecutarAtaqueTentaculo(bool esIzquierdo)
    {
        if (esIzquierdo)
        {

        }
        else
        {

        }
    }

    private IEnumerator EjecutarAtaqueLluvia()
    {
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
                    float posX = Random.Range(2f, 6f) * (Random.Range(0, 2) == 0 ? 1 : -1);
                    float posY = Random.Range(1.5f, 4f) * (Random.Range(0, 2) == 0 ? 1 : -1);
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

                if (!choca)
                {
                    encontradaPosicionLibre = true;
                }
            }

            if (encontradaPosicionLibre)
            {
                Instantiate(lightning, posicionCandidata, Quaternion.identity);
                posicionesUsadas.Add(posicionCandidata);
            }

            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1f);
        FinalizarAtaque();
    }

    void EjecutarAtaqueOjo()
    {

    }

    public IEnumerator RecibirDano()
    {
        vidaActual--;
        estadoActual = EstadoBoss.RecibiendoDano;

        yield return new WaitForSeconds(0.5f);
        if (bossCrySFX != null)
            audioSource.PlayOneShot(bossCrySFX);

        animator = gameObject.GetComponent<Animator>();
        animator.Play("GREEN_DAMAGE");

        if (vidaActual <= 0)
        {
            Morir();
        }

        yield return new WaitForSeconds(1);

        estadoActual = EstadoBoss.Idle;
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
