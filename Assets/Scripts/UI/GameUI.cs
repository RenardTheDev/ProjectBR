using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public static GameUI current;
    private void Awake()
    {
        if (current != null)
        {
            Destroy(gameObject);
        }
        else
        {
            current = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public bool invOpened;

    private void Start()
    {
        SceneManager.activeSceneChanged += ActiveSceneChanged;
        ToggleUICanvas("mainmenu", true);
    }

    private void ActiveSceneChanged(Scene from, Scene to)
    {
        Debug.Log("ActiveSceneChanged: from.buildIndex = " + from.buildIndex + " | to.buildIndex = " + to.buildIndex);
        DisableAllUICanvas();
        if (to.buildIndex == 0)
        {
            EnableUICanvas("mainmenu");
        }
    }

    public List<CanvasPacket> canvas;

    private void OnValidate()
    {
        foreach (CanvasPacket c in canvas)
        {
            c.name = c.go.name;
            c.tagHash = c.tag.GetHashCode();
            c.canvas = c.go.GetComponentsInChildren<Canvas>();
        }
    }

    public void UpdateCanvasList()
    {
        List<Canvas> cList = new List<Canvas>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i).GetComponent<Canvas>();
            if (c != null)
            {
                cList.Add(c);
            }
        }

        canvas = new List<CanvasPacket>();
        for (int i = 0; i < cList.Count; i++)
        {
            canvas.Add(new CanvasPacket(cList[i].gameObject));
        }
    }

    Dictionary<int, bool> pauseCanvasSave;

    private void Update()
    {
        if (TouchController.current.button_inventory.state == bindState.down)
        {
            if (GameManager.current.gameState == GameState.gameplay && !invOpened)
            {
                invOpened = true;

                ToggleUICanvas("controls", false);
                ToggleUICanvas("inv", true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (GameManager.current.gameState)
            {
                case GameState.loading:
                    // nothing to do here
                    break;
                case GameState.mainmenu:
                    {
                        if (IsUICanvasActive("settings"))
                        {
                            CloseSettings();
                        }
                        if (IsUICanvasActive("custom"))
                        {
                            CloseCustomization();
                        }
                        break;
                    }
                case GameState.gameplay:
                    {
                        if (invOpened)
                        {
                            invOpened = false;

                            ToggleUICanvas("controls", true);
                            ToggleUICanvas("inv", false);
                        }
                        else
                        {
                            TogglePause(true);
                        }
                        break;
                    }
                case GameState.pause:
                    {
                        if (IsUICanvasActive("settings"))
                        {
                            CloseSettings();
                        }
                        else
                        {
                            TogglePause(false);
                        }
                        break;
                    }
                default:
                    break;
            }
        }
    }

    //--- Windows togglers ---
    public void TogglePause(bool toggle)
    {
        if (toggle && GameManager.current.gameState == GameState.gameplay)
        {
            GameManager.current.gameState = GameState.pause;

            ToggleUICanvas("scr_effect", false);
            ToggleUICanvas("msg", false);
            ToggleUICanvas("controls", false);
            ToggleUICanvas("inv", false);
            ToggleUICanvas("player", false);

            ToggleUICanvas("pause", true);
        }
        else if (!toggle && GameManager.current.gameState == GameState.pause)
        {
            GameManager.current.gameState = GameState.gameplay;
            ToggleUICanvas("pause", false);
            if (invOpened)
            {
                ToggleUICanvas("inv", true);
            }
            else
            {
                ToggleUICanvas("scr_effect", true);
                ToggleUICanvas("msg", true);
                ToggleUICanvas("controls", true);
                ToggleUICanvas("player", true);
            }
        }
    }

    public void CloseCustomization()
    {
        if (GameManager.current.gameState == GameState.mainmenu)
        {
            ToggleUICanvas("custom", false);
            ToggleUICanvas("mainmenu", true);
        }
    }

    public void CloseSettings()
    {
        if (IsUICanvasActive("settings"))
        {
            if (GameManager.current.gameState == GameState.mainmenu)
            {
                ToggleUICanvas("settings", false);
                ToggleUICanvas("mainmenu", true);
            }
            else if (GameManager.current.gameState == GameState.pause)
            {
                ToggleUICanvas("settings", false);
                ToggleUICanvas("pause", true);
            }
        }
    }

    //--- UICanvas togglers ---
    public void EnableUICanvas(string tag)
    {
        ToggleUICanvas(tag, true);
    }
    public void DisableUICanvas(string tag)
    {
        ToggleUICanvas(tag, false);
    }
    public void ToggleUICanvas(string tag, bool state)
    {
        _toggleUICanvas(tag.GetHashCode(), state);
    }
    public void ToggleUICanvas(int tagHash, bool state)
    {
        _toggleUICanvas(tagHash, state);
    }

    void _toggleUICanvas(int tagHash, bool enable)
    {
        var target = canvas.Find(x => x.tagHash == tagHash);

        if (target != null)
        {
            if (tagHash == "mainmenu".GetHashCode())
            {
                if (enable)
                {
                    for (int i = 0; i < target.go.transform.childCount; i++)
                    {
                        target.go.transform.GetChild(i).gameObject.SetActive(i == 0);
                    }
                }
            }
            if (tagHash == "controls".GetHashCode())
            {
                for (int i = 1; i < target.canvas.Length; i++)
                {
                    target.canvas[i].gameObject.SetActive(enable);
                }
            }

            for (int i = 0; i < target.canvas.Length; i++) { target.canvas[i].enabled = enable; }
        }
        else
        {
            Debug.LogError("Can't find canvas packet tagged as \'" + tagHash + "\'");
        }
    }

    public void DisableAllUICanvas()
    {
        foreach (CanvasPacket c in canvas)
        {
            for (int i = 0; i < c.canvas.Length; i++) { c.canvas[i].enabled = false; }
        }
    }

    public bool IsUICanvasActive(string tag)
    {
        var target = canvas.Find(x => x.tagHash == tag.GetHashCode());

        if (target != null)
        {
            return target.canvas[0].enabled;
        }
        else
        {
            Debug.LogError("Can't find canvas packet tagged as \'" + tag + "\'");
            return false;
        }
    }
}

[System.Serializable]
public class CanvasPacket
{
    [HideInInspector]public string name;
    public string tag;
    public GameObject go;
    public Canvas[] canvas;
    public int tagHash;

    public CanvasPacket(GameObject newGO)
    {
        go = newGO;
        name = go.name;
        tag = name;
        tagHash = tag.GetHashCode();

        canvas = go.GetComponentsInChildren<Canvas>();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameUI))]
public class GameUIEditor : Editor
{
    GameUI script;
    bool showAssets;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (script == null) script = (GameUI)target;
        if (GUILayout.Button("UpdateCanvasList"))
        {
            script.UpdateCanvasList();
        }

    }
}
#endif