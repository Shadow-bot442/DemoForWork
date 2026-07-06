using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using ILRuntime.Runtime.Enviorment;

public class ControlNetwork : Control
{
    public NetworkVariable<ulong> id = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<Vector3> respawnPoint_net = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> hp_net = new NetworkVariable<int>(100,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public NetworkVariable<int> nowBullet_net = new NetworkVariable<int>(30, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> countBullet_net = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int teamIndex;
    private ILRuntime.Runtime.Enviorment.AppDomain appDomain;

    //同步是否活着 和 是否无敌
    public NetworkVariable<bool> isAlive_net = new NetworkVariable<bool>(true,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server) ;
    public NetworkVariable<bool> isPower_net = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    //确保先同步位置后 再产生移动
    public NetworkVariable<bool> isCanMove_net = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    //客户端start会比管理器赋值先执行，即还未连接上玩家就start了
    protected override void Start()
    {
        OnAliveChange(true,isAlive_net.Value);              //进入时更新一下 确保和场景一致
        isAlive_net.OnValueChanged += OnAliveChange;        //根据网络变量设置玩家在各自场景中失活激活
        //hp_net.OnValueChanged += OnTakeDamage;              //根据网络变量设置玩家血量和死亡


        //加载ILRuntime代码更改游戏逻辑
        ILRuntimeMgr.Instance.StartILRuntime((appDomain) => {
            this.appDomain = appDomain;
            fireValue = (int)appDomain.Invoke("HotFix_Project.FireRule", "get_fireValue", null, null);
            fireInterval = (float)appDomain.Invoke("HotFix_Project.FireRule", "get_fireInterval", null, null);
        });


        if (!IsOwner)
            return;
        
        nowBullet_net.OnValueChanged += OnBulletReduce;
        id.OnValueChanged += OnIDInit;
        hp_net.OnValueChanged += OnTakeDamage;              //根据网络变量设置玩家血量和死亡
        base.Start();
        gamePanel.ShowGrade();
        gamePanel.UpdateBK_HP_net(hp_net.Value);
        UpdateGradeServerRpc();
        
    }

    //在id赋值后调用一次
    private void OnIDInit(ulong pul, ulong nul) {

        SendInfoServerRpc("玩家"+id.Value + "加入" + (teamIndex==1 ? "红队":"蓝队"));  //其他玩家展示的信息
        StartCoroutine(DelaySendInfo("你是玩家" + id.Value));                          //自己展示的信息

        if (overPanel == null) overPanel = UIManager.Instance.GetPanel<UI_OverPanel>();
        if (stopPanel == null) stopPanel = UIManager.Instance.GetPanel<UI_StopPanel>();

        overPanel.ReturnCall_net += ReturnMenu;
        stopPanel.ReturnCall_net += ReturnMenu;
    }

    protected override void Update()
    {
        if (!IsOwner)
            return;

        if (overPanel != null && stopPanel != null && gamePanel != null)
        {
            if (Input.GetKey(KeyCode.LeftAlt) || overPanel.isShow || stopPanel.isShow)      //显示鼠标
            {
                CursorShow();
                animator.SetFloat("Blend", 0f);
            }
            else
                CursorLock();
        }
        else {
            gamePanel = UIManager.Instance.GetPanel<UI_GamePanel>();
            overPanel = UIManager.Instance.GetPanel<UI_OverPanel>();
            stopPanel = UIManager.Instance.GetPanel<UI_StopPanel>();
        }
        

        if (!IsOwner || !isCanMove_net.Value || overPanel.isShow || stopPanel.isShow)    //使角色无法移动
            return;


        if (Input.GetKeyDown(KeyCode.Escape) && !stopPanel.isShow) {                     //显示停止面板
            stopPanel.ShowMe();
        }
            

        SupplyBullet();

        LocalMove();

        CameraRotateAround();

        Anim();
  
    }


    //本地方法 重生和死亡
    private void Relife() {
        Invoke("DelayRelife",0.1f);
        transform.position = respawnPoint_net.Value;
        RelifeServerRpc(id.Value);      //调用网络服务器的respawed(重生)
    }
    private void DelayRelife() {
        animator.SetTrigger("IsRelife");
    }
    new private void Death()
    {

        ObjectPoolMgr.Instance.GetObj(ObjectPoolMgr.E_ParName.FireDeath);

        if (IsOwner) {
            animator.SetTrigger("IsDeath");
            SendInfoServerRpc("玩家" + id.Value + "已死亡");    //其他玩家提示死亡信息
            StartCoroutine(DelaySendInfo("您已死亡"));          //自己提示死亡信息
            Invoke("DelaySetActiveFalse", 2f);                  //死亡2s后显示ui 失活角色
            ParServerRpc(ObjectPoolMgr.E_ParName.FireDeath, transform.position, Vector3.zero);   //广播死亡特效

            AddGradeServerRpc(teamIndex == 1 ? 2 : 1);           //自己死亡时对方比分加1
            UpdateGradeServerRpc();                             //更新所有人的比分
            SetDeathServerRpc();                             //设置网络无法移动和无敌状态(死后)
        }
        
    }

    //死亡延迟调用 绑定panel方法 设置失活
    private void DelaySetActiveFalse()
    {
        overPanel.ShowMe();
        if (overPanel == null)
            overPanel = UIManager.Instance.GetPanel<UI_OverPanel>();
        overPanel.RelifeCall_net -= Relife;                 // 先解绑防止重复
        overPanel.RelifeCall_net += Relife;
        SetAliveForFalseServerRpc();
    }




    //网络方法 重生和死亡和激活失活
    [ServerRpc]
    public void RelifeServerRpc(ulong id)
    {
        NetworkGameMgr.Instance.ReSpawner(id);
    }
    [ServerRpc]
    public void SetDeathServerRpc()
    {
        isCanMove_net.Value = false;
        isPower_net.Value = true;
    }
    [ServerRpc]
    private void SetAliveForFalseServerRpc() { 
        isAlive_net.Value = false;
    }




    //在受伤时本地调用
    private void OnTakeDamage(int pValue, int nValue)
    {
        gamePanel.UpdateBK_HP_net(nValue);
        if (nValue <= 0)
            Death();
    }

    //在消耗子弹时本地调用
    private void OnBulletReduce(int pValue, int nValue)
    {
        gamePanel.updateBulletCount(nowBullet_net.Value, countBullet_net.Value);
    }

    //在生存改变时调用设置全局激活失活(全局)
    private void OnAliveChange(bool pValue,bool nValue) {
        gameObject.SetActive(nValue);
    }




    //添加比分加1
    [ServerRpc]
    public void AddGradeServerRpc(int teamIndex)
    {          //根据teamIndex判断
        if (teamIndex == 1)
            NetworkGameMgr.Instance.grade.x++;
        else
            NetworkGameMgr.Instance.grade.y++;
    }

    //发送比分给所有客户端更新ui
    [ClientRpc()]
    public void UpdateGradeClientRpc(Vector2 grade)
    {
        if (gamePanel != null)
            gamePanel.UpdateGrade(grade);
        else
        {
            gamePanel = UIManager.Instance.GetPanel<UI_GamePanel>();
            gamePanel.UpdateGrade(grade);
        }
    }

    //通过server更新所有客户端的ui比分
    [ServerRpc()]
    public void UpdateGradeServerRpc()
    {
        UpdateGradeClientRpc(NetworkGameMgr.Instance.grade);
        
    }



    //触发换弹方法
    protected void SupplyBullet()
    {
        if (gamePanel == null) gamePanel = UIManager.Instance.GetPanel<UI_GamePanel>();

        if ((Input.GetKeyDown(KeyCode.R) || gamePanel.isSupply) && isCanUseSupply)
        {
            if (countBullet_net.Value == 0)
            {
                gamePanel.ShowInfo("子弹已耗尽 无法填充");
                return;
            }
            else if (nowBullet_net.Value == 30)
            {
                gamePanel.ShowInfo("子弹已满 无需填充");
                return;
            }
            animator.SetBool("IsSupply", true);
            isCanUseSupply = false;
            //设置换弹音乐
            ParServerRpc(ObjectPoolMgr.E_ParName.SupplyBullet_Sound,transform.position,transform.eulerAngles);
            Invoke("invokeSupply", 3.9f);
        }
    }


    //重写本地动作 触发开火
    protected override void Anim()
    {
        base.Anim();
        //检测射击
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
#elif UNITY_ANDROID
            if (gamePanel.isShoot)
#endif
        {
            //射击后将重置面板的isShoot变量
            gamePanel.isShoot = false;
            Ray ray = myCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            LocalFire(ray);
        }
    }



    //本地换弹
    protected void invokeSupply() {
        gamePanel.isSupply = false;
        isCanUseSupply = true;
        animator.SetBool("IsSupply", false);
        SupplyBulletServerRpc();
    }

    //服务器换弹 子弹数量的计算和修改
    [ServerRpc]
    public void SupplyBulletServerRpc() {
        int need = 30 - nowBullet_net.Value;
        if (need > 0)
        {
            if (countBullet_net.Value >= need)
            {
                nowBullet_net.Value = 30;
                countBullet_net.Value -= need;
            }
            else
            {
                nowBullet_net.Value += countBullet_net.Value;
                countBullet_net.Value = 0;
            }
        }
    }



    //本地开火
    void LocalFire(Ray ray)
    {
        //玩家与摄像机的夹角
        float value = Vector3.Dot(transform.forward, Quaternion.AngleAxis(myCamera.transform.eulerAngles.y, Vector3.up) * Vector3.forward);
        value = Mathf.Clamp(value, -1f, 1f);
        float deg = Mathf.Acos(value) * Mathf.Rad2Deg;

        //float deg = Mathf.Acos(Vector3.Dot(transform.forward, Vector3.ProjectOnPlane(myCamera.transform.forward, Vector3.up))) * Mathf.Rad2Deg;

        //判断是否能射击(间隔 弹药 换弹ing)
        if (Time.time - lastFireTime <= fireInterval || nowBullet_net.Value == 0 || !isCanUseSupply)
        {
            if (nowBullet_net.Value == 0)
            {
                ParServerRpc(ObjectPoolMgr.E_ParName.NoBullet_Sound, firePoint.position, Vector3.zero);
                gamePanel.ShowInfo("子弹不足");
            }
            else {
                gamePanel.ShowInfo("无法开火");
            }
            return;
        }

        //判断是否能射击(根据ILRuntime)
        if(appDomain == null)
            appDomain = ILRuntimeMgr.Instance.appDomain;
#if UNITY_ANDROID
        if ( ! (bool)appDomain.Invoke("HotFix_Project.FireRule", "CanFire_Android", null, deg))
#else
        if ( ! (bool)appDomain.Invoke("HotFix_Project.FireRule", "CanFire_PC", null, deg))
#endif
        {

            gamePanel.ShowInfo("请朝向开火方向");
            return;
        }
        
        FireServerRpc(ray.origin, ray.direction, teamIndex, firePoint.position);
        //后坐力
        cameraAngleY += UnityEngine.Random.Range(-fireBackPower.x, fireBackPower.x);
        cameraAngleX += fireBackPower.y;
        //射击时设置一些设置
        lastFireTime = Time.time;
        SetSpeedZero();
        CancelInvoke("SetSpeedNormal");
        Invoke("SetSpeedNormal", 0.25f);

    }

    ////服务器开火 计算射击和血量
    [ServerRpc()]
    public void FireServerRpc(Vector3 origin,Vector3 dire, int teamIndex,Vector3 firePoint) {

        //发送开枪特效 更新子弹和ui子弹
        //gamePanel.updateBulletCount(--nowBullet, countBullet);
        ParClientRpc(ObjectPoolMgr.E_ParName.Fire, firePoint,new Vector3(0,dire.y,0) );

        nowBullet_net.Value--;
        RaycastHit hitInfo;

        //根据teamIndex设置检测层级
        int layerRange = 1 << LayerMask.NameToLayer("Map") | (teamIndex==1 ? 1 << LayerMask.NameToLayer("BlueTeam") : 1 << LayerMask.NameToLayer("RedTeam"));
        
        if (Physics.Raycast(origin,dire, out hitInfo, 100f, layerRange ))
        {
            if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("RedTeam") && !hitInfo.collider.GetComponent<ControlNetwork>().isPower_net.Value)
            {
                hitInfo.collider.GetComponent<ControlNetwork>().hp_net.Value -= fireValue;
                ParClientRpc(ObjectPoolMgr.E_ParName.HitPeople, hitInfo.point, hitInfo.normal);
            }
            else if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("BlueTeam") && !hitInfo.collider.GetComponent<ControlNetwork>().isPower_net.Value)
            {
                hitInfo.collider.GetComponent<ControlNetwork>().hp_net.Value -= fireValue;
                ParClientRpc(ObjectPoolMgr.E_ParName.HitPeople, hitInfo.point, hitInfo.normal);
            }
            else {
                ParClientRpc(ObjectPoolMgr.E_ParName.HitDecal, hitInfo.point, hitInfo.normal);
            }
        }
        
    }



    //发送特效给客户端
    [ClientRpc()]
    public void ParClientRpc(ObjectPoolMgr.E_ParName e_Par, Vector3 pos, Vector3 dire) {
        GameObject obj = ObjectPoolMgr.Instance.GetObj(e_Par);
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.LookRotation(dire);
    }
    //服务器包装发送特效给客户端
    [ServerRpc()]
    public void ParServerRpc(ObjectPoolMgr.E_ParName e_Par, Vector3 pos, Vector3 dire)
    {
        ParClientRpc(e_Par,pos,dire);
    }

    //发送全局消息 服务器包装
    [ServerRpc(RequireOwnership = false)]
    public void SendInfoServerRpc(string msg, ServerRpcParams rpcParams = default) {
        // 可在此基于 rpcParams.Receive.SenderClientId 做进一步验证或记录
        SendInfoClientRpc(msg);
    }
    [ClientRpc]
    public void SendInfoClientRpc(string msg)
    {
        UIManager.Instance.ShowInfo(msg);
    }

    //本地延迟设置自己的消息
    IEnumerator DelaySendInfo(string msg)
    {
        yield return new WaitForSeconds(0.1f);
        UIManager.Instance.ShowInfo(msg);
    }


    #region 移动逻辑
    //本地预测移动
    private void LocalMove()
    {

        //win控制输入
#if UNITY_EDITOR || UNITY_STANDALONE
        InputVec3 = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //安卓控制输入
#elif UNITY_ANDROID
        InputVec3 = new Vector3(moveRect.anchoredPosition.x, 0, moveRect.anchoredPosition.y).normalized;
#endif
        ccMove(InputVec3);
        MoveServerRpc(InputVec3);
    }


    //发送给服务器权威移动
    [ServerRpc()]
    public void MoveServerRpc(Vector3 input)
    {
        ccMove(input);
    }
    #endregion



    // 按钮点击（客户端执行）
    public void ReturnMenu()
    {
        if (!IsOwner) return;
        CursorShow();
        SendInfoServerRpc("玩家" + id.Value + "退出游戏");                //其他玩家提示信息
        UIManager.Instance.ShowInfo("您已退出游戏");                      //自己提示信息

        // 客户端向服务器发送Despawn请求
        Invoke("DelayDespawn",0.1f);
    }

    void DelayDespawn() {
        ReturnMenuServerRpc(id.Value);
    }

    // 发给服务器执行
    [ServerRpc(RequireOwnership = false)]
    private void ReturnMenuServerRpc(ulong id)
    {
        NetworkGameMgr.Instance.spawnedPlayers[id].GetComponent<NetworkObject>().Despawn();
    }
}
