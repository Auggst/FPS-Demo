using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -------------------------------------------------------------------------
// 类	    :	CameraBloodEffect
// 介绍		:	角色受伤特效
// -------------------------------------------------------------------------
[ExecuteInEditMode()]
public class CameraBloodEffect : MonoBehaviour
{
    [SerializeField] Texture2D _bloodTexture = null;
    [SerializeField] Texture2D _bloodNormalMap = null;
    [SerializeField] private float _bloodAmount = 1.0f;
    [SerializeField] private float _minBloodAmount = 0.0f;
    [SerializeField] private float _distortion = 1.0f;
    [SerializeField] private bool _autoFade = true;
    [SerializeField] private float _fadeSpeed = 0.05f;
    [SerializeField] private Shader _shader = null;
    private Material _material = null;

    public float bloodAmount { get { return _bloodAmount; } set { _bloodAmount = value; } }
    public float minBloodAmount { get { return _minBloodAmount; } set { _minBloodAmount = value; } }
    public float fadeSpeed { get { return _fadeSpeed; } set { _fadeSpeed = value; } }
    public bool autoFade { get { return _autoFade; } set { _autoFade = value; } }

    private void Update()
    {
        if(_autoFade)
        {
            _bloodAmount -= _fadeSpeed * Time.deltaTime;
            _bloodAmount = Mathf.Max(_bloodAmount, _minBloodAmount);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_shader == null) return;
        if(_material==null)
        {
            _material = new Material(_shader);
        }

        if (_material == null) return;

        //把数据发送给Shader
        if(_bloodTexture!=null)
            _material.SetTexture("_BloodTex", _bloodTexture);
        if (_bloodNormalMap != null)
            _material.SetTexture("_BloodBump", _bloodNormalMap);

        _material.SetFloat("_Distortion", _distortion);
        _material.SetFloat("_BloodAmount", _bloodAmount);

        Graphics.Blit(source,destination,_material);
    }
}
