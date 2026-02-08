using UnityEngine;
using System.Collections;

public class CameraShake2D : MonoBehaviour
{
    [Header("Shake Settings")]
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float duration = 0.3f;
    public float magnitude = 0.3f;

    [Header("Soft Shake Settings")]
    public float softDuration = 0.15f;
    public float softMagnitude = 0.1f;

    public bool isInducingShake = false;

    private Vector3 originalPos;

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    private void Update()
    {
        if (isInducingShake)
        {
            float offsetX = (Random.value * 2f - 1f) * softMagnitude;
            float offsetY = (Random.value * 2f - 1f) * softMagnitude;
            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
        }
    }

    public void Shake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine());
    }

    public void ShakeSoft()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeSoftCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;
            float curveValue = shakeCurve.Evaluate(t);

            float offsetX = (Random.value * 2f - 1f) * magnitude * curveValue;
            float offsetY = (Random.value * 2f - 1f) * magnitude * curveValue;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    private IEnumerator ShakeSoftCoroutine()
    {
        float timer = 0f;

        while (timer < softDuration)
        {
            float t = timer / softDuration;
            float curveValue = shakeCurve.Evaluate(t);

            float offsetX = (Random.value * 2f - 1f) * softMagnitude * curveValue;
            float offsetY = (Random.value * 2f - 1f) * softMagnitude * curveValue;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}