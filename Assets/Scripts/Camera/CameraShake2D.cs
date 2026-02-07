using UnityEngine;
using System.Collections;

public class CameraShake2D : MonoBehaviour
{
    [Header("Shake Settings")]
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float duration = 0.3f;
    public float magnitude = 0.3f;

    private Vector3 originalPos;

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    /// <summary>
    /// Llama a este m�todo para iniciar el shake
    /// </summary>
    public void Shake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;
            float curveValue = shakeCurve.Evaluate(t);

            // Aplica el shake multiplicando por la curva
            float offsetX = (Random.value * 2f - 1f) * magnitude * curveValue;
            float offsetY = (Random.value * 2f - 1f) * magnitude * curveValue;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        // Asegura que la c�mara vuelve a su posici�n original
        transform.localPosition = originalPos;
    }
}
