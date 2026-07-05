using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolMgr : MonoBehaviour
{
    public enum E_ParName { 
        Fire,FireDeath,TakeDamage_Sound,HitDecal,HitPeople,NoBullet_Sound, SupplyBullet_Sound
    }

    public List<GameObject> objects = new List<GameObject>();

    static ObjectPoolMgr instance;
    public static ObjectPoolMgr Instance => instance;

    private Dictionary<E_ParName, ObjectPool<GameObject>> Pools = new Dictionary<E_ParName, ObjectPool<GameObject>>();

    ObjectPool<GameObject> pool_0 = null;
    ObjectPool<GameObject> pool_1 = null;
    ObjectPool<GameObject> pool_2 = null;
    ObjectPool<GameObject> pool_3 = null;
    ObjectPool<GameObject> pool_4 = null;
    ObjectPool<GameObject> pool_5 = null;
    ObjectPool<GameObject> pool_6 = null;

    private void Awake()
    {
        ////服务器不需要特效对象池管理器
        //if (NetworkManager.Singleton.IsServer)
        //{ 
        //    Destroy(gameObject);
        //    return;
        //}

        //初始化所有对象池 并添加进Pools字典里
        instance = this;

        pool_0 = new ObjectPool<GameObject>(Create0, Get0, Release0, Destroy0,true,10,20);
        Pools.Add(E_ParName.Fire, pool_0);

         pool_1 = new ObjectPool<GameObject>(Create1, Get1, Release1, Destroy1,true, 10, 20);
        Pools.Add(E_ParName.FireDeath, pool_1);

         pool_2 = new ObjectPool<GameObject>(Create2, Get2, Release2, Destroy2, true, 10, 20);
        Pools.Add(E_ParName.TakeDamage_Sound, pool_2);

         pool_3 = new ObjectPool<GameObject>(Create3, Get3, Release3, Destroy3, true, 10, 40);
        Pools.Add(E_ParName.HitDecal, pool_3);

         pool_4 = new ObjectPool<GameObject>(Create4, Get4, Release4, Destroy4, true, 10, 20);
        Pools.Add(E_ParName.HitPeople, pool_4);

         pool_5 = new ObjectPool<GameObject>(Create5, Get5, Release5, Destroy5, true, 10, 20);
        Pools.Add(E_ParName.NoBullet_Sound, pool_5);

        pool_6 = new ObjectPool<GameObject>(Create6, Get6, Release6, Destroy6, true, 10, 20); 
        Pools.Add(E_ParName.SupplyBullet_Sound, pool_6);
    }

    //对象池的回调函数
    #region Fire
    GameObject Create0() {
        GameObject obj = Instantiate(objects[0]);                      //改
        obj.GetComponent<AutoParticle>().pool = pool_0;
        obj.SetActive(false);
        return obj;
    }

    void Get0(GameObject obj) {
        obj.SetActive(true);
    }

    void Release0(GameObject obj) {
        obj.SetActive(false);
    }

    void Destroy0(GameObject obj) { 
        Destroy(obj.gameObject);
    }

    #endregion
    #region FireDeath
    GameObject Create1()
    {
        GameObject obj = Instantiate(objects[1]);                      //改
        obj.GetComponent<AutoParticle>().pool = pool_1;
        obj.SetActive(false);
        return obj;
    }

    void Get1(GameObject obj)
    {
        obj.SetActive(true);
    }

    void Release1(GameObject obj)
    {
        obj.SetActive(false);
    }

    void Destroy1(GameObject obj)
    {
        Destroy(obj.gameObject);
    }

    #endregion
    #region TakeDamage_Sound
    GameObject Create2()
    {
        GameObject obj = Instantiate(objects[2]);                      //改
        obj.GetComponent<AutoParticle>().pool = pool_2;
        obj.SetActive(false);
        return obj;
    }

    void Get2(GameObject obj)
    {
        obj.SetActive(true);
    }

    void Release2(GameObject obj)
    {
        obj.SetActive(false);
    }

    void Destroy2(GameObject obj)
    {
        Destroy(obj.gameObject);
    }

    #endregion
    #region HitDecal
    GameObject Create3()
    {
        GameObject obj = Instantiate(objects[3]);                      //改
        obj.GetComponent<AutoParticle>().pool = pool_3;
        obj.SetActive(false);
        return obj;
    }

    void Get3(GameObject obj)
    {
        obj.SetActive(true);
    }

    void Release3(GameObject obj)
    {
        obj.SetActive(false);
    }

    void Destroy3(GameObject obj)
    {
        Destroy(obj.gameObject);
    }

    #endregion
    #region HitPeople
    GameObject Create4()
    {
        GameObject obj = Instantiate(objects[4]);                      //改
        obj.GetComponent<AutoParticle>().pool = pool_4;
        obj.SetActive(false);
        return obj;
    }

    void Get4(GameObject obj)
    {
        obj.SetActive(true);
    }

    void Release4(GameObject obj)
    {
        obj.SetActive(false);
    }

    void Destroy4(GameObject obj)
    {
        Destroy(obj.gameObject);
    }

    #endregion
    #region NoBullet_Sound
    GameObject Create5()
    {
        GameObject obj = Instantiate(objects[5]);                      //改
        obj.GetComponent<AutoParticle>().pool = pool_5;
        obj.SetActive(false);
        return obj;
    }

    void Get5(GameObject obj)
    {
        obj.SetActive(true);
    }

    void Release5(GameObject obj)
    {
        obj.SetActive(false);
    }

    void Destroy5(GameObject obj)
    {
        Destroy(obj.gameObject);
    }

    #endregion

    #region SupplyBullet_Sound
    GameObject Create6()
    {
        GameObject obj = Instantiate(objects[6]);                      //改
        obj.GetComponent<AutoParticle>().pool = pool_6;
        obj.SetActive(false);
        return obj;
    }

    void Get6(GameObject obj)
    {
        obj.SetActive(true);
    }

    void Release6(GameObject obj)
    {
        obj.SetActive(false);
    }

    void Destroy6(GameObject obj)
    {
        Destroy(obj.gameObject);
    }

    #endregion

    //使用对应物体对象池方法
    public GameObject GetObj(E_ParName e_Par)
    {
        return Pools[e_Par].Get();
    }

    //public bool ReleaseObj(GameObject obj)
    //{
    //    int i = -1;
    //    for (int j = 0; j < objects.Count; j++) {
    //        if (obj == objects[j]) {
    //            i = j;
    //            break;
    //        }
    //    }

    //    if (i == -1)
    //        return false;

    //    else 
    //    {
    //        Pools[(E_ParName)i].Release(obj);
    //        return true; 
    //    }
    //}
}
