using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Alerted1 : AIZombieState
{

    [SerializeField] [Range(1, 60)] float _maxDuration = 10.0f;
    [SerializeField] float _waypointAngleThreshold = 90.0f;
    [SerializeField] float _threatAngleThreshold = 10.0f;
    [SerializeField] float _directionChangeTime = 1.5f;

    float _timer = 0.0f;
    float _directionChangeTimer = 0.0f;

    //-----------------------------------------------------
    //名称：GetStateType
    //介绍：返回当前状态类型
    //-----------------------------------------------------
    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    //-----------------------------------------------------
    //名称：OnEnterState
    //介绍：进入此状态时，由State Machine调用，初始化
    //      一个计时器和配置state machine
    //-----------------------------------------------------
    public override void OnEnterState()
    {
        Debug.Log("Alerted");
        base.OnEnterState();
        if (_zombieStateMachine == null) return;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        _timer = _maxDuration;
        _directionChangeTimer = 0.0f;
    }

    //-----------------------------------------------------
    //名称：OnUpdate
    //介绍：此状态的核心逻辑
    //-----------------------------------------------------
    public override AIStateType OnUpdate()
    {
        _timer -= Time.deltaTime;
        _directionChangeTimer += Time.deltaTime;

        if (_timer <= 0.0f)
        {
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
            _zombieStateMachine.navAgent.isStopped = false;
            _timer = _maxDuration;
        }
        
        if(_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        if(_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            _timer = _maxDuration;
        }

        if(_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            _timer = _maxDuration;
        }

        if(_zombieStateMachine.AudioThreat.type == AITargetType.None &&
            _zombieStateMachine.VisualThreat.type==AITargetType.Visual_Food)
        {
            _zombieStateMachine.SetTarget(_stateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        float angle;

        if((_zombieStateMachine.targetType==AITargetType.Audio||_zombieStateMachine.targetType==AITargetType.Visual_Light) && !_zombieStateMachine.isTargetReached)
        {
            angle = AIState.FindSignedAngle(_zombieStateMachine.transform.forward,
                                            _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position);

            if(_zombieStateMachine.targetType==AITargetType.Audio&&Mathf.Abs(angle)<_threatAngleThreshold)
            {
                return AIStateType.Pursuit;
            }
            if(_directionChangeTimer > _directionChangeTime)
            {
                if(Random.value < _zombieStateMachine.intelligence)
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                }
                else
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                }

                _directionChangeTimer = 0.0f;
            }
        }
        else 
        if(_zombieStateMachine.targetType==AITargetType.Waypoint && !_zombieStateMachine.navAgent.pathPending)
        {
            angle = AIState.FindSignedAngle(_zombieStateMachine.transform.forward,
                                            _zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position);
            if (Mathf.Abs(angle) < _waypointAngleThreshold) return AIStateType.Patrol;
            if(_directionChangeTimer >_directionChangeTime)
            {
                _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                _directionChangeTimer = 0.0f;
            }

        }


        return AIStateType.Alerted;
    }


}
