using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public struct CameraParameter
{
    //Limit the rotation angle on the X axis
    public bool limitXAngle;

    public float minXAngle;
    public float maxXAngle;

    public bool limitYAngle;

    public float minYAngle;
    public float maxYAngle;

    //Sliding inertia
    public float orbitSensitive;

    //rotate speed
    public float mouseMoveRotio;

    public CameraParameter(bool limitXAngle = true,
        float minXAngle = 0,
        float maxXAngle = 80f,
        bool limitYAngle = false,
        float minYAngle = 0f,
        float maxYAngle = 90f,
        float orbitSensitive = 10f,
        float mouseMoveRotio = 0.3f)
    {
        this.limitXAngle = limitXAngle;
        this.minXAngle = minXAngle;
        this.maxXAngle = maxXAngle;
        this.limitYAngle = limitYAngle;
        this.minYAngle = minYAngle;
        this.maxYAngle = maxYAngle;
        this.orbitSensitive = orbitSensitive;
        this.mouseMoveRotio = mouseMoveRotio;
    }


}



public class CameraOrbit : MonoBehaviour
{

    private Vector3 lastMousePos;

    private Vector3 targetEulerAngle;

    private Vector3 eulerAngle;


    public CameraParameter freeOrbitParameter;

    private CameraParameter cureentCameraParameter;


    public Transform cameraRootTf;

    public Transform cameraTf;


    private float cameraDistance;

    private float targetCameraDistance;


    private float lastTouchDistance;

    //The closest distance the camera advances
    public float minDistance = 30f;

    public float maxDistance = 100f;

    //Camera advances speed
    public float mouseScroollRatio = 2f;

    //Camera propulsion sensitivity
    public float zoomSensitive = 5f;

    private Quaternion originalRotate;

    public float[] yMinAngles;

    public float[] yMaxAngles;

    public bool[] isAlreadyFire;


    void Start()
    {
        originalRotate = cameraRootTf.rotation;
        cameraDistance = cameraTf.localPosition.z;
        targetCameraDistance = cameraDistance;
        cureentCameraParameter = freeOrbitParameter;

        isAlreadyFire = new bool[yMinAngles.Length];
    }

    void Update()
    {
        Oribit();
        Zoom();
    }

    //Camera rotation method
    private void Oribit()
    { 
        if (UnityEngine.Input.GetMouseButtonDown(0))
        {
            lastMousePos = Input.mousePosition;
        }


        if (Input.GetMouseButton(0))
        {
            targetEulerAngle.x += -1 * (Input.mousePosition.y - lastMousePos.y) * cureentCameraParameter.mouseMoveRotio;
            targetEulerAngle.y += (Input.mousePosition.x - lastMousePos.x) * cureentCameraParameter.mouseMoveRotio;


            if (cureentCameraParameter.limitXAngle)
            {
                targetEulerAngle.x = Mathf.Clamp(targetEulerAngle.x, cureentCameraParameter.minXAngle, cureentCameraParameter.maxXAngle);
            }


            if (cureentCameraParameter.limitYAngle)
            {
                targetEulerAngle.y = Mathf.Clamp(targetEulerAngle.y, cureentCameraParameter.minYAngle, cureentCameraParameter.maxYAngle);
            }

            lastMousePos = Input.mousePosition;
        }


        if (Input.touchCount < 2)
        {
            eulerAngle = Vector3.Lerp(eulerAngle, targetEulerAngle, Time.fixedDeltaTime * cureentCameraParameter.orbitSensitive);

            cameraRootTf.rotation = originalRotate * Quaternion.Euler(eulerAngle);

        }

        FireEvent(cameraRootTf.localEulerAngles.y);
    }

    //Camera propulsion method
    private void Zoom()
    {
        if (Input.touchCount < 2)
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                cameraDistance = -cameraTf.localPosition.z;
                targetCameraDistance = cameraDistance - Input.GetAxis("Mouse ScrollWheel") * cameraDistance * mouseScroollRatio;
                targetCameraDistance = Mathf.Clamp(targetCameraDistance, minDistance, maxDistance);
            }
        }
        else
        {
            if (Input.GetTouch(1).phase == TouchPhase.Began)
            {
                lastTouchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);

            }
            if (Input.GetTouch(1).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Moved)

            {
                cameraDistance = -cameraTf.localPosition.z;
                targetCameraDistance = cameraDistance - (Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) - lastTouchDistance) * mouseScroollRatio;
                targetCameraDistance = Mathf.Clamp(targetCameraDistance, minDistance, maxDistance);
                lastMousePos = Input.mousePosition;

            }
        }
        if (Mathf.Abs(targetCameraDistance - cameraDistance) > 0.1f)
        {
            cameraDistance = Mathf.Lerp(cameraDistance, targetCameraDistance, Time.fixedDeltaTime * zoomSensitive);
            cameraTf.localPosition = new Vector3(0f, 0f, -cameraDistance);
        }
    }

    /// <summary>
    /// Trigger line animation event
    /// </summary>
    /// <param name="yAngle">Camera rotation angle</param>
    private void FireEvent(float yAngle)
    {
        for (int i = 0; i < yMinAngles.Length; i++)
        {
            if (yAngle > yMinAngles[i] && yAngle < yMaxAngles[i])
            {
                if (!isAlreadyFire[i])
                {
                    Debug.Log("右");
                    //EventCenter.CameraEvent.RaiseCameraReachAngle(i, true);
                    isAlreadyFire[i] = true;
                }
            }
            else
            {
                if (isAlreadyFire[i])
                {
                    Debug.Log("左");
                    //EventCenter.CameraEvent.RaiseCameraReachAngle(i, false);
                    isAlreadyFire[i] = false;
                }
            }
        }
    }
}
