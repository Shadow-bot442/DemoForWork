using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AutoParticle : MonoBehaviour
{
    private ParticleSystem _particleSystem = null;
    private AudioSource _audioSource = null;
    public float endTime;
    public ObjectPool<GameObject> pool = null;
    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void InvokeMeth() {
        if (pool == null)
            Destroy(gameObject);
        else
            pool.Release(gameObject);
    }

    private void OnEnable()
    {
        if (_audioSource != null)
        {
            _audioSource.Play();
        }
        if (_particleSystem != null)
        {
            _particleSystem.Play();

        }

        CancelInvoke("InvokeMeth");
        //延迟执行
        Invoke("InvokeMeth", endTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
