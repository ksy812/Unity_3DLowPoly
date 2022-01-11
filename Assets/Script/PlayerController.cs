using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;

    [SerializeField]
    private float jumpForce;

    //상태 변수
    private bool isWalk = false;
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;

    //움직임 체크 변수
    private Vector3 lastPos;

    //앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    //땅 착지 여부
    private CapsuleCollider capsuleCollider;

    //카메라 민감도
    [SerializeField]
    private float lookSensitivity;

    //카메라 각도 제한
    [SerializeField]
    private float cameraRatationLimit; //제한
    private float currentCameraRotationX = 0; //정면. 생략해도 기본값이 0임.

    //필요한 컴포넌트
    [SerializeField]
    private Camera theCamera; //카메라 컴포넌트 불러옴. 직렬화함. 왜? 카메라객체는 플레이어객체에 있는게 아님. 걔 자식한테 있음.
    private Rigidbody myRigid;
    private GunController theGunController;
    private Crosshair theCrosshair;


    // Start is called before the first frame update
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        //theCamera = FindObjectOfType<Camera>(); //모든 객체 뒤져서 카메라 컴포넌트 있으면 넣어줌. 여러개 있으면? 별로임.. 그러니까 시리얼라이즈필드 쓰겠다고~
        myRigid = GetComponent<Rigidbody>(); // 9문장(private Rigidbody myRigid;) 위에 [SerializeField]써 인스펙터창에서 옮기는 것과 같은 효과
        theGunController = FindObjectOfType<GunController>();
        theCrosshair = FindObjectOfType<Crosshair>();

        //초기화
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y; //originPosY = transform.position.y;이 아님!! 얘가 바뀌면 플레이어가 땅에 꺼짐.
        //현재 카메라는 플레이어에 속해있음. 상대적인 좌표 사용을 위해 position이 아닌 localPosition 사용
        applyCrouchPosY = originPosY;
    }

    // Update is called once per frame
    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        MoveCheck();
        CameraRotation();
        CharacterRotation();
    }


    //앉기 시도
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }
    //실제로 앉는 함수
    private void Crouch()
    {
        isCrouch = !isCrouch;
        theCrosshair.CrouchingAnimation(isCrouch);
        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
        StartCoroutine(CrouchCoroutine()); //위에거를 조금 더 자연스러운 모션으로 하기 위함!
    }
    //부드럽게 앉는 동작
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;
        while(_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15) break; //이 조건문이 없다면 계속 실행함
            yield return null; //null=한 프레임 대기
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
    }
    private void IsGround()
    {
        //여기서 -transfrom.up 을 쓰게 된다면 문제가 생김. 고정된 값인 벡터를 쓰자!
        //(이 위치에서, 어느 방향으로, 이 거리만큼)
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        theCrosshair.JumpingAnimation(!isGround);
    }
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }
    private void Jump()
    {
        if (isCrouch)
            Crouch(); //앉은 상태에서 점프하면 앉은 상태 해제
        myRigid.velocity = transform.up * jumpForce;
    }
    //달리기 시도
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }
    //달리기 실행
    private void Running()
    {
        if (isCrouch) Crouch();

        theGunController.CancelFineSight();

        isRun = true;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = runSpeed;
    }
    //달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;
    }

    //움직임 실행
    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal"); //입력이 일어나면 -1(왼), 0(입력x), 1(오) 리턴됨. Horizontal은 유니티 기본 제공
        float _moveDirZ = Input.GetAxisRaw("Vertical"); //입력이 일어나면 -1(뒤), 0(입력x), 1(앞) 리턴됨

        Vector3 _moveHorizontal = transform.right * _moveDirX; //transform? 컴포넌트(인스펙터 창 상단에 위치)가 가지고있는 위치값 등에 라이트를 쓰겠대.
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed; //방향에 속도 곱해줌. 가는 방향 구해준거임 대충
        //normalized 사용 이유? 좌표평면 그려봐라. xz(1, 1), xz(0.5, 0.5)는 같음. 정규화 위함.

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
        //Time.deltaTime(약 0.016) 사용 이유? 1초동안 _velocity만큼 이동시키기 위함. 그러지 않으면 순간이동급임
    }
    private void MoveCheck()
    {
        if (!isRun && !isCrouch && isGround)
        {
            if (Vector3.Distance(lastPos, transform.position) >= 0.01f) isWalk = true;
            else isWalk = false;
            lastPos = transform.position;

            theCrosshair.WalkingAnimation(isWalk);
        }
        
    }
    private void CameraRotation()
    {
        //상하 카메라 회전
        float _xRotation = Input.GetAxisRaw("Mouse Y"); //1, -1 리턴됨
        float _cameraRotationX = _xRotation * lookSensitivity; //민감도를 통해 화면이 천천히 움직이도록 설정
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRatationLimit, cameraRatationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }
    private void CharacterRotation()
    {
        //좌우 캐릭터 회전
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
        //내부에서는 쿼터늄(4원소) 값으로 이루어짐. 우리가 보기 편하게 오일러(3원소) 값임.ㅈㅍ 들리는대로 써서 이름 이거 아닐수도.
    }
}
