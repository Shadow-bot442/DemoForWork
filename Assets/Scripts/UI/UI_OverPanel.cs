using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UI_OverPanel : UI_BasePanel
{
    public Button btnAgain;
    public Button btnReturn;

    public UnityAction RelifeCall_net;
    public UnityAction ReturnCall_net;

    public override void Init()
    {
        btnAgain.onClick.AddListener(() => {
            if (SceneManager.GetActiveScene().name == "2_Test")
            {
                ReGameAlone();
            }
            else
            { 
                ReGameFriend();
            }
        });


        btnReturn.onClick.AddListener(() => {

            if (SceneManager.GetActiveScene().name != "2_Test")
            {
                ReturnCall_net?.Invoke();
                Invoke("DelayShutdown", 0.3f);
                Invoke("DelayLoadScene", 0.5f);
                ReturnCall_net = null;
            }

            else
            {
                SceneManager.LoadScene(1);
            }

            HideMe();
            UIManager.Instance.HidePanel<UI_GamePanel>();
            UIManager.Instance.ShowPanel<UI_MainPanel>();
        });
    }

    private void DelayShutdown()
    {
        NetworkManager.Singleton.Shutdown();
    }
    private void DelayLoadScene()
    {
        SceneManager.LoadScene(1);
    }

    private void ReGameAlone() {
        SceneManager.LoadScene(2);                  
        HideMe();
        UIManager.Instance.ShowInfo("目标:击败两只魔王");
    }


    private void ReGameFriend()
    {
        HideMe();
        RelifeCall_net?.Invoke();
    }

}
