using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkGameMgr : NetworkBehaviour
{
    static NetworkGameMgr instance;
    public static NetworkGameMgr Instance => instance;
    public Transform redRoot;
    public Transform blueRoot;
    public GameObject perfab_1;
    public GameObject perfab_2;


    public Vector2 grade = Vector2.zero;

    public Dictionary<ulong, GameObject> spawnedPlayers = new Dictionary<ulong, GameObject>();

    //人数 x表示红队，y表示蓝队
    private Vector2 count;

    private Dictionary<ulong, Transform> pointsBeTaked_Blue;
    private Dictionary<ulong, Transform> pointsBeTaked_Red;

    private void Awake()
    {
        if (ConnectManager.Instance.ConnectState == ConnectManager.E_ConnectState.Client)
        {
            if (!NetworkManager.Singleton.StartClient()) { 
                UIManager.Instance.ShowInfo("客户端连接失败");
                UIManager.Instance.GetPanel<UI_GamePanel>().ShowInfo(ConnectManager.Instance.IPAddress);
            }
            
            Destroy(gameObject);
            return;
        }

        //if( ConnectManager.Instance.ConnectState == ConnectManager.E_ConnectState.Server)
        else
        {
            //添加回调函数和开启客户端连接
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnRole;
            NetworkManager.Singleton.OnClientDisconnectCallback += DeleteRole;
        }


        //初始化各个值
        instance = this;
        pointsBeTaked_Blue = new Dictionary<ulong, Transform>();
        pointsBeTaked_Red = new Dictionary<ulong, Transform>();
        count = Vector2.zero;
    }

    //生成角色
    private void SpawnRole(ulong id) {
        if (count.x + count.y >= 10)
        {
            UIManager.Instance.ShowInfo("房间已满，无法加入: " + id);
            return;
            //提示房间已满,并返回主菜单
        }

        

        int teamNum = DistributeTeam();
        Transform pos = GetIdlePos(teamNum);
        if (pos == null)
        {
            UIManager.Instance.ShowInfo("未找到空闲出生点，无法生成玩家: " + id);
            return;
        }

        GameObject player = teamNum == 1 ? Instantiate<GameObject>(perfab_1) : Instantiate<GameObject>(perfab_2);
        player.transform.position = pos.position;
        player.transform.rotation = pos.rotation;


        //添加位置和角色数量
        if (teamNum == 1)
        {
            count.x += 1;
            pointsBeTaked_Red.Add(id, pos);
        }
        else {
            count.y += 1;
            pointsBeTaked_Blue.Add(id, pos);
        }

        //生成玩家权限 更新玩家变量
        player.GetComponent<NetworkObject>().SpawnWithOwnership(id, true);
        ControlNetwork controlNetwork = player.GetComponent<ControlNetwork>();
        controlNetwork.respawnPoint_net.Value = pos.position;
        controlNetwork.hp_net.Value = 100;
        controlNetwork.countBullet_net.Value = 100;
        controlNetwork.nowBullet_net.Value = 30;
        controlNetwork.id.Value = id;
        controlNetwork.isPower_net.Value = true;
        controlNetwork.isAlive_net.Value = true;
        StartCoroutine(CancelIsPower(controlNetwork,2f));
        StartCoroutine(SetCanMove(controlNetwork,1f));
        spawnedPlayers.Add(id,player);
        UIManager.Instance.ShowInfo("已为客户端生成玩家: " + id + " 队伍:" + (teamNum==1?"红":"蓝"));
    }

    //重生
    public void ReSpawner(ulong id)
    {
        ControlNetwork cn = spawnedPlayers[id].GetComponent<ControlNetwork>();
        if (cn == null) return;
        cn.hp_net.Value = 100;
        cn.countBullet_net.Value = 100;
        cn.isAlive_net.Value= true;
        cn.nowBullet_net.Value = 30;
        cn.isPower_net.Value = true;
        StartCoroutine(CancelIsPower(cn, 2f));
        StartCoroutine(SetCanMove(cn, 2f));
    }

    IEnumerator CancelIsPower(ControlNetwork cn,float time) { 
        yield return new WaitForSeconds(time);
        cn.isPower_net.Value = false;
    }
    IEnumerator SetCanMove(ControlNetwork cn, float time)
    {
        yield return new WaitForSeconds(time);
        cn.isCanMove_net.Value = true;
    }

    //删除角色
    private void DeleteRole(ulong id)
    {
        // 从字典中移除引用并更新计数
        spawnedPlayers.Remove(id);

        if (pointsBeTaked_Blue.ContainsKey(id))
        {
            pointsBeTaked_Blue.Remove(id);
            count.y -= 1;
        }
        if (pointsBeTaked_Red.ContainsKey(id))
        {
            pointsBeTaked_Red.Remove(id);
            count.x -= 1;
        }
        UIManager.Instance.ShowInfo("玩家已断开: " + id);

    }

    //get空闲位置
    private Transform GetIdlePos(int teamNum) {
        if (teamNum == 1)
        {
            for (int i = 0; i < redRoot.childCount; i++) {
                Transform child = redRoot.GetChild(i).transform;
                if (!pointsBeTaked_Red.ContainsValue(child))
                    return child;
            }
        }
        else{
            for (int i = 0; i < blueRoot.childCount; i++)
            {
                Transform child = blueRoot.GetChild(i).transform;
                if (!pointsBeTaked_Blue.ContainsValue(child))
                    return child;
            }
        }

        return null;
    }

    //分配角色方队
    private int DistributeTeam() {
        if (count.x > count.y)
        {
            return 2;
        }
        else
            return 1;
    }

    

    public override void OnDestroy()
    {
        if (ConnectManager.Instance.ConnectState == ConnectManager.E_ConnectState.Server) {
            pointsBeTaked_Blue.Clear();
            pointsBeTaked_Red.Clear();
            pointsBeTaked_Blue = null;
            pointsBeTaked_Red = null;
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnRole;
            NetworkManager.Singleton.OnClientDisconnectCallback -= DeleteRole;
        }
        
    }

}
