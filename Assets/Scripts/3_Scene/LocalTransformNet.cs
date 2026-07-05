using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class LocalTransformNet : NetworkTransform
{
    public float maxOffset = 0.5f;
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }

    protected override void OnNetworkTransformStateUpdated(ref NetworkTransformState oldState, ref NetworkTransformState newState)
    {
        base.OnNetworkTransformStateUpdated(ref oldState, ref newState);

        if (!IsOwner)
            return;

        //判断若是大于最大偏移值就回正位置
        if (Vector3.Distance(transform.position, newState.GetPosition()) > maxOffset) {
            CharacterController cc = GetComponent<CharacterController>();
            cc.enabled = false;
            transform.position = newState.GetPosition();
            cc.enabled = true;
        }
    }
}
