using UnityEngine;

public class LoadBossScene : MonoBehaviour
{
    [SerializeField] private string bossSceneName = "javierrrr";

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Usamos el sistema de transición
            SceneChanger.Instance.ChangeScene(bossSceneName);
        }
    }
}