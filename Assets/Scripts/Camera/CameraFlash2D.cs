using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraFlash2D : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.08f;

    private Camera cam;
    private GameObject flashObj;
    private SpriteRenderer flashSprite;
    private Coroutine flashRoutine;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        // Crear objeto del flash
        flashObj = new GameObject("CameraFlash");
        flashObj.transform.SetParent(transform);
        flashObj.transform.localPosition = new Vector3(0, 0, 1);

        flashSprite = flashObj.AddComponent<SpriteRenderer>();
        flashSprite.sprite = CreateWhiteSprite();
        flashSprite.color = new Color(1, 1, 1, 0);
        flashSprite.sortingOrder = 9999; // encima de TODO

        ResizeFlash();
        flashObj.SetActive(false);
    }

    // 👉 Llamas a esto desde el Player
    public void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        ResizeFlash(); // por si cambia resolución

        flashObj.SetActive(true);
        flashSprite.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        flashSprite.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
        flashObj.SetActive(false);
    }

    // Ajusta el tamaño para cubrir TODA la cámara
    private void ResizeFlash()
    {
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        flashObj.transform.localScale = new Vector3(width, height, 1f);
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }
}
