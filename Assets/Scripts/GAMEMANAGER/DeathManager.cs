using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void FuckingDie()
    {
        SceneManager.LoadScene("Temp_Door");
    }
}
