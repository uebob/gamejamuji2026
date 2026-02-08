using UnityEngine;
using System.Collections;

public class Lightning : MonoBehaviour
{
    [SerializeField] GameObject lightningRay;
    public AudioClip lightningSFX;

    private SpriteRenderer sr;
    private BoxCollider2D col;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        if (col != null) col.enabled = false;
        if (lightningRay != null) lightningRay.SetActive(false);
        StartCoroutine(Strike());
    }

    private IEnumerator Strike()
    {
        if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.5f);

        yield return new WaitForSeconds(0.67f);

        // Si el objeto fue destruido mientras esper�bamos, salimos
        if (this == null) yield break;

        if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f);
        if (col != null) col.enabled = true;

        // El AudioManager a veces da tirones si se llama muchas veces seguidas
        if (lightningSFX != null) AudioManager.Instance?.PlaySFX(lightningSFX);

        if (lightningRay != null) lightningRay.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}