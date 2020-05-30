using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CustomCameraProjection : MonoBehaviour
{
    Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (cam == null) cam = GetComponent<Camera>();

        if (!useManual) cam.ResetProjectionMatrix();
        Matrix4x4 m = PerspectiveOffCenter(cam.projectionMatrix);
        cam.projectionMatrix = m;
    }

    [Header("Manual matrix")]
    public bool useManual = false;
    public Vector4[] matrix = new Vector4[4];

    Matrix4x4 PerspectiveOffCenter(Matrix4x4 origMatrix)
    {
        Matrix4x4 m = origMatrix;

        //m.m00 = Mathf.Sin(Time.time * 1.2f) * 0.1f;
        if (!useManual)
        {
            matrix[0].x = m.m00;
            matrix[0].y = m.m01;
            matrix[0].z = m.m02;
            matrix[0].w = m.m03;

            matrix[1].x = m.m10;
            matrix[1].y = m.m11;
            matrix[1].z = m.m12;
            matrix[1].w = m.m13;

            matrix[2].x = m.m20;
            matrix[2].y = m.m21;
            matrix[2].z = m.m22;
            matrix[2].w = m.m23;

            matrix[3].x = m.m30;
            matrix[3].y = m.m31;
            matrix[3].z = m.m32;
            matrix[3].w = m.m33;
        }
        else
        {
            m.m00 = matrix[0].x;
            m.m01 = matrix[0].y;
            m.m02 = matrix[0].z;
            m.m03 = matrix[0].w;

            m.m10 = matrix[1].x;
            m.m11 = matrix[1].y;
            m.m12 = matrix[1].z;
            m.m13 = matrix[1].w;

            m.m20 = matrix[2].x;
            m.m21 = matrix[2].y;
            m.m22 = matrix[2].z;
            m.m23 = matrix[2].w;

            m.m30 = matrix[3].x;
            m.m31 = matrix[3].y;
            m.m32 = matrix[3].z;
            m.m33 = matrix[3].w;
        }

        return m;
    }

    [Header("Debug")]
    public float GUI_gridWidth = 64;
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(cam.projectionMatrix.m00.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m01.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m02.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m03.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(cam.projectionMatrix.m10.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m11.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m12.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m13.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(cam.projectionMatrix.m20.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m21.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m22.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m23.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(cam.projectionMatrix.m30.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m31.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m32.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.Label(cam.projectionMatrix.m33.ToString("0.00"), GUILayout.Width(GUI_gridWidth));
        GUILayout.EndHorizontal();
    }

    private void OnDisable()
    {
        cam.ResetProjectionMatrix();
    }
}