using UnityEngine;
using System.Collections;

public class DashRefill : MonoBehaviour
{
    private Collider2D col;
    [SerializeField] private float noColFrames = 0.5f;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false;
        StartCoroutine(EnableCollider());
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.CompareTag("Player"))
        {
            collider.gameObject.GetComponent<PlayerMovement>().canDash = true;
            Destroy(gameObject);
        }
    }

    private IEnumerator EnableCollider()
    {
        yield return new WaitForSeconds(noColFrames);
        col.enabled = true;
    }
}
