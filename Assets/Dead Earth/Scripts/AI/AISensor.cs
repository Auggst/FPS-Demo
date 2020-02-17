using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISensor : MonoBehaviour
{

    private AIStateMachine _parentStateMachine = null;
    public AIStateMachine parentStateMachine { set { _parentStateMachine = value; } }

    void OnTriggerEnter(Collider other)
    {
        if (_parentStateMachine != null) _parentStateMachine.OnTriggerEvent(AITriggerEventType.Enter, other);
    }

    void OnTriggerStay(Collider other)
    {
        if (_parentStateMachine != null) _parentStateMachine.OnTriggerEvent(AITriggerEventType.Stay, other);
    }

    void OnTriggerExit(Collider other)
    {
        if (_parentStateMachine != null) _parentStateMachine.OnTriggerEvent(AITriggerEventType.Exit, other);
    }

}
