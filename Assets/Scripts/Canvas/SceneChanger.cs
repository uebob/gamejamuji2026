using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChanger : MonoBehaviour
{
    // El "Instance" permite que otros scripts digan: SceneChanger.Instance.ChangeScene(...)
    public static SceneChanger Instance;

    private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        // Lógica de Singleton: solo puede haber uno
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            canvasGroup = GetComponent<CanvasGroup>();

            // Si olvidaste asignar el CanvasGroup en el inspector, lo buscamos
            if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>();

            canvasGroup.alpha = 0; 
            canvasGroup.blocksRaycasts = false; 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(Fade(1)); // Fundido a negro
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(0.1f); // Pequeña espera para que la escena cargue
        yield return StartCoroutine(Fade(0)); // Desaparece el negro
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }
}