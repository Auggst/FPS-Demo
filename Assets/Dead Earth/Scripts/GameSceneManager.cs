using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -------------------------------------------------------------------------
// 类	    :	GameSceneManager
// 介绍		:	作为场景管理的单例模式的类
// -------------------------------------------------------------------------
public class GameSceneManager : MonoBehaviour
{

    [SerializeField] private ParticleSystem _bloodParticles = null;

    //单例模式
    private static GameSceneManager _instance = null;
    public static GameSceneManager instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = (GameSceneManager)FindObjectOfType(typeof(GameSceneManager));
            }
            return _instance;
        }
    }

    private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();

    public ParticleSystem bloodParticles { get { return _bloodParticles; } }

    // -------------------------------------------------------------------------
    // 类	    :	RegisterAIStateMachine
    // 介绍		:	用字典类型来存储已出现的AIStateMachine
    // -------------------------------------------------------------------------
    public void RegisterAIStateMachine(int key,AIStateMachine stateMachine)
    {
        if(!_stateMachines.ContainsKey(key))
        {
            _stateMachines[key] = stateMachine;
        }
    }

    // -------------------------------------------------------------------------
    // 类	    :	GetAIStateMachine
    // 介绍		:	获取目标值的AIStateMachine
    // -------------------------------------------------------------------------
    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine machine = null;
        if(_stateMachines.TryGetValue(key,out machine))
        {
            return machine;
        }
        return null;
    }


}
