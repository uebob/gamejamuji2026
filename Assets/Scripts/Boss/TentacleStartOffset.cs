using UnityEngine;
using System.Collections;
using NUnit.Framework;

public class TentacleStartOffset : MonoBehaviour
{
    private Animator anim;
    public GameObject boss;
    private bool isLoco = false;
    void Start()
    {
        anim = GetComponent<Animator>();
        // La velocidad será entre 90% y 110% de la original
        anim.speed = Random.Range(0.9f, 1.1f);
    }

    void Update()
    {
        if (boss.GetComponent<BossController>().estadoActual == BossController.EstadoBoss.RecibiendoDano || 
        boss.GetComponent<BossController>().estadoActual == BossController.EstadoBoss.Muerto )
        {
            if(!isLoco) StartCoroutine(VolverseLoco());
        }
    }

    public IEnumerator VolverseLoco()
    {
        isLoco = true;
        anim.speed *= 4;
        yield return new WaitForSeconds(0.8f);
        anim.speed *= 0.25f;
        isLoco = false;
    }

}
