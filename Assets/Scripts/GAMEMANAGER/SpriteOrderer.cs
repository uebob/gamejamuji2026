using UnityEngine;
using System.Linq;

public class SpriteOrderer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SpriteRenderer[]spriteRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        spriteRenderers = spriteRenderers.OrderByDescending(sr => sr.transform.position.y).ToArray();
            
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if(!spriteRenderers[i].CompareTag("Refill"))
                spriteRenderers[i].sortingOrder = i;
        }
    }
}
