using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectManager : MonoBehaviour
{
    public enum E_ConnectState
    {
        Server,
        Client,
    }
    private static ConnectManager instance;
    public static ConnectManager Instance => instance;
    public E_ConnectState ConnectState { get; set; }
    public string IPAddress { get; set; }
    public ushort Port { get; set; }

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ConnectAsServer(ushort port) {
        IPAddress = "";
        Port = port;
        ConnectState = E_ConnectState.Server;
    }
    public void ConnectAsClient(string ip, ushort port) {
        IPAddress = ip;
        Port = port;
        ConnectState = E_ConnectState.Client;
    }
}
