using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPointMgr : MonoBehaviour
{
    static EnemyPointMgr instance;
    public static EnemyPointMgr Instance => instance;

    public int monsterRemained = 0;
    private void Awake()
    {
        instance = this;
    }

    public List<Transform> enemy1Points = new List<Transform>();
    public List<Transform> enemy2Points = new List<Transform>();

    public Vector3 GetPoint_1() { 
        int randomIndex = Random.Range(0, enemy1Points.Count);
        return enemy1Points[randomIndex].position;
    }

    public Vector3 GetPoint_2()
    {
        int randomIndex = Random.Range(0, enemy2Points.Count);
        return enemy2Points[randomIndex].position;
    }

}
