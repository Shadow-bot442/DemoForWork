using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Runtime.Enviorment;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ILRuntimeMgr : MonoBehaviour
{
    private static ILRuntimeMgr instance;
    public static ILRuntimeMgr Instance {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ILRuntimeMgr");
                instance = go.AddComponent<ILRuntimeMgr>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    MemoryStream dllStream;
    MemoryStream pdbStream;
    public AppDomain appDomain;

    private void Awake()
    {
        appDomain = new AppDomain();
    }

    public void StartILRuntime(System.Action<AppDomain> onStart)
    {

        ABMgr.Instance.LoadResAsync<TextAsset>("hotfix", "HotFix_Project.dll", (dll) =>
        {
            ABMgr.Instance.LoadResAsync<TextAsset>("hotfix", "HotFix_Project.pdb", (pdb) =>
            {
                dllStream = new MemoryStream(dll.bytes);
                pdbStream = new MemoryStream(pdb.bytes);
                appDomain.LoadAssembly(dllStream, pdbStream, new PdbReaderProvider());
                onStart?.Invoke(appDomain);
            });
        });
    }

    public void StopILRuntime()
    {
        if (appDomain == null) return;

        if (dllStream != null)
            dllStream.Close();
        if (pdbStream != null)
            pdbStream.Close();
        dllStream = null;
        pdbStream = null;
        appDomain = null;
    }
}
