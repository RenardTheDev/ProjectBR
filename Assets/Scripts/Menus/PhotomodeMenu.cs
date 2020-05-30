using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System;
using System.IO;

public class PhotomodeMenu : MonoBehaviour
{
    public static PhotomodeMenu inst;
    public GameObject controls_container;

    public Button btn_takePicture;

    public Button[] btn_fov;
    public Button[] btn_dutch;

    public Text pathLabel;

    public FixedJoystick joy;
    public FixedTouchField field;

    public float flySpeed = 5f;

    public int ScrShotWidth = 1280;
    public int ScrShotHeight = 720;


    private void Awake()
    {
        inst = this;

        pathLabel.text = ScreenShotName(ScrShotWidth, ScrShotHeight);

        cam = Camera.main;

        btn_fov[0].onClick.AddListener(() => { ChangeFov(-5); });
        btn_fov[1].onClick.AddListener(() => { ChangeFov(5); });

        btn_dutch[0].onClick.AddListener(() => { ChangeDutch(-5); });
        btn_dutch[1].onClick.AddListener(() => { ChangeDutch(5); });

        btn_takePicture.onClick.AddListener(() => TakePicture(ScrShotWidth, ScrShotHeight));
    }

    void ChangeFov(int value)
    {
        cm_cam.m_Lens.FieldOfView += value;
        if (cm_cam.m_Lens.FieldOfView > 120) cm_cam.m_Lens.FieldOfView = 120;
        if (cm_cam.m_Lens.FieldOfView < 10) cm_cam.m_Lens.FieldOfView = 10;
    }

    void ChangeDutch(int value)
    {
        cm_cam.m_Lens.Dutch += value;
        if (cm_cam.m_Lens.Dutch > 180) cm_cam.m_Lens.FieldOfView = 180;
        if (cm_cam.m_Lens.Dutch < -180) cm_cam.m_Lens.FieldOfView = -180;
    }

    public void EnterPhotomode()
    {
        controls_container.SetActive(true);
        CameraControllerBase.current.TogglePhotomodeCamera(true);

        cm_Trans.position = cam.transform.position;
        //cm_Trans.rotation = cam.transform.rotation;
        look = cam.transform.eulerAngles;
        look.z = 0;
        cm_cam.m_Lens.FieldOfView = 60;
    }

    public void ExitPhotomode()
    {
        controls_container.SetActive(false);
        CameraControllerBase.current.TogglePhotomodeCamera(false);
    }

    Vector3 look;
    public float sens;
    private void Update()
    {
        cm_Trans.Translate(new Vector3(joy.InputVector.x, 0, joy.InputVector.y) * Time.unscaledDeltaTime * flySpeed);
        look.x -= field.TouchDist.y * sens;
        look.y += field.TouchDist.x * sens;

        look.x = Mathf.Clamp(look.x, -89, 89);
        look.y = Mathf.Repeat(look.y, 360);

        cm_Trans.rotation = Quaternion.Euler(look);
    }

    Camera cam;
    public Transform cm_Trans;
    public CinemachineVirtualCamera cm_cam;

    void TakePicture(int resWidth, int resHeight)
    {
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24)
        {
            anisoLevel = 16,
            antiAliasing = 4,
            filterMode = FilterMode.Trilinear
        };

        cam.targetTexture = rt;

        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);

        cam.Render();

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);

        cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(resWidth, resHeight);
        System.IO.File.WriteAllBytes(filename, bytes);

        Debug.Log(string.Format("Took screenshot to: {0}", filename));
    }

    public static string ScreenShotName(int width, int height)
    {
        return string.Format("{0}/screen_{1}x{2}_{3}.png",
                             GetAndroidScreenshotFolder(),
                             width, height,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    private static string GetAndroidScreenshotFolder()
    {
        string path = Application.dataPath;

        if (Application.platform == RuntimePlatform.Android)
        {
            path = Application.persistentDataPath;
        }
        return path;
    }
}