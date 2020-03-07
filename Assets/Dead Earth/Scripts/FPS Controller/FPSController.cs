using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//角色移动状态
public enum PlayerMoveStatus { NotMoving, Crouching, Walking, Runnig, NotGrounded, Landing }

//曲线动画控制晃动反馈类型
public enum CurveControlledBobCallbackType { Horizontal,Vertical}

//曲线动画晃动反馈
public delegate void CurveControlledBobCallback();

// -------------------------------------------------------------------------
// 类	    :	CurveControlledBobEvent
// 介绍		:	用于处理曲线动画晃动反馈方法的回调
// -------------------------------------------------------------------------
[System.Serializable]
public class CurveControlledBobEvent
{
    public float Time = 0.0f;
    public CurveControlledBobCallback Function = null;
    public CurveControlledBobCallbackType Type = CurveControlledBobCallbackType.Vertical;
}

// -------------------------------------------------------------------------
// 类	    :	CurveControlledBob
// 介绍		:	处理曲线动画晃动反馈
// -------------------------------------------------------------------------
[System.Serializable]
public class CurveControlledBob
{
    //晃动曲线
    [SerializeField]
    AnimationCurve _bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                              new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                              new Keyframe(2f, 0f));
    [SerializeField] float _horizontalMultiplier = 0.01f;   //垂直晃动系数
    [SerializeField] float _verticalMultiplier = 0.02f;     //水平晃动系数
    [SerializeField] float _verticaltoHorizontalSpeedRation = 2.0f; //晃动速率
    [SerializeField] float _baseInterval = 1.0f;    //基础间隔

    private float _prevXPlayHead;   //上一帧角色头部x坐标
    private float _prevYPlayHead;   //上一帧角色头部y坐标
    private float _xPlayHead;       //角色头部x坐标
    private float _yPlayHead;       //角色头部y坐标
    private float _curveEndTime;    //曲线动画结束时间
    private List<CurveControlledBobEvent> _events = new List<CurveControlledBobEvent>();    //晃动事件列表

    // -------------------------------------------------------------------------
    // 方法	    :	Initialize
    // 介绍		:	初始化
    // -------------------------------------------------------------------------
    public void  Initialize()
    {
        //记录bob曲线的时长
        _curveEndTime = _bobcurve[_bobcurve.length - 1].time;
        _xPlayHead = 0.0f;
        _yPlayHead = 0.0f;
        _prevXPlayHead = 0.0f;
        _prevYPlayHead = 0.0f;
    }

    // -------------------------------------------------------------------------
    // 方法	    :	RegisterEventCallback
    // 介绍		:	注册事件回调
    // -------------------------------------------------------------------------
    public void RegisterEventCallback(float time,CurveControlledBobCallback function,CurveControlledBobCallbackType type)
    {
        CurveControlledBobEvent ccbeEvent = new CurveControlledBobEvent();
        ccbeEvent.Time = time;
        ccbeEvent.Function = function;
        ccbeEvent.Type = type;
        _events.Add(ccbeEvent);
        _events.Sort(
            //匿名方法
            delegate (CurveControlledBobEvent t1, CurveControlledBobEvent t2)
            {
                return (t1.Time.CompareTo(t2.Time));
            }
        );
    }

    // -------------------------------------------------------------------------
    // 方法	    :	GetVectorOffset
    // 介绍		:	获取向量坐标
    // -------------------------------------------------------------------------
    public Vector3 GetVectorOffset(float speed)
    {
        _xPlayHead += (speed * Time.deltaTime)/_baseInterval;
        _yPlayHead += ((speed * Time.deltaTime) / _baseInterval) * _verticaltoHorizontalSpeedRation;

        if (_xPlayHead > _curveEndTime) _xPlayHead -= _curveEndTime;
        if (_yPlayHead > _curveEndTime) _yPlayHead -= _curveEndTime;

        for(int i =0;i<_events.Count;i++)
        {
            CurveControlledBobEvent ev = _events[i];
            if(ev!=null)
            {
                if(ev.Type == CurveControlledBobCallbackType.Vertical)
                {
                    if((_prevYPlayHead<ev.Time && _yPlayHead >= ev.Time)||
                       (_prevYPlayHead>_yPlayHead &&(ev.Time > _prevYPlayHead || ev.Time <= _yPlayHead)))
                    {
                        ev.Function();
                    }
                }
                else
                {
                    if((_prevXPlayHead < ev.Time && _xPlayHead >=ev.Time)||
                       (_prevXPlayHead > _xPlayHead && (ev.Time > _prevXPlayHead || ev.Time <= _xPlayHead)))
                    {
                        ev.Function();
                    }
                }
            }
        }

        float xPos = _bobcurve.Evaluate(_xPlayHead) * _horizontalMultiplier;
        float yPos = _bobcurve.Evaluate(_yPlayHead) * _verticalMultiplier;

        _prevXPlayHead = _xPlayHead;
        _prevYPlayHead = _yPlayHead;

        return new Vector3(xPos, yPos, 0f);
    }
}


// -------------------------------------------------------------------------
// 类	    :	FPSController
// 介绍		:	角色控制
// -------------------------------------------------------------------------
[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public List<AudioSource> AudioSources = new List<AudioSource>();    //音效列表
    private int _audioToUse = 0;

    [SerializeField] private float _walkSpeed = 2.0f;   //行走速度
    [SerializeField] private float _runSpeed = 4.5f;    //跑步速度
    [SerializeField] private float _jumpSpeed = 7.5f;   //跳跃速度
    [SerializeField] private float _crouchSpeed = 1.0f; //爬行速度
    [SerializeField] private float _stickToGroundForce = 5.0f;  //地面支撑力
    [SerializeField] private float _gravityMultiplier = 2.5f;   //重力系数
    [SerializeField] private float _runStepLengthen = 0.75f;    //跑步步长
    [SerializeField] private CurveControlledBob _headBob = new CurveControlledBob();
    [SerializeField] private GameObject _flashLight = null;
    //鼠标控制
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook _mouseLook = new UnityStandardAssets.Characters.FirstPerson.MouseLook();

    private Camera _camera = null;
    private bool _jumpButtonPressed = false;    //跳跃判定
    private Vector2 _inputVector = Vector2.zero;    //输入值
    private Vector3 _moveDirection = Vector3.zero;  //移动方向
    private bool _previouslyGrounded = false;
    private bool _isWalking = true;     //行走判定
    private bool _isJumping = false;    //跳跃判定
    private bool _isCrouching = false;  //爬行判定
    private Vector3 _localSpaceCameraPos = Vector3.zero;    //本地坐标摄像机位置
    private float _controllerHeight = 0.0f;         //角色高度

    private float _fallingTimer = 0.0f;     //下落时间

    private CharacterController _characterController = null;    //角色控制器
    private PlayerMoveStatus _movementStatus = PlayerMoveStatus.NotMoving;
    
    public PlayerMoveStatus movementStatue { get { return _movementStatus; } }
    public float walkSpeed { get { return _walkSpeed; } }
    public float runSpeed { get { return _runSpeed; } }


    protected void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _controllerHeight = _characterController.height;
        _camera = Camera.main;
        _localSpaceCameraPos = _camera.transform.localPosition;
        _movementStatus = PlayerMoveStatus.NotMoving;
        _fallingTimer = 0.0f;
        //初始化MouseLook脚本
        _mouseLook.Init(transform, _camera.transform);
        _headBob.Initialize();
        _headBob.RegisterEventCallback(1.5f, PlayFootStepSound, CurveControlledBobCallbackType.Vertical);

        if (_flashLight) _flashLight.SetActive(false);
    }

    protected void Update()
    {
        //下落计时器，在地面上则为0
        if (_characterController.isGrounded) _fallingTimer = 0.0f;
        else _fallingTimer += Time.deltaTime;

        if (Time.timeScale > Mathf.Epsilon)
            _mouseLook.LookRotation(transform, _camera.transform);
        //手电筒触发
        if(Input.GetButtonDown("Flashlight"))
        {
            if (_flashLight) _flashLight.SetActive(!_flashLight.activeSelf);
        }
        //跳跃判定，不可在跳跃过程中按跳跃键
        if (!_jumpButtonPressed)
            _jumpButtonPressed = Input.GetButtonDown("Jump");
        //爬行判定
        if(Input.GetButtonDown("Crouch"))
        {
            _isCrouching = !_isCrouching;
            _characterController.height = _isCrouching == true ? _controllerHeight / 2.0f : _controllerHeight;  
        }
        //落地时刻
        if (!_previouslyGrounded && _characterController.isGrounded)
        {
            if (_fallingTimer > 0.5f)
            {
                //TODO:落地音效
            }

            _moveDirection.y = 0f;
            _isJumping = false;
            _movementStatus = PlayerMoveStatus.Landing;
        }
        else //空中
        if (!_characterController.isGrounded)
            _movementStatus = PlayerMoveStatus.NotGrounded;
        else  //静止
        if (_characterController.velocity.sqrMagnitude < 0.01f)
            _movementStatus = PlayerMoveStatus.NotMoving;
        else  //行走
        if (_isWalking)
            _movementStatus = PlayerMoveStatus.Walking;
        else  //跑步
            _movementStatus = PlayerMoveStatus.Runnig;

        _previouslyGrounded = _characterController.isGrounded;

    }

    // -------------------------------------------------------------------------
    // 方法	    :	FixedUpdate
    // 介绍		:	用于获取按键和调整方向
    // -------------------------------------------------------------------------
    protected void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool waswalking = _isWalking;
        _isWalking = !Input.GetKey(KeyCode.LeftShift);

        float speed = _isCrouching ? _crouchSpeed : _isWalking ? _walkSpeed : _runSpeed;
        _inputVector = new Vector2(horizontal, vertical);

        if (_inputVector.sqrMagnitude > 1) _inputVector.Normalize();

        Vector3 desiredMove = transform.forward * _inputVector.y + transform.right * _inputVector.x;

        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, _characterController.radius, Vector3.down, out hitInfo, _characterController.height/2f,1))
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        _moveDirection.x = desiredMove.x * speed;
        _moveDirection.z = desiredMove.z * speed;

        if(_characterController.isGrounded)
        {
            _moveDirection.y = -_stickToGroundForce;

            if(_jumpButtonPressed)
            {
                _moveDirection.y = _jumpSpeed;
                _jumpButtonPressed = false;
                _isJumping = true;
                //TODO:跳跃音效
            }
        }
        else
        {
            _moveDirection += Physics.gravity * _gravityMultiplier * Time.fixedDeltaTime;
        }

        _characterController.Move(_moveDirection * Time.fixedDeltaTime);

        Vector3 speedXZ = new Vector3(_characterController.velocity.x, 0.0f, _characterController.velocity.z);
        if (speedXZ.magnitude > 0.01f)
            _camera.transform.localPosition = _localSpaceCameraPos + _headBob.GetVectorOffset(speedXZ.magnitude * (_isCrouching||_isWalking?1.0f:_runStepLengthen));
        else
            _camera.transform.localPosition = _localSpaceCameraPos;
    }

    void PlayFootStepSound()
    {
        //if (_isCrouching) return;
        AudioSources[_audioToUse].Play();
        _audioToUse = (_audioToUse == 0) ? 1 : 0;
    }
}
