using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public RectTransform img_Progress;

    float maxWidth;
    float maxHeight;

    private void Start()
    {
        maxWidth = img_Progress.sizeDelta.x;
        maxHeight = img_Progress.sizeDelta.y;
        img_Progress.sizeDelta = new Vector2(0, maxHeight);

#if UNITY_SERVER
        ConnectManager.Instance.ConnectAsServer(5888);
#elif UNITY_ANDROID
        ConnectManager.Instance.ConnectAsClient("10.251.33.79", 5888);
#else
        ConnectManager.Instance.ConnectAsClient("10.251.33.79", 5888);
#endif
        //开启更新检查 并更新
        ABUpdateMgr.Instance.CheckUpdate((isOver) => { }, (msg) => { Debug.Log(msg); },UpdateProgress, AllOverCallBack);
    }

    private void AllOverCallBack() {
        SceneManager.LoadScene(1);
    }

    private void UpdateProgress(float progress) { 
        img_Progress.sizeDelta = new Vector2(maxWidth * progress, maxHeight);
    }

}
