using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

public class BossController : MonoBehaviour
{
    private int vidaActual;
    [SerializeField] private int vidaMaxima = 3;
    private enum EstadoBoss {Idle, Atacando, RecibiendoDano, Muerto}
    private EstadoBoss estadoActual;

    [SerializeField] private float tiempoEntreAtaques = 2f;
    private float tiempoUltimoAtaque;

    public GameObject weakPoint;
    private Animator animator;

    void Start()
    {

        vidaActual = vidaMaxima;
        estadoActual = EstadoBoss.Idle;
        tiempoUltimoAtaque = -tiempoEntreAtaques;
    }

    void Update()
    {

        if(Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(RecibirDano());
        }
        
        if (estadoActual == EstadoBoss.Idle && Time.time - tiempoUltimoAtaque >= tiempoEntreAtaques)
        {
            SeleccionarAtaqueAleatorio();
            estadoActual = EstadoBoss.Atacando;
            tiempoUltimoAtaque = Time.time;
        }
    }

    void SeleccionarAtaqueAleatorio()
    {
        int ataqueAleatorio = Random.Range(0, 3);
        
        if (ataqueAleatorio == 0)
        {
            EjecutarAtaqueTentaculo(Random.Range(0, 2) == 0);
        }
        else if (ataqueAleatorio == 1)
        {
            EjecutarAtaqueLluvia();
        }
        else
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
        estadoActual = EstadoBoss.Atacando;
        yield return new WaitForSeconds(0.3f);
    }

    void EjecutarAtaqueOjo()
    {

    }

    public IEnumerator RecibirDano()
    {
        vidaActual--;
        estadoActual = EstadoBoss.RecibiendoDano;

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
        tiempoUltimoAtaque = 0f;
    }

}
