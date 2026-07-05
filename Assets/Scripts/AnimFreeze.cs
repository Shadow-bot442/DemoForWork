using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimFreeze : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<Control>().SetSpeedZero();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<Control>().SetSpeedNormal();
    }
}
