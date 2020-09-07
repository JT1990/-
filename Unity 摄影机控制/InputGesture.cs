// *************************************************************************************************************
// 创建者: 魏国栋
// 创建时间: 2020/07/28 11:06:12
// 功能: 
// 版 本：v 1.2.0
// ************************************************************************************************************* 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public enum Gesture {
    Default = 40100,        //默认
    SingleTap = 40101,      //单指操作
    DoubleTap = 40102,      //双指操作
    TripleTap = 40103,      //三指操作
    LongPressTap = 40104    //长按
}

public class InputGesture : MonoBehaviour
{
    /// <summary>
    /// 缩放事件（鼠标滚轮、触屏双指异向滑动）
    /// </summary>
    public Action<float> onZoom;
    /// <summary>
    /// 平移事件（鼠标滚轮拖拽、触屏双指同向滑动）
    /// </summary>
    public Action<Vector2> onPan;
    /// <summary>
    /// 旋转事件（鼠标左键拖拽、触屏单指滑动）
    /// </summary>
    public Action<Vector2> onRotateAround;
    /// <summary>
    /// 点击事件(立即执行）（鼠标左键点击、触屏点击）
    /// </summary>
    public Action<Vector2> onClick;
    /// <summary>
    /// 鼠标右键点击事件(立即执行）
    /// </summary>
    public Action<Vector2> onRightClick;
    /// <summary>
    /// 单击事件(有延时，为了区分双击）（鼠标左键点击、触屏点击）
    /// </summary>
    public Action<Vector2> onSingleClick;
    /// <summary>
    /// 双击事件（鼠标左键双击、触屏双击）
    /// </summary>
    public Action<Vector2> onDoubleClick;

    private Vector2 lastMousePosition;
    private Vector2 lastTouchPosition1;
    private Vector2 lastTouchPosition2;
    private Vector3 lastMouseDownPosition;
    private float lastDoubleClickTime;
    private static bool _applicationIsQuitting = false;

    public Gesture gesture;

    private static InputGesture instance;
    public static InputGesture Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                return null;
            }
            if (instance == null)
            {
                instance = new GameObject($"{nameof(InputGesture)}").AddComponent<InputGesture>();
            }
            return instance;
        }
    }
    private void Awake()
    {
        instance = this;
    }

   

    public void OnDestroy()
    {
        _applicationIsQuitting = true;
    }

    // Update is called once per frame
    void Update()
    {
        //if (EventSystem.current.IsPointerOverGameObject()) return;
#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )
        DealRotateAroundTouch();
        DealPanOrZoomTouch();
#else
        DealPanMouse();
        DealRotateAroundMouse();
        DealZoomMouse();
        DealRightClickEvent();
#endif
        DealClickEvent();

    }

    void DealClickEvent()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMouseDownPosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (Vector3.Distance( lastMouseDownPosition, Input.mousePosition)<16)
            //if (lastMouseDownPosition == Input.mousePosition)
            {
                onClick?.Invoke(lastMouseDownPosition);
                Invoke("IeMonitor", 0.3f);
            }
        }
    }

    void DealRightClickEvent()
    {

        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(1))
        {
            onPan?.Invoke((Vector2)Input.mousePosition - lastMousePosition);
            lastMousePosition = Input.mousePosition;
        }

        //if (Input.GetMouseButtonDown(1))
        //{
        //    lastMouseDownPosition = Input.mousePosition;
        //}
        //if (Input.GetMouseButtonUp(1))
        //{
        //    if (lastMouseDownPosition == Input.mousePosition)
        //    {
        //        onRightClick?.Invoke(lastMouseDownPosition);
        //        Invoke("IeMonitor", 0.3f);
        //    }
        //}
    }
    private void OnGUI()
    {
        if (!EventSystem.current) return;
        //if (EventSystem.current.IsPointerOverGameObject()) return;
        if (Event.current.isMouse && Event.current.button == 0 && Event.current.clickCount > 1)
        {
            lastDoubleClickTime = Time.realtimeSinceStartup;
            gesture = Gesture.DoubleTap;
            onDoubleClick?.Invoke(Input.mousePosition);
        }

        //GUILayout.TextField(Input.mousePosition.ToString());
    }

    void IeMonitor()
    {
        if (Time.realtimeSinceStartup - lastDoubleClickTime > 1f)
        {
            onSingleClick?.Invoke(lastMouseDownPosition);
            gesture = Gesture.SingleTap;

        }
    }

    /// <summary>
    /// 处理平移
    /// </summary>
    void DealPanMouse()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            onPan?.Invoke((Vector2)Input.mousePosition - lastMousePosition);
            lastMousePosition = Input.mousePosition;
        }
    }

    /// <summary>
    /// 处理旋转
    /// </summary>
    void DealRotateAroundMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            onRotateAround?.Invoke((Vector2)Input.mousePosition - lastMousePosition);
            lastMousePosition = Input.mousePosition;
        }
    }

    /// <summary>
    /// 处理旋转
    /// </summary>
    void DealZoomMouse()
    {
        float delta = Input.GetAxis("Mouse ScrollWheel");
        if (delta != 0)
        {
            onZoom?.Invoke(delta * 100);
        }
    }

    void DealRotateAroundTouch()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                lastMousePosition = Input.mousePosition;
            }
            if (touch.phase == TouchPhase.Moved)
            {
                onRotateAround?.Invoke((Vector2)touch.position - lastMousePosition);
                lastMousePosition = Input.mousePosition;
            }
        }
    }

    void DealPanOrZoomTouch()
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            lastTouchPosition1 = Input.GetTouch(0).position;
        }
        else if (Input.touchCount == 2)
        {
            if (Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
            {
                lastTouchPosition1 = Input.GetTouch(0).position;
            }
            if (Input.GetTouch(1).phase == UnityEngine.TouchPhase.Began)
            {
                lastTouchPosition2 = Input.GetTouch(1).position;
            }

            if (Input.GetTouch(0).phase == UnityEngine.TouchPhase.Moved || Input.GetTouch(1).phase == UnityEngine.TouchPhase.Moved)
            {
                var tempPosition1 = Input.GetTouch(0).position;
                var tempPosition2 = Input.GetTouch(1).position;

                var dir1 = tempPosition1 - lastTouchPosition1;
                var dir2 = tempPosition2 - lastTouchPosition2;
                if (Vector3.Dot(dir1, dir2) > 0)
                {
                    onPan?.Invoke(dir1);
                }
                else
                {
                    float currentTouchDistance = Vector3.Distance(tempPosition1, tempPosition2);
                    float lastTouchDistance = Vector3.Distance(lastTouchPosition1, lastTouchPosition2);
                    onZoom?.Invoke(currentTouchDistance - lastTouchDistance);
                }
                lastTouchPosition1 = tempPosition1;
                lastTouchPosition2 = tempPosition2;
            }
        }
    }

}
