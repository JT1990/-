// *************************************************************************************************************
// 创建者: 魏国栋
// 创建时间: 2020/07/28 11:06:12
// 功能: 
// 版 本：v 1.2.0
// ************************************************************************************************************* 

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TapTouch : MonoBehaviour
{
    public static TapTouch Instance;
    public Transform mainCamera;

    public Vector3 targetPosition;
    public float distance = 60f;
    public float minDistance = 20f;
    public float maxDistance = 120f;
    public float zoomSpeed = 1f;
    public float panSpeed = 1f;
    public float rotateSpeed = 1f;
    public float angleX;
    public float angleY;
    public Vector2 limitAngleX = new Vector2(10,90);
    public Vector2 limitAngleY = new Vector2(10,90);
    public float inertia = 3;      //惯性
    public bool canPan=true;

    public Transform target;
    private Vector3 lastTargetAngle;

    private Bounds limitBounds;

    private void Awake()
    {
        Instance = this;
        QualitySettings.shadowDistance = TapTouch.Instance.maxDistance;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitCamera();
        InitTarget();

    }

    // Update is called once per frame
    void Update()
    {
        UpdateTransform();
        UpdateMainCamera();
    }

    public void Reset()
    {
        targetPosition = Vector3.zero;
        distance = maxDistance;
        //angleX = 90;
        //angleY = 0;
    }

    public void Set(Vector3 pos, float rotationXAxis, float rotationYAxis, float distance)
    {
        //targetPosition = pos;
        angleX = rotationXAxis;
        angleY = DealAngleY(rotationYAxis);
        this.distance = distance;

    }

    private float DealAngleY(float _angleY)
    {
        if (_angleY - angleY > 360)
        {
            _angleY -= 360;
            return DealAngleY(_angleY);
        }
        else if (angleY - _angleY > 360)
        {
            _angleY += 360;
            return DealAngleY(_angleY);
        }
        return _angleY;
    }

    private void InitTarget()
    {
        if (!target)
        {
            target = new GameObject("[CameraTarget]").transform;
            transform.SetParent(target);
        }
    }

    private void InitCamera()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        InitInputGestureEvents();
    }

    private void OnDisable()
    {
        RemoveInputGestureEvents();
    }

    void InitInputGestureEvents()
    {
        InputGesture.Instance.onPan += InputGesture_onPan;
        InputGesture.Instance.onZoom += InputGesture_onZoom;
        InputGesture.Instance.onRotateAround += InputGesture_onRotateAround;
        //InputGesture.Instance.onDoubleClick += InputGesture_onDoubleClick;
    }

    void RemoveInputGestureEvents()
    {
        if (!InputGesture.Instance) return;
        InputGesture.Instance.onPan -= InputGesture_onPan;
        InputGesture.Instance.onZoom -= InputGesture_onZoom;
        InputGesture.Instance.onRotateAround -= InputGesture_onRotateAround;
        //InputGesture.Instance.onDoubleClick -= InputGesture_onDoubleClick;
    }

    /// <summary>
    /// 平移事件处理
    /// </summary>
    /// <param name="delta"></param>
    void InputGesture_onPan(Vector2 delta)
    {
        delta *= panSpeed * Time.deltaTime * 0.1f * distance;
        Vector3 vUp = Quaternion.Euler(0, 90, 0) * target.right;
        Vector3 temp = targetPosition + target.right * -delta.x + vUp * delta.y;
        targetPosition = ApplyLimitBounds(temp.x, targetPosition.y, temp.z);
    }

    /// <summary>
    /// 旋转事件处理
    /// </summary>
    /// <param name="delta"></param>
    private void InputGesture_onRotateAround(Vector2 delta)
    {
        float factor = Time.deltaTime * 20 * rotateSpeed;
        angleX += -delta.y * factor;
        angleY += delta.x * factor;
        if (limitAngleX.x!=0 || limitAngleX.y!=0)
            angleX = ClampAngle(angleX, limitAngleX.x, limitAngleX.y);
        if (limitAngleY.x != 0 || limitAngleY.y != 0)
            angleY = ClampAngle(angleY, limitAngleY.x, limitAngleY.y);
    }

    /// <summary>
    /// 缩放事件处理
    /// </summary>
    /// <param name="delta"></param>
    private void InputGesture_onZoom(float delta)
    {
        //if (EventSystem.current.IsPointerOverGameObject()) return;
        distance = -transform.localPosition.z - delta * zoomSpeed * distance * Time.deltaTime;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    /// <summary>
    /// 更新Target、虚拟相机 Transform
    /// </summary>
    void UpdateTransform()
    {
        var dir = new Vector3(angleX, angleY, 0);
        lastTargetAngle = Vector3.Lerp(lastTargetAngle, dir, 0.02f * inertia);
        target.eulerAngles = lastTargetAngle;
        if (canPan) target.position = Vector3.Lerp(target.position, targetPosition, 0.02f * inertia);
        Vector3 dirPos = Vector3.back * distance;
        transform.localPosition = Vector3.Lerp(transform.localPosition, dirPos, 0.02f * inertia);
    }

    /// <summary>
    /// 更新主相机Transform
    /// </summary>
    void UpdateMainCamera()
    {
        mainCamera.transform.position = transform.position;
        mainCamera.transform.rotation = transform.rotation;
    }

    /// <summary>
    /// 角度限制
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    #region 根据当前展示的模型限制相机 移动范围 及相机 最大高度
    public void SetLimitBounds(Bounds bounds)
    {
        limitBounds = bounds;
        RemoveInputGestureEvents();
        InitInputGestureEvents();
        SetMaxDistance();
    }

    /// <summary>
    /// 根据展示的模型边界计算相机高度最大值
    /// </summary>
    /// <returns></returns>
    private void SetMaxDistance()
    {
        if (limitBounds.extents != Vector3.zero)
        {
            float tan = Mathf.Tan(Camera.main.fieldOfView / 2 * Mathf.Deg2Rad);
            float helfC = Vector3.Distance(limitBounds.center, limitBounds.center + limitBounds.extents);
            //maxDistance = helfC / tan;
        }
    }

    private Vector3 ApplyLimitBounds(float x,float y, float z)
    {
        if (limitBounds.extents != Vector3.zero)
        {
            Vector3 v1 = limitBounds.center - limitBounds.extents;
            Vector3 v2 = limitBounds.center + limitBounds.extents;
            x = Mathf.Clamp(x, v1.x, v2.x);
            z = Mathf.Clamp(z, v1.z, v2.z);
        }
        return new Vector3(x, y, z);
    }
    #endregion

    #region 双击操作

    void InputGesture_onDoubleClick(Vector2 vector2)
    {
        //Reset();
    }

    #endregion
}
