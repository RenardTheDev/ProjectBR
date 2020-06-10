using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class CanvasHelper
{
    static Texture2D[] canvasState;

    static Dictionary<int, CanvasType> canvases;

    static CanvasHelper()
    {
        canvasState = new[]
        {
            AssetDatabase.LoadAssetAtPath("Assets/Editor/CanvasHelperIcons/disabled.png", typeof(Texture2D)) as Texture2D,
            AssetDatabase.LoadAssetAtPath("Assets/Editor/CanvasHelperIcons/enabled.png", typeof(Texture2D)) as Texture2D
        };

        EditorApplication.update += UpdateCB;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
    }

    static void UpdateCB()
    {
        GameObject[] go = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];

        canvases = new Dictionary<int, CanvasType>();

        foreach (GameObject g in go)
        {
            var ct = g.GetComponent<CanvasType>();
            if (ct != null)
            {
                canvases.Add(g.GetInstanceID(), ct);
            }
        }
    }

    static void HierarchyItemCB(int instID, Rect selectionRect)
    {
        if (canvases != null)
        {
            GUI.color = Color.white;

            if (canvases.ContainsKey(instID))
            {
                Rect r = new Rect(selectionRect);
                /*r.x = r.x-32;
                GUI.Label(r, canvasState[canvases[instID].canvas.enabled ? 1 : 0]);*/

                if (!canvases[instID].canvas.enabled)
                {
                    GUI.color = Color.black;
                    r = new Rect(selectionRect);
                    r.y = r.y + r.height / 2;
                    r.x = r.x + 16;
                    r.height = 1;
                    GUI.Box(r, "");
                }

                Color c = canvases[instID].OverlayColor;
                c.a = 0.25f;
                GUI.color = c;

                r = new Rect(selectionRect);
                r.width = r.width + 32;
                GUI.Box(r, "");
            }

            GUI.color = Color.white;
        }
    }
}