using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkManager))]
public class GuardForNet : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton != this.GetComponent<NetworkManager>()) { 
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
}
