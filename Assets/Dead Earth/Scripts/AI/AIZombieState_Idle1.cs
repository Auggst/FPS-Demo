using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ----------------------------------------------------------------------
// 类    : AIZombieState_Idle1
// 介绍	 : Idle状态
// ----------------------------------------------------------------------
public class AIZombieState_Idle1 : AIZombieState
{
    [SerializeField] Vector2 _idleTimeRange = new Vector2(10.0f, 60.0f);

    float _idleTime = 0.0f;
    float _timer = 0.0f;
    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }

    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	当第一次进入此状态时调用此函数
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        Debug.Log("Idle");
        base.OnEnterState();
        if (_zombieStateMachine == null) return;

        _idleTime = Random.Range(_idleTimeRange.x, _idleTimeRange.y);
        _timer = 0.0f;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;
        _zombieStateMachine.ClearTarget();
    }

    //静止状态时的状态判定
    // -------------------------------------------------------------------------
    // 类	    :	OnUpdate
    // 介绍		:	主要逻辑
    // -------------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        if (_zombieStateMachine == null) return AIStateType.Idle;
        //视觉判定
        if(_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }

        //听觉判定
        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }


        //食物判定
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        _timer += Time.deltaTime;
        if (_timer > _idleTime)
        {
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
            _zombieStateMachine.navAgent.isStopped = false;
            return AIStateType.Alerted;
        }
        return AIStateType.Idle;
    }
}
