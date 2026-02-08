using UnityEngine;
using System.Collections;

public class FollowingTentacle : MonoBehaviour
{
    private GameObject player;
    
    [Header("Configuración")]
    public float speed = 5f;   
    public Vector3 offset;     
    private bool moving = false;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
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
        yield return new WaitForSeconds(0.5f);
        moving = true;
    }
}