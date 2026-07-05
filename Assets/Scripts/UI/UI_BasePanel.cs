using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class UI_BasePanel : MonoBehaviour
{
    public CanvasGroup canvas;
    private float showSpeed = 1;
    public bool isShow;
    
    public virtual void Awake()
    {
        HideMe();
        canvas.alpha = 0;
    }

    public virtual void Start()
    {
        Init();
    }

    public abstract void Init();

    public virtual void Update() {
        if (isShow && canvas.alpha != 1) {
            canvas.alpha += Time.deltaTime * showSpeed;
            if (canvas.alpha >= 1 ) 
                canvas.alpha = 1;
        }
        else if (!isShow && canvas.alpha != 0)
        {
            canvas.alpha -= Time.deltaTime * showSpeed;
            if (canvas.alpha <= 0)
                canvas.alpha = 0;
        }
    }

    public virtual void ShowMe() { 
        isShow = true;
        canvas.alpha = 0;
        canvas.interactable = true;
        canvas.blocksRaycasts = true;
    }

    public virtual void HideMe()
    {
        isShow = false;
        canvas.alpha = 1;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
    }
}
