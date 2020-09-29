using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IconGenerator : MonoBehaviour
{
    public static IconGenerator current;

    public Transform holder;
    public Camera cam;

    public ItemObject test_item;
    public int test_size;
    public List<Texture2D> test_output;

    public GameObject test_go;

    Vector3 center;
    float modelWidth;
    float modelHeight;

    public Color iconColor = Color.white;

    Vector3 point;
    float bottom;
    float top;
    float left;
    float right;
    public Texture2D GenerateItemIcon(int size, ItemObject item)
    {
        var go = Instantiate(item.prefab_w, holder);
        Renderer[] r = go.GetComponentsInChildren<Renderer>();

        center = Vector3.zero;
        modelWidth = 0;
        modelHeight = 0;

        bottom = 0;
        top = 0;
        left = 0;
        right = 0;

        for (int i = 0; i < r.Length; i++)
        {
            point = r[i].bounds.ClosestPoint(Vector3.up * 1000);
            if (point.y > top) top = point.y;

            point = r[i].bounds.ClosestPoint(Vector3.down * 1000);
            if (point.y < bottom) bottom = point.y;

            point = r[i].bounds.ClosestPoint(Vector3.right * 1000);
            if (point.x > right) right = point.x;

            point = r[i].bounds.ClosestPoint(Vector3.left * 1000);
            if (point.x < left) left = point.x;
        }

        modelHeight = top - bottom;
        modelWidth = right - left;

        center = new Vector3(0.5f * (right + left), 0.5f * (top + bottom), 0);

        Debug.Log($"item \'{item.Name}\' has modelWidth={modelWidth} and modelHeight={modelHeight}");

        cam.transform.position = center + Vector3.back * 2;
        cam.orthographicSize = Mathf.Min(modelWidth, modelHeight) * 0.6f;

        float aspect = modelWidth / modelHeight;

        int width = Mathf.RoundToInt(size * aspect);
        int height = size;

        Texture2D icon = TakePicture(width, height);

        if (format == TextureFormat.Alpha8)
        {
            Texture2D tempTex = new Texture2D(width, height, TextureFormat.ARGB4444, true);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pix = icon.GetPixel(x, y);
                    tempTex.SetPixel(x, y, new Color(iconColor.r, iconColor.g, iconColor.b, iconColor.a * pix.a));
                }
            }

            icon = tempTex;
        }

        icon.Apply();
        icon.name = "icon_" + item.Name;

        DestroyImmediate(go);

        //test_output = icon;
        test_output.Add(icon);

        return icon;
    }

    private void OnDrawGizmos()
    {
        if (test_go != null)
        {
            Renderer[] r = test_go.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < r.Length; i++)
            {
                Gizmos.color = Color.red;
                point = r[i].bounds.ClosestPoint(Vector3.up * 1000);
                Gizmos.DrawCube(point, Vector3.one * 0.05f);

                Gizmos.color = Color.green;
                point = r[i].bounds.ClosestPoint(Vector3.down * 1000);
                Gizmos.DrawCube(point, Vector3.one * 0.05f);

                Gizmos.color = Color.blue;
                point = r[i].bounds.ClosestPoint(Vector3.right * 1000);
                Gizmos.DrawCube(point, Vector3.one * 0.05f);

                Gizmos.color = Color.yellow;
                point = r[i].bounds.ClosestPoint(Vector3.left * 1000);
                Gizmos.DrawCube(point, Vector3.one * 0.05f);
            }

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(center, 0.025f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(0,top,0), 0.025f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(0, bottom, 0), 0.025f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(new Vector3(right, 0, 0), 0.025f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(new Vector3(left, 0, 0), 0.025f);
        }
    }

    public void GenerateInventoryIcons()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemObject");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var item = AssetDatabase.LoadAssetAtPath<ItemObject>(path);

            Texture2D icon = GenerateItemIcon(test_size, item);

            string iconPath = IconSavePath(path);

            byte[] bytes = icon.EncodeToPNG();
            string filename = iconPath + icon.name + ".png";
            System.IO.File.WriteAllBytes(filename, bytes);

            item.icon = AssetDatabase.LoadAssetAtPath<Sprite>(filename);
            if (item is WeaponObject) ((WeaponObject)item).weapon.icon = item.icon;

            Debug.Log($"icon path: \'{filename}\'");
        }
    }

    string IconSavePath(string itemPath)
    {
        string save = "";

        var parts = itemPath.Split('/');
        save = itemPath.Replace(parts[parts.Length - 1], string.Empty);

        return save;
    }

    public TextureFormat format = TextureFormat.RGBA32;
    Texture2D TakePicture(int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 16)
        {
            antiAliasing = 4,
            filterMode = FilterMode.Bilinear
        };

        cam.targetTexture = rt;

        Texture2D picture = new Texture2D(width, height, format, true);

        picture.wrapMode = TextureWrapMode.Clamp;

        cam.Render();

        RenderTexture.active = rt;
        picture.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        DestroyImmediate(rt);

        return picture;
    }

    float maxSize(Vector3 size)
    {
        return Mathf.Min(size.x, size.y);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(IconGenerator))]
public class IconGeneratorEditor : Editor
{
    IconGenerator script;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (IconGenerator)target;
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate test icon"))
        {
            if (script.test_output == null)
            {
                script.test_output = new List<Texture2D>();
            }
            else
            {
                script.test_output.Clear();
            }

            script.GenerateItemIcon(script.test_size, script.test_item);
        }

        if (GUILayout.Button("Generate icons"))
        {
            if (script.test_output == null)
            {
                script.test_output = new List<Texture2D>();
            }
            else
            {
                script.test_output.Clear();
            }

            script.GenerateInventoryIcons();
        }

        if (script.test_output != null && script.test_output.Count > 0)
        {
            for (int i = 0; i < script.test_output.Count; i++)
            {
                GUILayout.Label(script.test_output[i]);
            }
        }
    }
}
#endif