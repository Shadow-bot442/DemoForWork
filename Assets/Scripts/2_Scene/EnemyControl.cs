using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyControl : MonoBehaviour
{
    private bool isFindPlayer = false;
    private bool isCanAttack = false;
    private bool isChasing = false;
    private bool isArrived = false;
    private float lastMoveTime = 0f;
    private float lastAttackTime = 0f;

    private CanvasGroup canvasGroup;
    private Vector3 playerTarget;

    [Header("参数相关")]
    public int id = 1;
    public int hp = 100;
    public int attackValue = 10;
    public float moveInternal = 3f;
    public float attackDistance = 10f;
    public float checkDistance = 15f;
    public float attackInterval = 2f;

    [Header("引用相关")]
    public Transform canvas;
    public NavMeshAgent agent;
    public Animator animator;
    public GameObject fireParticle;
    public GameObject player;
    public Transform firePos;
    public GameObject hitPlayerPar;
    public GameObject hitMapPar;
    public RectTransform img_HP;
    public GameObject takeDamageSound;
    public GameObject deathSound;

    private void Start()
    {
        canvasGroup = canvas.GetComponent<CanvasGroup>();
        NavMove();
        EnemyPointMgr.Instance.monsterRemained++;
    }

    private void Update()
    {
        if (player == null || player.GetComponent<Control>() == null) { 
            Destroy(this);
            return;
        }

        //0. 显示UI
        if (Vector3.Distance(transform.position, player.transform.position) <= checkDistance)
        {
            canvasGroup.alpha = 1;
            canvas.rotation = Quaternion.LookRotation(player.transform.position - canvas.position + Vector3.up*2);
        }
        else {
            canvasGroup.alpha = 0;
        }

        //1. 移动
        if ((agent.remainingDistance < 0.1f || !agent.hasPath) && !isArrived && !isFindPlayer)
        {
            isArrived = true;
            lastMoveTime = Time.time;
        }

        if (Time.time - lastMoveTime >= moveInternal && !isFindPlayer && isArrived) {
            NavMove();
            isArrived = false;
        }

        //2. 攻击
        //不断判断是否发现玩家，发现玩家后持续追击，直到玩家进入攻击范围
        if (Vector3.Distance(player.transform.position, transform.position) <= checkDistance)
        {
            transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
            isFindPlayer = true;


            //如果玩家在攻击范围内,并且第一次进入，则设置可攻击和上次攻击时间，另外如果玩家不在攻击范围内，则持续追击，并且设置不可攻击(刷新攻击间隔)
            if (Vector3.Distance(player.transform.position, transform.position) <= attackDistance)
            {
                if (!isCanAttack)
                {
                    isCanAttack = true;
                    lastAttackTime = Time.time;
                    isChasing = false;
                }
                agent.isStopped = true;
            }
            else
            {
                //NavMove((player.transform.position - transform.position).normalized * (attackDistance+2));
                NavMove(player.transform.position);
                isCanAttack = false;
                agent.isStopped = false;
            }
        }
        else {
            isFindPlayer = false;
            isChasing = isCanAttack = false;
        }

        //如果玩家在攻击范围内，并且满足攻击间隔，则攻击
        if (Time.time - lastAttackTime >= attackInterval && isCanAttack) {
            Fire();
            lastAttackTime = Time.time;
        }

        //3. 设置anim
        if (animator != null && agent != null) {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    private void NavMove()
    {
        switch (id)
        {
            case 1:
                agent.SetDestination(EnemyPointMgr.Instance.GetPoint_1());
                break;
            case 2:
                agent.SetDestination(EnemyPointMgr.Instance.GetPoint_2());
                break;
        }
    }

    private void NavMove(Vector3 pos) {
        agent.SetDestination(pos);
    }

    RaycastHit hitinfo;
    private void Fire() {

        Instantiate<GameObject>(fireParticle, firePos.position, firePos.rotation);
        
        switch (player.GetComponent<Control>().NowState)
        {
            case E_State.Run:
            case E_State.Walk:
                playerTarget = player.transform.position + Vector3.up * 1.5f;
                break;
            case E_State.Crouch:
                playerTarget = player.transform.position + Vector3.up * 1f;
                break;
            case E_State.Prone:
                playerTarget = player.transform.position + Vector3.up * 0.5f;
                break;
        }

        if (Physics.Raycast(new Ray(firePos.position, (playerTarget - firePos.position).normalized), out hitinfo, 100f, 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Map"))) {
            if (hitinfo.transform.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
            {
                if(player.GetComponent<Control>() != null)
                {
                    player.GetComponent<Control>().TakeDamage(attackValue);
                    Instantiate<GameObject>(hitPlayerPar, hitinfo.point, Quaternion.LookRotation(hitinfo.normal));
                }
                
            }
            else {
                Instantiate<GameObject>(hitMapPar, hitinfo.point, Quaternion.LookRotation(hitinfo.normal));
            }
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        hp = Mathf.Clamp(hp,0,100);
        img_HP.sizeDelta = new Vector2(hp * 0.03f, img_HP.sizeDelta.y);
        animator.SetTrigger("IsGotHited");
        if (hp <= 0)
        {
            Death();
        }
        else {
            Instantiate<GameObject>(takeDamageSound, transform.position, Quaternion.identity);
        }
    }

    private void Death() {
        animator.SetTrigger("IsDeath");
        Destroy(gameObject, 3f);
        Instantiate<GameObject>(deathSound,transform.position,Quaternion.identity);
        Destroy(this);
        EnemyPointMgr.Instance.monsterRemained--;
        if (EnemyPointMgr.Instance.monsterRemained == 0) {
            UIManager.Instance.ShowInfo("你成功完成任务!");
        }
    }
}
