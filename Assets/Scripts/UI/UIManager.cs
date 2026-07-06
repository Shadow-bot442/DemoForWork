using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance = null;
    public static UIManager Instance => instance;

    //UI面板字典存储
    public Dictionary<string, UI_BasePanel> Dics ;

    public Text textInfo;

    private void Awake()
    {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        //初始化
        instance = this;
        DontDestroyOnLoad(gameObject);
        Dics = new Dictionary<string, UI_BasePanel>();


        RectTransform rectTransform;
        ABMgr.Instance.LoadResAsync<GameObject>("ui", "MainPanel", (obj) =>
        {
            obj.transform.SetParent(transform);
            rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            UI_BasePanel basePanel = obj.GetComponent<UI_BasePanel>();
            Dics.Add(basePanel.GetType().Name, basePanel);
            ShowPanel<UI_MainPanel>();
        });

        ABMgr.Instance.LoadResAsync<GameObject>("ui", "GamePanel", (obj) =>
        {
            obj.transform.SetParent(transform);
            rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            UI_BasePanel basePanel = obj.GetComponent<UI_BasePanel>();
            Dics.Add(basePanel.GetType().Name, basePanel);
        }); 

        ABMgr.Instance.LoadResAsync<GameObject>("ui", "OverPanel", (obj) =>
        {
            obj.transform.SetParent(transform);
            rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            UI_BasePanel basePanel = obj.GetComponent<UI_BasePanel>();
            Dics.Add(basePanel.GetType().Name, basePanel);
        });

        ABMgr.Instance.LoadResAsync<GameObject>("ui", "StopPanel", (obj) =>
        {
            obj.transform.SetParent(transform);
            rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            UI_BasePanel basePanel = obj.GetComponent<UI_BasePanel>();
            Dics.Add(basePanel.GetType().Name, basePanel);
        });
    }
    
    public T GetPanel<T>() where T: UI_BasePanel
    {
        string panelName = typeof(T).Name;
        if (Dics.ContainsKey(panelName))
        {
            return Dics[panelName] as T;
        }
        else { 
            return null;
        }
    }

    public bool ShowPanel<T>() where T : UI_BasePanel {
        T panel = GetPanel<T>();
        if (panel != null)
        {
            panel.ShowMe();
            return true;
        }
        else {
            return false;
        }
    }
    public bool HidePanel<T>() where T : UI_BasePanel
    {
        T panel = GetPanel<T>();
        if (panel != null)
        {
            panel.HideMe();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsOverUI(Vector2 point) {
        PointerEventData pointData = new PointerEventData(EventSystem.current);
        pointData.position = point;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointData,results);
        return results.Count > 0;
    }

    public void ShowInfo(string msg)
    {
        string timeInfo = System.DateTime.Now.ToString("HH:mm:ss");
        msg = timeInfo + "   " + msg;
        textInfo.text = msg;
    }


}
