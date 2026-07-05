using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_StopPanel : UI_BasePanel
{
    public Button btnGoOn;
    public Button btnReturn;

    public UnityAction ReturnCall_net;
    public override void Init()
    {
        btnGoOn.onClick.AddListener(GoOn);
        btnReturn.onClick.AddListener(Return);
    }

    private void GoOn() {
        HideMe();
    }

    private void Return() {

        if (SceneManager.GetActiveScene().name != "2_Test") {
            ReturnCall_net?.Invoke();
            Invoke("DelayShutdown", 0.3f);
            Invoke("DelayLoadScene", 0.5f);
            ReturnCall_net = null;
        }
            
        else { 
            SceneManager.LoadScene(1);
        }

        HideMe();
        UIManager.Instance.HidePanel<UI_GamePanel>();
        UIManager.Instance.ShowPanel<UI_MainPanel>();
    }

    private void DelayShutdown()
    {
        NetworkManager.Singleton.Shutdown();
    }
    private void DelayLoadScene()
    {
        SceneManager.LoadScene(1);
    }

    public override void ShowMe()
    {
        base.ShowMe();
        if(SceneManager.GetActiveScene().name == "2_Test")
            canvas.alpha = 1;
    }

    public override void HideMe()
    {
        base.HideMe();
        if (SceneManager.GetActiveScene().name == "2_Test")
            canvas.alpha = 0;
    }
}
