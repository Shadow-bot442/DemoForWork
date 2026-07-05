using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

public enum E_BaseState { 
    Stand,Crouch,Prone
}

public class AnimCC : StateMachineBehaviour
{
    public E_BaseState nowState;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        switch (nowState)
        {
            case E_BaseState.Stand:
                animator.gameObject.GetComponent<Control>().SetStandCC();
                break;
            case E_BaseState.Crouch:
                animator.gameObject.GetComponent<Control>().SetCrouchCC();

                break;
            case E_BaseState.Prone:
                animator.gameObject.GetComponent<Control>().SetProneCC();

                break;
            default:
                break;
        }
    }
}
