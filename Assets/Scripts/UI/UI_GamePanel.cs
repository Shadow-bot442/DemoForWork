using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GamePanel : UI_BasePanel
{
    [Header("组件")]
    public EventTrigger btnMove;
    public Button btnJump;
    public Button btnCrouch;
    public Button btnProne;
    public Button btnSlow;
    public Button btnShoot;
    public Button btnSupply;
    public Button btnStop;
    public RectTransform bk_nowHP;
    public Text text_bulletCount;
    public Text text_info;
    public TextMeshProUGUI text_HP;
    public RectTransform Grade_Network;
    public TextMeshProUGUI text_Grade_Network;

    [Header("参数")]
    public int hp = 100;

    //设置触发bool值告诉控制器 这个键被按下了
    public bool isCrouch = false;
    public bool isProne = false;
    public bool isJump = false;
    public bool isSlow = false;
    public bool isShoot = false;
    public bool isSupply = false;
    public bool isStop = false;

    public override void Init()
    {
        //移动按键拖拽
        moveParentPos = btnMove.transform.parent.GetComponent<RectTransform>();
        moveNowPos = btnMove.gameObject.GetComponent<RectTransform>();


        // Ensure btnMove has a Graphic (Image/Text) and that RaycastTarget is enabled so touch works on mobile.
        var graphic = btnMove.GetComponent<UnityEngine.UI.Graphic>();
        if (graphic == null)
        {
            // If missing, add a transparent Image so it can receive raycasts.
            var img = btnMove.gameObject.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1f, 1f, 1f, 0f);
            img.raycastTarget = true;
            Debug.Log("UI_GamePanel: btnMove missing Graphic. Added transparent Image to receive touch events.");
            graphic = img;
        }
        else
        {
            if (!graphic.raycastTarget) graphic.raycastTarget = true;
        }
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener((data) =>
        {
            DragMove(data);
        });
        btnMove.triggers.Add(entry);

        EventTrigger.Entry entry1 = new EventTrigger.Entry();
        entry1.eventID = EventTriggerType.EndDrag;
        entry1.callback.AddListener((data) =>
        {
            DragEnd(data);
        });
        btnMove.triggers.Add(entry1);

        btnCrouch.onClick.AddListener(onPressCrouch);
        btnProne.onClick.AddListener(onPressProne);
        btnJump.onClick.AddListener(onPressJump);
        btnSlow.onClick.AddListener(onPressSlow);
        btnShoot.onClick.AddListener(onPressShoot);
        btnSupply.onClick.AddListener(onPressSupply);
        btnStop.onClick.AddListener(onPressStop);

        HideGrade();
    }

    // 手指 id，用于手动回退逻辑
    int activeTouchId = -1;

     public override void Update()
    {
        base.Update();
        // 如果 EventTrigger 不可用或在移动设备上失效，使用手动触摸回退处理摇杆
        // 优先在移动平台执行
#if UNITY_ANDROID || UNITY_IOS
        // 处理触摸
        if (Input.touchCount > 0)
        {
            // 若已有追踪的触摸 id，则优先处理它
            bool handled = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (activeTouchId == -1)
                {
                    // 尚无追踪，尝试在 Began 时捕获第一个落在摇杆父容器内的触摸
                    if (t.phase == TouchPhase.Began)
                    {
                        Vector2 local;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(moveParentPos, t.position, null, out local);
                        if (moveParentPos.rect.Contains(local))
                        {
                            activeTouchId = t.fingerId;
                            MoveByLocalPoint(local);
                            handled = true;
                            break;
                        }
                    }
                }
                else if (t.fingerId == activeTouchId)
                {
                    if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Began)
                    {
                        Vector2 local;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(moveParentPos, t.position, null, out local);
                        MoveByLocalPoint(local);
                    }
                    else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        activeTouchId = -1;
                        moveNowPos.anchoredPosition = Vector2.zero;
                    }
                    handled = true;
                    break;
                }
            }

            // 未处理且存在触摸但不是 Began 的情况：检查是否有触摸在区域内并处理（容错）
            if (!handled && activeTouchId == -1)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch t = Input.GetTouch(i);
                    Vector2 local;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(moveParentPos, t.position, null, out local);
                    if (moveParentPos.rect.Contains(local))
                    {
                        activeTouchId = t.fingerId;
                        MoveByLocalPoint(local);
                        break;
                    }
                }
            }
        }
        else
        {
            // 没有触摸，确保复位
            if (activeTouchId != -1)
                activeTouchId = -1;
            if (moveNowPos != null)
                moveNowPos.anchoredPosition = Vector2.zero;
        }
#else
        // 编辑器/PC：使用鼠标作为回退
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(moveParentPos, mousePos, null, out local);
            if (moveParentPos.rect.Contains(local))
            {
                MoveByLocalPoint(local);
            }
        }
        else
        {
            if (moveNowPos != null)
                moveNowPos.anchoredPosition = Vector2.zero;
        }
#endif
    }

    void MoveByLocalPoint(Vector2 moveLocal)
    {
        if (moveLocal.magnitude >= 135)
        {
            Vector2 dire = moveLocal.normalized;
            moveLocal = dire * 135;
        }
        moveNowPos.anchoredPosition = moveLocal;
    }

    void onPressCrouch() => isCrouch = true;
    void onPressProne() => isProne = true;
    void onPressJump() => isJump = true;
    void onPressSlow() => isSlow = true;
    void onPressShoot() => isShoot = true;
    void onPressSupply() => isSupply = true;
    void onPressStop() {
        isStop = true;
        UIManager.Instance.ShowPanel<UI_StopPanel>();
    }

    Touch touch;
    RectTransform moveParentPos;
    RectTransform moveNowPos;
    Vector2 moveLocalPos;

    void DragMove(BaseEventData data) {
        PointerEventData pointerEventData = (PointerEventData)data;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                moveParentPos,
                pointerEventData.position,
                pointerEventData.pressEventCamera,
                out moveLocalPos
            );
        if (moveLocalPos.magnitude >= 135) {
            Vector2 dire = moveLocalPos.normalized;
            moveLocalPos = dire * 135;
        }
        moveNowPos.anchoredPosition = moveLocalPos;
    }

    void DragEnd(BaseEventData data) {
        moveNowPos.anchoredPosition = Vector2.zero;
    }

    public void updateBK_HP(int value) {
        hp += value;
        hp = Mathf.Clamp(hp,0,100);
        text_HP.text = hp.ToString();
        bk_nowHP.sizeDelta = new Vector2(hp*7, bk_nowHP.sizeDelta.y);
    }
    public void UpdateBK_HP_net(int value)
    {
        text_HP.text = value.ToString();
        bk_nowHP.sizeDelta = new Vector2(value * 7, bk_nowHP.sizeDelta.y);
    }

    public void updateBulletCount(int nowBullet,int allBullet) {
        text_bulletCount.text = nowBullet + "/" + allBullet;
    }

    public void ShowInfo(string msg) {
        color = text_info.color;
        color.a = 0;
        text_info.color = color;
        text_info .text = msg;
        StartCoroutine(IE_ShowInfo());
        CancelInvoke();
        Invoke("HideInfo", 2f); 
    }

    private void HideInfo()
    {
        StartCoroutine(IE_HideInfo());
    }

    Color color;

    IEnumerator IE_ShowInfo() {
        while (true) {
            color = text_info.color;
            color.a += Time.deltaTime;
            text_info.color = color;
            
            if (text_info.color.a >= 1)
                break;

            yield return null;
        }
    }

    IEnumerator IE_HideInfo()
    {
        while (true)
        {
            color = text_info.color;
            color.a -= Time.deltaTime;
            text_info.color = color;

            if (text_info.color.a <= 0)
                break;

            yield return null;
        }
    }

    public override void ShowMe()
    {
        base.ShowMe();
#if UNITY_EDITOR || UNITY_STANDALONE
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
#endif
        hp = 100;
        updateBK_HP(0);
    }

    public void ShowGrade() { 
        Grade_Network.gameObject.SetActive(true);
    }
    public void HideGrade() {
        Grade_Network.gameObject.SetActive(false);
    }
    public void UpdateGrade(Vector2 grade) { 
        text_Grade_Network.text = grade.x + " / " + grade.y;
    }
}
