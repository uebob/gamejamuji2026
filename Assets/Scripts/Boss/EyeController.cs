using UnityEngine;

public class EyeController : MonoBehaviour
{

    private Vector3 posicionInicial;
    [SerializeField] private Vector3 posicionBaja;
    [SerializeField] private float velocidadMovimiento = 2f;
    private bool estaDescendiendo;
    private bool estaAscendiendo;
    

    private bool estaAtacando;
    private bool esVulnerable;
    [SerializeField] private float duracionEnPosicionBaja = 3f;
    private float tiempoEnPosicionBaja;
    

    [SerializeField] private BossController controladorBoss;
    [SerializeField] private GameObject prefabLaser;
    [SerializeField] private Transform puntoDisparo;
    
    void Start()
    {
        posicionInicial = transform.position;
    }
    
    void Update()
    {
        if (!estaAtacando) return;
        
        if (estaDescendiendo)
        {
            Descender();
        }
        else if (estaAscendiendo)
        {
            Ascender();
        }
        else
        {
            tiempoEnPosicionBaja += Time.deltaTime;
            
            if (tiempoEnPosicionBaja >= duracionEnPosicionBaja)
            {
                IniciarAscenso();
            }
        }
    }
    
    public void IniciarAtaque()
    {
        estaAtacando = true;
        estaDescendiendo = true;
        tiempoEnPosicionBaja = 0f;
    }
    
    void Descender()
    {
        transform.position = Vector3.MoveTowards(transform.position, posicionBaja, velocidadMovimiento * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, posicionBaja) < 0.01f)
        {
            estaDescendiendo = false;
            DispararLaser();
            esVulnerable = true;
        }
    }
    
    void IniciarAscenso()
    {
        estaAscendiendo = true;
        esVulnerable = false;
    }
    
    void Ascender()
    {
        transform.position = Vector3.MoveTowards(transform.position, posicionInicial, velocidadMovimiento * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, posicionInicial) < 0.01f)
        {
            estaAscendiendo = false;
            estaAtacando = false;
            controladorBoss.FinalizarAtaque();
        }
    }
    
    void DispararLaser()
    {
        if (prefabLaser != null && puntoDisparo != null)
        {
            Instantiate(prefabLaser, puntoDisparo.position, puntoDisparo.rotation);
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (esVulnerable && collision.CompareTag("Player"))
        {
            controladorBoss.RecibirDaño();
        }
    }
}