using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class LocalAnimatorNet : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
