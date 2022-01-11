using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//추상메소드가 하나 이상 있기 떄문에 추상클래스로 변경해줌~
public abstract class CloseWeaponController : MonoBehaviour
{
    //현재 장착된 Hand형 타입 무기(사실 무기는 아님)
    [SerializeField]
    protected CloseWeapon currentCloseWeapon;

    //공격 중?
    private bool isAttack = false;
    protected bool isSwing = false;

    protected RaycastHit hitInfo;

    /*추상클래스이기 때문에 Update() 지워야 함 => 자식 클래스에 넣어줌
    // Update is called once per frame
    void Update()
    {
        if (isActivate) TryAttack();
    }
    */
    protected void TryAttack()
    {
        if (Input.GetButton("Fire1"))
        {
            if (!isAttack)
            {
                //코루틴 실행
                StartCoroutine(AttackCoroutine());

            }
        }
    }
    protected IEnumerator AttackCoroutine()
    {
        isAttack = true;
        currentCloseWeapon.anim.SetTrigger("Attack");

        yield return new WaitForSeconds(currentCloseWeapon.attackDelayA);
        isSwing = true;

        //공격 활성화 시점
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentCloseWeapon.attackDelayB);
        isSwing = false;

        yield return new WaitForSeconds(currentCloseWeapon.attackDelay - currentCloseWeapon.attackDelayA - currentCloseWeapon.attackDelayB);
        isAttack = false;
    }
    /* HitCoroutine()을 상속받은 클래스에서 구현할 수 있도록 변경
    protected IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if (CheckObject())
            {
                //충돌했음
                isSwing = false;
                Debug.Log(hitInfo.transform.name);
            }
            yield return null;
        }
    }
    */
    protected abstract IEnumerator HitCoroutine();
    protected bool CheckObject()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentCloseWeapon.range))
        {

            return true;
        }
        return false;
    }
    //가상함수. 완성 함수이지만 추가 현집이 가능한 함수
    public virtual void CloseWeaponChange(CloseWeapon _closeWeapon)
    {
        if (WeaponManager.currentWeapon != null)
            WeaponManager.currentWeapon.gameObject.SetActive(false);

        currentCloseWeapon = _closeWeapon;
        WeaponManager.currentWeapon = currentCloseWeapon.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentCloseWeapon.anim;

        currentCloseWeapon.transform.localPosition = Vector3.zero;
        currentCloseWeapon.gameObject.SetActive(true);
    }
}
