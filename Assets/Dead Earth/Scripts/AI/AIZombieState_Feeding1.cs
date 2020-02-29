using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Feeding1 : AIZombieState
{

    [SerializeField] float _slerpSpeed = 5.0f;
    [SerializeField] Transform _bloodParticleMount = null;
    [SerializeField] [Range(0.01f, 1.0f)] float _bloodParticlesBurstTime = 0.1f;
    [SerializeField] [Range(1, 100)] int _bloodParticlesBurstAmount = 10;

    private int _eatingStateHash = Animator.StringToHash("Feeding State");
    private int _eatingLayerIndex = -1;
    private float _timer = 0.0f;


    public override AIStateType GetStateType()
    {
        return AIStateType.Feeding;
    }

    public override void OnEnterState()
    {
        Debug.Log("Feeding State");

        base.OnEnterState();
        if (_zombieStateMachine == null) return;

        //获取层级的索引
        if (_eatingLayerIndex == -1) 
            _eatingLayerIndex = _zombieStateMachine.animator.GetLayerIndex("Cinematic");

        //重置出血粒子效果计时器
        _timer = 0.0f;

        //设置StateMachine的动画控制器
        _zombieStateMachine.feeding = true;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.attackType = 0;

        //更新代理的位置但不更新旋转方向
        _zombieStateMachine.NavAgentControl(true, false);
    }

    public override void OnExitState()
    {
        if (_zombieStateMachine != null)
            _zombieStateMachine.feeding = false;
    }

    // -------------------------------------------------------------------------
    // 类	    :	OnUpdate
    // 介绍		:	主要逻辑
    // -------------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;

        if(_zombieStateMachine.satisfaction>0.9f)
        {
            _zombieStateMachine.GetWaypointPosition(false);
            return AIStateType.Alerted;
        }

        //如果视野中有目标则进入警戒模式
        if(_zombieStateMachine.VisualThreat.type!=AITargetType.None && _zombieStateMachine.VisualThreat.type!=AITargetType.Visual_Food)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }

        //如果有声音在听力范围内进入警戒模式
        if(_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }
        
        //进食
        if(_zombieStateMachine.animator.GetCurrentAnimatorStateInfo(_eatingLayerIndex).shortNameHash==_eatingStateHash)
        {
            _zombieStateMachine.satisfaction = Mathf.Min(_zombieStateMachine.satisfaction + ((Time.deltaTime * _zombieStateMachine.replenishRate)/100.0f),1.0f);
            if(GameSceneManager.instance && GameSceneManager.instance.bloodParticles && _bloodParticleMount)
            {
                if(_timer > _bloodParticlesBurstTime)
                {
                    ParticleSystem system = GameSceneManager.instance.bloodParticles;
                    system.transform.position = _bloodParticleMount.transform.position;
                    system.transform.rotation = _bloodParticleMount.transform.rotation;
                    //TODO 弃用修改
                    system.simulationSpace = ParticleSystemSimulationSpace.World;
                    system.Emit(_bloodParticlesBurstAmount);
                    _timer = 0.0f;
                }
            }
        }

        if(!_zombieStateMachine.useRootRotation)
        {
            //令僵尸面向目标
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);

        }

        return AIStateType.Feeding;
    }
}
