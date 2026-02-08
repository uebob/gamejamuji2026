using UnityEngine;
using System.Collections;

public class FollowingTentacle : MonoBehaviour
{
    private GameObject player;
    
    [Header("Configuración")]
    public float speed = 5f;   
    public Vector3 offset;     
    private bool moving = false;
    private Animator animator;
    private Collider2D col;
    public AudioClip popupSFX;
    private AudioSource audioSource;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(popupSFX);
        player = GameObject.FindGameObjectWithTag("Player");
        col = GetComponent<Collider2D>();
        col.enabled = false;
        StartCoroutine(StartMoving());
    }

    void Update()
    {
        if (player == null) return;

        if (moving)
        {
            Vector3 targetPos = player.transform.position + offset;
            targetPos.z = transform.position.z; 
            transform.position = Vector3.Lerp(transform.position, targetPos, speed * Time.deltaTime);
        }
    }

    private IEnumerator StartMoving()
    {
        yield return new WaitForSeconds(1f);
        moving = true;
        yield return new WaitForSeconds(0.5f);
        col.enabled = true;
        yield return new WaitForSeconds(3.5f);

        if (animator != null)
        {
            animator.Play("tentaculo_escondiendose");
        }

        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject);
    }
}