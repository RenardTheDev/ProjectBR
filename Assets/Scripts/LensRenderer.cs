using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LensRenderer : MonoBehaviour
{
    public WeaponDATA data;
    public float magnification = 10f;
    public float lensMag = 10f;
    Transform mainCamTr;
    Camera cam;
    Camera maincam;
    Quaternion rot;

    public UpdateMethod updateMethod = UpdateMethod.Physics;

    public Transform lensTrans;
    public float lensDiameter = 0.1f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        mainCamTr = Camera.main.transform;
        maincam = Camera.main;
    }

    public void ToggleLens(bool state)
    {
        cam.enabled = state;
        if (!state)
        {
            cam.targetTexture.Release();
            cam.targetTexture.DiscardContents();
        }
        else
        {
            cam.targetTexture.Release();

            cam.targetTexture.width = (int)(Screen.height / 2f);
            cam.targetTexture.height = (int)(Screen.height / 2f);

            //if (updateMethod != UpdateMethod.None) UpdateLens();

            cam.targetTexture.Create();
        }
    }

    Vector3 look;
    float dist;
    float size;
    public void UpdateLens()
    {
        if (cam.enabled)
        {
            look = (lensTrans.position - mainCamTr.position);
            dist = look.magnitude;
            size = ToPixelSize(dist, lensDiameter);

            cam.targetTexture.Release();

            cam.targetTexture.width = (int)size;
            cam.targetTexture.height = (int)size;

            cam.targetTexture.Create();

            cam.fieldOfView = maincam.fieldOfView / (magnification * Mathf.Pow(dist, lensMag) * Mathf.Rad2Deg);

            rot = Quaternion.LookRotation(look.normalized);
            Debug.DrawRay(transform.position, look.normalized, Color.green);

            transform.rotation = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y, transform.parent.eulerAngles.z);
        }
    }

    float ToPixelSize(float distance, float diameter)
    {
        float pixelSize = (diameter * Mathf.Rad2Deg * Screen.height) / (distance * maincam.fieldOfView);
        return pixelSize;
    }

    private void OnGUI()
    {
        GUILayout.Space(100);
        GUILayout.Label(cam.pixelRect.ToString());
        GUILayout.Label("dist = " + dist.ToString("0.00") + "\nsize = " + size.ToString("0.00"));
    }

    private void FixedUpdate()
    {
        //if (updateMethod == UpdateMethod.Physics) UpdateLens();
    }

    private void LateUpdate()
    {
        //if (updateMethod == UpdateMethod.Late) UpdateLens();
    }

    private void Update()
    {
        //if (updateMethod == UpdateMethod.Normal) UpdateLens();
    }
}
