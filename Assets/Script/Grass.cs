using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    //풀 체력(보통 1)
    [SerializeField]
    private int hp;

    //이펙트 제거 시간
    [SerializeField]
    
    private float destroyTime;
    //폭발력 세기
    [SerializeField]
    private float force;

    //타격 효과
    [SerializeField]
    private GameObject go_hit_effect_prefab;

    private Rigidbody[] rigidbodys; //영상에서 스펠링 틀린거 꽤 불편함;
    private BoxCollider[] boxColliders;

    [SerializeField]
    private string hit_sound;

    // Start is called before the first frame update
    void Start()
    {
        rigidbodys = this.transform.GetComponentsInChildren<Rigidbody>();
        boxColliders = transform.GetComponentsInChildren<BoxCollider>();
    }
    
    public void Damage()
    {
        hp--;

        Hit();
        if(hp <= 0)
        {
            Destruction();
        }
    }

    private void Hit()
    {
        SoundManager.instance.PlaySE(hit_sound);
        var clone = Instantiate(go_hit_effect_prefab, transform.position + Vector3.up, Quaternion.identity);
        Destroy(clone, destroyTime);
    }
    private void Destruction()
    {
        for (int i = 0; i < rigidbodys.Length; i++)
        {
            rigidbodys[i].useGravity = true;
            rigidbodys[i].AddExplosionForce(force, transform.position, 1f);
            boxColliders[i].enabled = true;
        }
        Destroy(this.gameObject, destroyTime);
    }
}
