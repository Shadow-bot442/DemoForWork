using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainPanel : UI_BasePanel
{
    public Button btnAlone;
    public Button btnFriend;
    public Button btnQuit;

    public override void Init()
    {
        if (ConnectManager.Instance != null)
        {
            //初始化网络事件和配置网络接口 并且开启服务器
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.SetConnectionData(ConnectManager.Instance.IPAddress, ConnectManager.Instance.Port);
            if (ConnectManager.Instance.ConnectState.Equals(ConnectManager.E_ConnectState.Server))
            {
                transport.SetConnectionData("0.0.0.0", ConnectManager.Instance.Port);
                NetworkManager.Singleton.StartServer();
            }
        }


        btnAlone.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(2);
            HideMe();
            if(UIManager.Instance.Dics.ContainsKey("UI_GamePanel")) 
                UIManager.Instance.ShowPanel<UI_GamePanel>();
            UIManager.Instance.ShowInfo("目标:击败两只魔王");
        });


        btnFriend.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(3);
            HideMe();
            if (UIManager.Instance.Dics.ContainsKey("UI_GamePanel"))
                UIManager.Instance.ShowPanel<UI_GamePanel>();
        });


        btnQuit.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        //如果是服务器直接加载多人游戏场景
#if UNITY_SERVER
    SceneManager.LoadScene(3);
#endif
    }



    private void OnClientConnected(ulong id) {
        UIManager.Instance.ShowInfo("你的id是 " + id);
    }

    private void OnServerStarted()
    {
        if(NetworkManager.Singleton.IsServer)
            UIManager.Instance.ShowInfo("服务器已开启");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null)
            return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.Shutdown();
    }

    public override void ShowMe()
    {
        base.ShowMe();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
