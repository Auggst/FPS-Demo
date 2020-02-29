using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ----------------------------------------------------------------
// 类	    :	AIZombieState_Patrol1
// 介绍		:	巡逻状态
// ----------------------------------------------------------------
public class AIZombieState_Patrol1 : AIZombieState
{

    [SerializeField] float _turnOnSpotThreshold = 80.0f;
    [SerializeField] float _slerpSpeed = 5.0f;
    [SerializeField] [Range(0.0f,3.0f)] float _speed = 1.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	当第一次进入此状态时调用此函数
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        Debug.Log("Patrol");
        base.OnEnterState();
        if (_zombieStateMachine == null) return;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));

        _zombieStateMachine.navAgent.isStopped=false ;
    }

    // -------------------------------------------------------------------------
    // 类	    :	OnUpdate
    // 介绍		:	主要逻辑，保证面向目标以及处理过渡
    // -------------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {

        //视觉范围内目标为玩家
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        //视觉范围内目标为灯光
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }


        //听觉范围内目标声源为主要目标
        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        //目标为食物
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
        {
            if((1.0f-_zombieStateMachine.satisfaction)>(_zombieStateMachine.VisualThreat.distance/_zombieStateMachine.sensorRadius))
            {
                _stateMachine.SetTarget(_stateMachine.VisualThreat);
                return AIStateType.Pursuit;
            }
        }

        //如果路径已经计算出就等待
        if (_zombieStateMachine.navAgent.pathPending) 
        {
            _zombieStateMachine.speed = 0;
            return AIStateType.Patrol;
        }    
        else
            _zombieStateMachine.speed = _speed;

        //计算转向目标的角度
        float angle =Vector3.Angle(_zombieStateMachine.transform.forward, (_zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position));
       
        //如果角度太大就转为警戒状态
        if(angle > _turnOnSpotThreshold)
        {
            return AIStateType.Alerted;
        }

        //如果旋转被禁用，则要保证僵尸面向目标
        if(!_zombieStateMachine.useRootRotation)
        {
            //生成一个新的Quaternion来代表我们所需要的旋转
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);
            //过渡到目标方向
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }

        //如果代理失去原来路径则调用NextWaypoint，产生新的路径
        if(_zombieStateMachine.navAgent.isPathStale || 
          !_zombieStateMachine.navAgent.hasPath ||
            _zombieStateMachine.navAgent.pathStatus!=UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(true));
        }

        return AIStateType.Patrol;
    }


    // -------------------------------------------------------------------------
    // 类	    :	OnDestinationReached
    // 介绍		:	当僵尸到达目的地时由父类StateMachine调用
    // -------------------------------------------------------------------------
    public override void OnDestinationReached(bool isReached)
    {
        if (_zombieStateMachine == null || !isReached) return;

        //选择下一个地点
        if (_zombieStateMachine.targetType == AITargetType.Waypoint)
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(true));
    }


    /*public override void OnAnimatorIKUpdated()
    {
        if (_zombieStateMachine == null) return;

        _zombieStateMachine.animator.SetLookAtPosition(_zombieStateMachine.targetPosition + Vector3.up);
        _zombieStateMachine.animator.SetLookAtWeight(0.55f);
    
    }*/
}
