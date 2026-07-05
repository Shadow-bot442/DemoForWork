using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum E_State { 
    Run,Walk,Crouch,Prone
}

public class Control : NetworkBehaviour
{

    //基础信息
    public int hp = 100;
    protected int sceneIndex;

    //其他组件
    public Camera myCamera;
    public CharacterController cc;
    public Animator animator;

    //摄像机相关速度
    public float scrollSpeed;
    public float cameraSpeed;
    public float cameraSpeed_Ador;
    public float rotateSpeed;
    protected float cameraAngleY;
    protected float cameraAngleX;
    public Vector3 cameraPos;
    protected float cameraView;

    //调试速度
    public float MaxSpeed;
    public float CrouchSpeed;
    public float ProneSpeed;
    public float JumpSpeed;
    public float JumpDownSpeed = 5;
    protected float nowJumpSpeed = 0;

    [Header("射击相关")]
    public Transform firePoint;
    public GameObject fireParticle;
    public GameObject hitPeopleParticle;
    public GameObject hitMapParticle;
    public GameObject deathParticle;
    public GameObject supplyMusic;
    public GameObject noBulletMusic;
    public Vector2 fireBackPower;
    public float fireInterval = 0.5f;
    public int fireValue = 20;
    protected float lastFireTime = 0;
    public int countBullet = 100;
    public int nowBullet = 30;
    public GameObject takeDamageMusic;

    //临时变量
    protected Vector3 InputVec3;
    protected float nowMoveSpeed;
    protected RectTransform moveRect;
    protected float angleY;


    //当前状态
    public E_State NowState = E_State.Run;
    //是否在跳跃
    protected bool isJump;
    //获取面板
    protected UI_GamePanel gamePanel;
    protected UI_OverPanel overPanel;
    protected UI_StopPanel stopPanel;

    protected virtual void Start()
    {
        sceneIndex = SceneManager.GetActiveScene().buildIndex;

        myCamera = Camera.main;
        Application.targetFrameRate = 120;
        gamePanel = UIManager.Instance.GetPanel<UI_GamePanel>();
        overPanel = UIManager.Instance.GetPanel<UI_OverPanel>();
        stopPanel = UIManager.Instance.GetPanel<UI_StopPanel>();
        cameraAngleX = myCamera.transform.eulerAngles.x;
        cameraAngleY = myCamera.transform.eulerAngles.y;
        cameraView = myCamera.fieldOfView;
        UIManager.Instance.ShowPanel<UI_GamePanel>();
        moveRect = UIManager.Instance.GetPanel<UI_GamePanel>().btnMove.GetComponent<RectTransform>();
        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        //UI相关初始化
        if (sceneIndex == 2)
            gamePanel.updateBulletCount(nowBullet, countBullet);
    }

    protected virtual void Update()
    {
        if (overPanel.isShow || stopPanel.isShow) {
            CursorShow();
            Time.timeScale = 0;
            return;
            
        }
        else {
            CursorLock();
            Time.timeScale = 1;
        }
            
        

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            CursorShow();
        }
        else
        {
            CursorLock();
        }


        if (Input.GetKeyDown(KeyCode.Escape) && !stopPanel.isShow)
        {
            UIManager.Instance.ShowPanel<UI_StopPanel>();
        }
            


        Move();
        CameraRotateAround();
        CameraFieldOfView();
        
        Anim();
        SupplyBullet();
    }

    protected bool isCanUseSupply = true;

    

    protected void CursorLock() {
        if (!Cursor.visible) return;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    protected void CursorShow()
    {
        if (Cursor.visible) return;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void SupplyBullet() {
        if ( (Input.GetKeyDown(KeyCode.R) || gamePanel.isSupply) && isCanUseSupply) {
            if (countBullet == 0)
            {
                gamePanel.ShowInfo("子弹已耗尽 无法填充");
                return;
            }
            else if (nowBullet == 30) {
                gamePanel.ShowInfo("子弹已满 无需填充");
                return;
            }
            animator.SetBool("IsSupply",true);
            isCanUseSupply = false;
            GameObject.Instantiate<GameObject>(supplyMusic,transform.position,transform.rotation);
            Invoke("invokeSupply", 3.9f);
        }
    }
    private void invokeSupply(){
        gamePanel.isSupply = false;
        isCanUseSupply = true;

        int need = 30 - nowBullet;
        if (need > 0)
        {
            if (countBullet >= need)
            {
                nowBullet = 30;
                countBullet -= need;
            }
            else
            {
                nowBullet += countBullet;
                countBullet = 0;
            }
        }
        animator.SetBool("IsSupply", false);
        gamePanel.updateBulletCount(nowBullet, countBullet);
    }
    protected virtual void Anim() {

        if (animator != null && cc != null) { 

            //设置当前animator速度匹配cc
            //animator.SetFloat("Blend", cc.velocity.magnitude);

            //当跳跃时不检测任何行为
            int stateNameIndex1 = Animator.StringToHash("Pistol Jump (1)");
            int stateNameIndex2 = Animator.StringToHash("Pistol Jump");
            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateNameIndex1 || animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateNameIndex2)
                return;

            //当换弹时不检测任何行为
            if (!isCanUseSupply) return;

            if (sceneIndex == 2) {
                //检测射击
#if UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetMouseButtonDown(0))
#elif UNITY_ANDROID
            if (gamePanel.isShoot)
#endif
                {
                    gamePanel.isShoot = false;
                    Fire();
                }
            }
            

            //冲刺和步行切换检测
            if (Input.GetKeyDown(KeyCode.LeftShift) || gamePanel.isSlow ) {

                gamePanel.isSlow = false;

                if (NowState == E_State.Run)
                {
                    NowState = E_State.Walk;
                }
                else if (NowState == E_State.Walk) {
                    NowState = E_State.Run;
                }
            }

            //下蹲检测
            if (Input.GetKeyDown(KeyCode.LeftControl) || gamePanel.isCrouch) {

                gamePanel.isCrouch = false;

                NowState = (NowState == E_State.Crouch) ? E_State.Run : E_State.Crouch;
                animator.SetBool("Crouch", animator.GetBool("Crouch") ? false : true );
                animator.SetBool("IsProne", false);
            }

            //趴下检测
            if (Input.GetKeyDown(KeyCode.Z) || gamePanel.isProne) {

                gamePanel.isProne = false;

                NowState = (NowState == E_State.Prone) ? E_State.Run : E_State.Prone;
                animator.SetBool("IsProne", animator.GetBool("IsProne") ? false : true);
                animator.SetBool("Crouch", false);
            }
            
            //跳跃检测
            if (Input.GetKeyDown(KeyCode.Space) || gamePanel.isJump)
            {
                gamePanel.isJump = false;

                if (NowState == E_State.Prone)
                {
                    NowState = E_State.Run;
                    animator.SetBool("IsProne", false);
                    return;
                }
               
                animator.SetTrigger("Jump");
                NowState = E_State.Run;
                animator.SetBool("Crouch",false);
                
            }
        }
    }

    protected virtual void Move()
    {

        //win控制输入
#if UNITY_EDITOR || UNITY_STANDALONE
        InputVec3 = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //安卓控制输入
#elif UNITY_ANDROID
        InputVec3 = new Vector3(moveRect.anchoredPosition.x, 0, moveRect.anchoredPosition.y).normalized;
#endif
        ccMove(InputVec3);
    }

    protected void ccMove(Vector3 inputVec3) {
        nowMoveSpeed = 0;
        angleY = 0;

        if (inputVec3 != Vector3.zero)
        {
            if (inputVec3.x > 0)
                angleY = Mathf.Acos(Vector3.Dot(Vector3.forward, inputVec3.normalized)) * Mathf.Rad2Deg;
            else
                angleY = Mathf.Acos(Vector3.Dot(Vector3.forward, inputVec3.normalized)) * -Mathf.Rad2Deg;
        }

        //设置当前速度
        switch (NowState)
        {
            case E_State.Run:
                nowMoveSpeed = MaxSpeed * Mathf.Max(Mathf.Abs(inputVec3.z), Mathf.Abs(inputVec3.x));
                break;
            case E_State.Walk:
                nowMoveSpeed = CrouchSpeed * Mathf.Max(Mathf.Abs(inputVec3.z), Mathf.Abs(inputVec3.x));
                break;
            case E_State.Crouch:
                nowMoveSpeed = CrouchSpeed * Mathf.Max(Mathf.Abs(inputVec3.z), Mathf.Abs(inputVec3.x));
                break;
            case E_State.Prone:
                nowMoveSpeed = ProneSpeed * Mathf.Max(Mathf.Abs(inputVec3.z), Mathf.Abs(inputVec3.x));
                break;
        }

        //角色控制器 位移
        if (cc != null)
        {
            animator.SetBool("IsGround", cc.isGrounded);

            if (!isJump && nowJumpSpeed != 0f)
            {
                nowJumpSpeed -= Time.deltaTime * JumpDownSpeed;
            }

            cc.Move(this.transform.forward * nowMoveSpeed * Time.deltaTime + Vector3.up * (nowJumpSpeed - 10) * Time.deltaTime);
            //设置当前animator速度匹配cc
            animator.SetFloat("Blend", nowMoveSpeed);
        }
    }

    protected virtual void CameraRotateAround() {

#if UNITY_EDITOR || UNITY_STANDALONE
        //算出累计环绕旋转的四元数
        cameraAngleY += Input.GetAxisRaw("Mouse X") * cameraSpeed;
        cameraAngleX -= Input.GetAxisRaw("Mouse Y") * cameraSpeed;

#elif UNITY_ANDROID

//判断是否接触ui元素
        for (int i = 0; i < Input.touchCount; i++) {
            Touch touch = Input.GetTouch(i);
            if (!UIManager.Instance.IsOverUI(touch.position) && touch.phase == TouchPhase.Moved) {

                //算出累计环绕旋转的四元数 

                cameraAngleY += touch.deltaPosition.x * cameraSpeed_Ador;
                cameraAngleX -= touch.deltaPosition.y * cameraSpeed_Ador;
            }
        }
#endif

        cameraAngleX = Mathf.Clamp(cameraAngleX,-45,35);
        Quaternion quaternion = Quaternion.Euler(cameraAngleX, cameraAngleY, 0);
        myCamera.transform.rotation = quaternion;
        transform.eulerAngles = new Vector3(0, cameraAngleY + angleY, 0);
        myCamera.transform.position = Quaternion.AngleAxis(-angleY, Vector3.up) * (transform.right * cameraPos.x + transform.up * cameraPos.y + transform.forward * cameraPos.z) + transform.position;

        //新位置 = 位置*四元数    and       看向玩家
        //Vector3 targetPos = this.transform.position + quaternion * cameraPos;
        //myCamera.transform.position = targetPos;
        //myCamera.transform.LookAt(this.transform.position + myCamera.transform.up *2f + myCamera.transform.right * 0.5f);
    }

    private void CameraFieldOfView()
    {
        cameraView -= Input.mouseScrollDelta.y * scrollSpeed;
        cameraView = Mathf.Clamp(cameraView, 35, 65);
        myCamera.fieldOfView = cameraView;
    }

    private void Fire()
    {

        //判断是否能射击
        if (Time.time - lastFireTime <= fireInterval || nowBullet == 0 || !isCanUseSupply || isJump)
        {
            if (nowBullet == 0)
            {
                gamePanel.ShowInfo("没有子弹啦");
                GameObject.Instantiate<GameObject>(noBulletMusic);
                return;
            }
            gamePanel.ShowInfo("无法射击");
            return;
        }
        Vector3 dirCameraUp = Quaternion.AngleAxis(myCamera.transform.eulerAngles.y, Vector3.up) * Vector3.forward;
        if (Vector3.Dot(transform.forward, dirCameraUp) <= 0)
        {
            gamePanel.ShowInfo("射击时请面向摄像机方向");
            return;
        }

        lastFireTime = Time.time;
        SetSpeedZero();
        CancelInvoke("SetSpeedNormal");
        Invoke("SetSpeedNormal", 0.2f);
        gamePanel.updateBulletCount(--nowBullet, countBullet);
        GameObject.Instantiate<GameObject>(fireParticle, firePoint);

        RaycastHit hitInfo;
        if (Physics.Raycast(myCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height) / 2), out hitInfo, 1000f, 1 << LayerMask.NameToLayer("Player")
                                                                                                | 1 << LayerMask.NameToLayer("Map") | 1 << LayerMask.NameToLayer("Monster")))
        {
            transform.rotation = myCamera.transform.rotation;
            if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                hitInfo.collider.GetComponent<Control>().TakeDamage(fireValue);
                GameObject.Instantiate<GameObject>(hitPeopleParticle, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            }
            else if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Map"))
            {

                GameObject.Instantiate<GameObject>(hitMapParticle, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            }
            else
            {
                if (hitInfo.transform.GetComponent<EnemyControl>() != null)
                {
                    GameObject.Instantiate<GameObject>(hitPeopleParticle, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                    hitInfo.transform.GetComponent<EnemyControl>().TakeDamage(fireValue);
                }


            }
        }
        //后坐力
        cameraAngleY += UnityEngine.Random.Range(-fireBackPower.x, fireBackPower.x);
        cameraAngleX += fireBackPower.y;


    }

    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
        hp = Mathf.Clamp(hp, 0, 100);
        gamePanel.updateBK_HP(-damage);
        Instantiate<GameObject>(takeDamageMusic, transform.position, Quaternion.identity);
        if (hp == 0) Death();
    }

    public void Death()
    {
        animator.SetTrigger("IsDeath");
        GameObject.Instantiate<GameObject>(deathParticle, transform.position, transform.rotation);
        UIManager.Instance.HidePanel<UI_GamePanel>();
        Destroy(gameObject, 2f);
        Destroy(this);
        UIManager.Instance.ShowPanel<UI_OverPanel>();
    }

    //设置某些动作禁用速度
    public void WhenJumpStart() { 
        MaxSpeed = 1;
        CrouchSpeed = 1;
        ProneSpeed = 1;
        JumpStart();
    }

    public void WhenJumpEnd() {
        MaxSpeed = 7f;
        CrouchSpeed = 2.5f;
        ProneSpeed = 1;
        
    }

    public void SetSpeedZero() {
        MaxSpeed = 0;
        CrouchSpeed = 0;
        ProneSpeed = 0;
    }
    public void SetSpeedNormal() {
        MaxSpeed = 7f;
        CrouchSpeed = 2.5f;
        ProneSpeed = 1;
    }

    public void JumpEnd() { 
        isJump = false;
    }

    public void JumpStart(){ 
        isJump = true;
        nowJumpSpeed = JumpSpeed;
    } 

    public void SetCrouchCC()
    {
        if (cc != null)
        {
            cc.center = Vector3.up * 0.7f;
            cc.height = 1.4f;
        }
    }

    public void SetProneCC()
    {
        if (cc != null)
        {
            cc.center = Vector3.up * 0.5f;
            cc.height = 1;
        }
    }
    public void SetStandCC()
    {
        if (cc != null)
        {
            cc.center = Vector3.up;
            cc.height = 2;
        }
    }

    public override void OnDestroy()
    {
        CursorShow();
    }


}
