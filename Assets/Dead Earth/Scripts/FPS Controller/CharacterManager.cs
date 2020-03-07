using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -------------------------------------------------------------------------
// 类	    :	CharacterManager
// 介绍		:	角色相关管理
// -------------------------------------------------------------------------
public class CharacterManager : MonoBehaviour
{
    [SerializeField] private CapsuleCollider _meleeTrigger = null;  //角色触发器
    [SerializeField] private CameraBloodEffect _cameraBloodEffect = null;   //受伤粒子特效
    [SerializeField] private Camera _camera = null;     //角色镜头
    [SerializeField] private float _health = 100.0f;    //角色生命值

    private Collider _collider = null;
    private FPSController _fpsController = null;
    private CharacterController _characterController = null;
    private GameSceneManager _gameSceneManager = null;
    private int _aiBodyPartLayer = -1;


    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider>();
        _fpsController = GetComponent<FPSController>();
        _characterController = GetComponent<CharacterController>();
        _gameSceneManager = GameSceneManager.instance;

        _aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

        if(_gameSceneManager!=null)
        {
            PlayerInfo info = new PlayerInfo();
            info.camera = _camera;
            info.characterManager = this;
            info.collider = _collider;
            info.meleeTrigger = _meleeTrigger;

            _gameSceneManager.RegisterPlayerInfo(_collider.GetInstanceID(), info);
        }
    }

    // -------------------------------------------------------------------------
    // 类	    :	TakeDamage
    // 介绍		:	受伤
    // -------------------------------------------------------------------------
    public void TakeDamage(float amount)
    {
        _health = Mathf.Max(_health - (amount * Time.deltaTime), 0.0f);
        if(_cameraBloodEffect!=null)
        {
            _cameraBloodEffect.minBloodAmount = (1.0f - (_health / 100.0f));
            _cameraBloodEffect.bloodAmount = Mathf.Min(_cameraBloodEffect.minBloodAmount + 0.3f, 1.0f);
        }
    }

    // -------------------------------------------------------------------------
    // 类	    :	DoDamage
    // 介绍		:	攻击
    // -------------------------------------------------------------------------
    public void DoDamage(int hitDirection = 0)
    {
        if (_camera == null)  return;
        if (_gameSceneManager == null) return;

        Ray ray ;
        RaycastHit hit;
        bool isSomethingHit = false;
        ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0.0f));

        isSomethingHit = Physics.Raycast(ray,out hit,1000.0f,1<<_aiBodyPartLayer);

        if(isSomethingHit)
        {
            AIStateMachine stateMachine = _gameSceneManager.GetAIStateMachine(hit.rigidbody.GetInstanceID());
            if(stateMachine)
            {
                stateMachine.TakeDamage(hit.point, ray.direction * 5.0f, 25, hit.rigidbody, this, 0);
            }
        }
    }

    //左键射击
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            DoDamage();
        }
    }
}
