using UnityEngine;
using System.Collections;


public class Lightning : MonoBehaviour
{
    [SerializeField] GameObject lightningRay;
    void Start()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        lightningRay.SetActive(false);
        StartCoroutine(Strike());
    }

    private IEnumerator Strike()
    {
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        
        yield return new WaitForSeconds(1f);
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        GetComponent<BoxCollider2D>().enabled = true;
        lightningRay.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        GetComponent<BoxCollider2D>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
