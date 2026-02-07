using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance;
 
    private bool isDying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public void FuckingDie(GameObject player)
    {
        if (isDying) return;
        isDying = true;
        StartCoroutine(DieRoutine(player));
    }

    private IEnumerator DieRoutine(GameObject player)
    {
        // Bloquear player
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
            pm.enabled = false;
      
        // Cambiar escena
        SceneManager.LoadScene("Temp_Door");

        // Esperar un frame para que cargue la nueva escena
        yield return null;

  
        // Desbloquear player
        if (pm != null)
            pm.enabled = true;

        isDying = false;
    }
}
