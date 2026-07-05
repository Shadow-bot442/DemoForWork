using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TEst : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        Invoke("DelayTest",5);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void DelayTest() {
        Debug.Log("INvoke重新启用");
    }
}
