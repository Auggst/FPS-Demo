using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// -------------------------------------------------------------------------
// 类	    :	MeleeZoneTrigger
// 介绍		:	角色触发器
// -------------------------------------------------------------------------
public class MeleeZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(other.GetInstanceID());
        if(machine)
        {
            machine.inMeleeRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(other.GetInstanceID());
        if(machine)
        {
            machine.inMeleeRange = false;
        }
    }
}
