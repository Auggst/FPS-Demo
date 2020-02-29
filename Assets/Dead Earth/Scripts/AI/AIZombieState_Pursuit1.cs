using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------
// 类   	: AIZombieState_Pursuit1
// 介绍		: 僵尸的追逐状态
// -----------------------------------------------------------------
public class AIZombieState_Pursuit1 : AIZombieState
{
    [SerializeField] [Range(0, 10)] private float _speed = 1.0f;
    [SerializeField] private float _slerpSpeed = 5.0f;
    [SerializeField] private float _repathDistanceMultiplier = 0.035f;
    [SerializeField] private float _repathVisualMinDuration = 0.05f;
    [SerializeField] private float _repathVisualMaxDuration = 5.0f;
    [SerializeField] private float _repathAudioMinDuration = 0.25f;
    [SerializeField] private float _repathAudioMaxDuration = 5.0f;
    [SerializeField] private float _maxDuration = 40.0f;

    private float _timer = 0.0f;
    private float _repathTimer = 0.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Pursuit;
    }

    public override void OnEnterState()
    {
        Debug.Log("Pursuit");

        base.OnEnterState();
        if (_zombieStateMachine == null) return;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;


        _timer = 0.0f;
        _repathTimer = 0.0f;

        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.targetPosition);
        _zombieStateMachine.navAgent.isStopped = false;
    }

    // ---------------------------------------------------------------------
    // 类	:	OnUpdateAI
    // 介绍	:	核心逻辑
    // ---------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;
        _repathTimer += Time.deltaTime;

        if (_timer > _maxDuration)
            return AIStateType.Patrol;

        //如果发现目标为玩家，且在范围内则攻击
        if (_stateMachine.targetType == AITargetType.Visual_Player && _zombieStateMachine.inMeleeRange)
        {
            return AIStateType.Attack;
        }

        //如果到达目标则判断类型
        if (_zombieStateMachine.isTargetReached)
        {
            switch (_stateMachine.targetType)
            {
                case AITargetType.Audio:
                case AITargetType.Visual_Light:
                    _stateMachine.ClearTarget();
                    return AIStateType.Alerted;
                case AITargetType.Visual_Food:
                    return AIStateType.Feeding;
            }

        }

        //如果失去路径则调用转为警戒状态
        if (_zombieStateMachine.navAgent.isPathStale ||
            (!_zombieStateMachine.navAgent.hasPath && !_zombieStateMachine.navAgent.pathPending) ||
            _zombieStateMachine.navAgent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.navAgent.pathPending)
            _zombieStateMachine.speed = 0;
        else
        {
            _zombieStateMachine.speed = _speed;

            //如果接近的目标为玩家，将调整角度，面向玩家
            if (!_zombieStateMachine.useRootRotation && _zombieStateMachine.targetType == AITargetType.Visual_Player && _zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player && _zombieStateMachine.isTargetReached)
            {
                Vector3 targetPos = _zombieStateMachine.targetPosition;
                targetPos.y = _zombieStateMachine.transform.position.y;
                Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
                _zombieStateMachine.transform.rotation = newRot;
            }
            else
            //逐渐更新角度使其与目标一致
            if (!_stateMachine.useRootRotation && !_zombieStateMachine.isTargetReached)
            {
                Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);
                _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);

            }
            else
            //如果到达目的地则转为警戒状态
            if (_zombieStateMachine.isTargetReached)
            {
                return AIStateType.Alerted;
            }

        }

        //如果目标为玩家
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            //当前目标位置与玩家位置不同
            if (_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position)
            {
                if (Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
                {
                    _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.VisualThreat.position);
                    _repathTimer = 0.0f;
                }
            }

            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);

            return AIStateType.Pursuit;
        }
        if (_zombieStateMachine.targetType == AITargetType.Visual_Player)
            return AIStateType.Pursuit;

        //如果目标为灯光
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            if (_zombieStateMachine.targetType == AITargetType.Audio || _zombieStateMachine.targetType == AITargetType.Visual_Food)
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                return AIStateType.Alerted;
            }
            else
            if (_zombieStateMachine.targetType == AITargetType.Visual_Light)
            {
                //获取目标碰撞器的ID
                int currenntID = _zombieStateMachine.targetColliderID;

                if (currenntID == _zombieStateMachine.VisualThreat.collider.GetInstanceID())
                {
                    if (_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position)
                    {
                        if (Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
                        {
                            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.VisualThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }

                    _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                    return AIStateType.Pursuit;
                }
                else
                {
                    _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                    return AIStateType.Alerted;
                }
            }
        }
        else
        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            if (_zombieStateMachine.targetType == AITargetType.Visual_Food)
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
                return AIStateType.Alerted;
            }
            else
            if (_zombieStateMachine.targetType == AITargetType.Audio)
            {
                int currentID = _zombieStateMachine.targetColliderID;

                if (currentID == _zombieStateMachine.AudioThreat.collider.GetInstanceID())
                {
                    if (_zombieStateMachine.targetPosition != _zombieStateMachine.AudioThreat.position)
                    {
                        if (Mathf.Clamp(_zombieStateMachine.AudioThreat.distance * _repathDistanceMultiplier, _repathAudioMinDuration, _repathAudioMaxDuration) < _repathTimer)
                        {
                            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.AudioThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }

                    _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
                    return AIStateType.Pursuit;
                }
                else
                {
                    _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
                    return AIStateType.Alerted;
                }
            }
        }


        return AIStateType.Pursuit;
    }



}
