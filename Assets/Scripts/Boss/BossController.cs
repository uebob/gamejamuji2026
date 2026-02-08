using UnityEngine;

public class BossController : MonoBehaviour
{
    private int vidaActual;
    [SerializeField] private int vidaMaxima = 3;
    private enum EstadoBoss {Idle, Atacando, Muerto}
    private EstadoBoss estadoActual;

    [SerializeField] private float tiempoEntreAtaques = 2f;
    private float tiempoUltimoAtaque;

    //[SerializeField] private TentacleAttack tentaculoIzquierdo;
    //[SerializeField] private TentacleAttack tentaculoDerecho;
    [SerializeField] private EyeController ojo;
    //[SerializeField] private LaserRain lanzadorLaseres;

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
        tentaculoIzquierdo.IniciarBarrido();
    }
    else
    {
        tentaculoDerecho.IniciarBarrido();
    }
}

void EjecutarAtaqueLluvia()
{
    lanzadorLaseres.IniciarAtaque();
}

void EjecutarAtaqueOjo()
{
    ojo.IniciarAtaque();
}

public void RecibirDaño()
{
    vidaActual--;
    
    if (vidaActual <= 0)
    {
        Morir();
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
}

}
