using UnityEngine;

public class FollowingTentacle : MonoBehaviour
{
    public GameObject player;
    
    [Header("Configuración")]
    public float speed = 5f;   
    public Vector3 offset;        

    void Update()
    {
        if (player == null) return;

        Vector3 targetPos = player.transform.position + offset;
        targetPos.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, targetPos, speed * Time.deltaTime);
    }
}