using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;
    public GlobalSettings settings;

    GameObject SettingsWindow;
    public Text configPathLabel;
    public Button closeSettings;

    [Header("FPS cap")]
    public Text fpscap_value;
    public Button fpsCap_30;
    public Button fpsCap_60;
    public Button fpsCap_120;

    [Header("vSync")]
    public Text vsync_value;
    public Button[] vsync;

    [Header("TextureQuality")]
    public Text texquality_value;
    public Button[] texQuality;

    [Header("SkinWeights")]
    public Text skinWeights_value;
    public Button[] skinWeights;

    [Header("RenderDistance")]
    public Text rd_value;
    public Button rd_increase;
    public Button rd_decrease;

    private void Awake()
    {
        configPathLabel.text = "config path: \"" + (Application.persistentDataPath + "/config.dat") + "\"";

        if (!instance)
        {
            instance = this;

            SettingsWindow = transform.GetChild(0).gameObject;

            fpsCap_30.onClick.AddListener(() => { FPSCapChange(30); });
            fpsCap_60.onClick.AddListener(() => { FPSCapChange(60); });
            fpsCap_120.onClick.AddListener(() => { FPSCapChange(120); });

            vsync[0].onClick.AddListener(() => { VSyncChange(0); });
            vsync[1].onClick.AddListener(() => { VSyncChange(1); });
            vsync[2].onClick.AddListener(() => { VSyncChange(2); });

            texQuality[0].onClick.AddListener(() => { TextureQualityChange(0); });
            texQuality[1].onClick.AddListener(() => { TextureQualityChange(1); });
            texQuality[2].onClick.AddListener(() => { TextureQualityChange(2); });
            texQuality[3].onClick.AddListener(() => { TextureQualityChange(3); });
            texQuality[4].onClick.AddListener(() => { TextureQualityChange(4); });

            skinWeights[0].onClick.AddListener(() => { SkinWeightsChange(1); });
            skinWeights[1].onClick.AddListener(() => { SkinWeightsChange(2); });
            skinWeights[2].onClick.AddListener(() => { SkinWeightsChange(4); });
            skinWeights[3].onClick.AddListener(() => { SkinWeightsChange(255); });
            skinWeights[4].onClick.AddListener(() => { SkinWeightsChange(0); });

            rd_increase.onClick.AddListener(() => { RenderDistanceChange(100); });
            rd_decrease.onClick.AddListener(() => { RenderDistanceChange(-100); });
        }

        settings = new GlobalSettings();

        LoadConfiguration();

        ApplySettings();

        fpscap_value.text = "Target FPS\n\t<color=grey>test options = " + settings.targetFrameRate + "</color>";
        vsync_value.text = "Vertical synchronization\n\t<color=grey>test options = " + settings.vSyncCount + "</color>";
        texquality_value.text = "Texture quality\n\t<color=grey>test options = " + settings.masterTextureLimit + "</color>";
        skinWeights_value.text = "Skinned mesh quality\n\t<color=grey>test options = " + settings.skinWeights + "</color>";
        rd_value.text = "" + settings.renderDistance;
    }

    public void LoadConfiguration()
    {
        if (File.Exists(Application.persistentDataPath + "/config.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/config.dat");

            settings = (GlobalSettings)bf.Deserialize(file);
            file.Close();
        }

        if (settings.skinWeights < 1) settings.skinWeights = 1;

        Debug.Log("Settings loaded!");
    }

    public void SaveConfiguration()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/config.dat");

        bf.Serialize(file, settings);
        file.Close();

        Debug.Log("Settings saved!");
    }

    public void ToggleSettingsWindow(bool state)
    {
        SettingsWindow.SetActive(state);
    }

    public void ApplySettings()
    {
        Application.targetFrameRate = settings.targetFrameRate;

        QualitySettings.vSyncCount = settings.vSyncCount;
        QualitySettings.masterTextureLimit = settings.masterTextureLimit;

        QualitySettings.skinWeights = (SkinWeights)settings.skinWeights;

        SaveConfiguration();
    }

    void FPSCapChange(int value)
    {
        settings.targetFrameRate = value;

        fpscap_value.text = "Target FPS\n\t<color=grey>test options = " + settings.targetFrameRate + "</color>";

        ApplySettings();
    }

    void VSyncChange(int value)
    {
        settings.vSyncCount = value;

        vsync_value.text = "Vertical synchronization\n\t<color=grey>test options = " + settings.vSyncCount + "</color>";

        ApplySettings();
    }

    void TextureQualityChange(int value)
    {
        settings.masterTextureLimit = value;

        texquality_value.text = "Texture quality\n\t<color=grey>test options = " + settings.masterTextureLimit + "</color>";

        ApplySettings();
    }

    void SkinWeightsChange(int value)
    {
        settings.skinWeights = value;

        skinWeights_value.text = "Skinned mesh quality\n\t<color=grey>test options = " + settings.skinWeights + "</color>";

        ApplySettings();
    }

    void RenderDistanceChange(int value)
    {
        if (value > 0)
        {
            if (settings.renderDistance < 1000)
            {
                settings.renderDistance += value;
                if (settings.renderDistance > 1000) settings.renderDistance = 1000;
            }
        }
        if (value < 0)
        {
            if (settings.renderDistance > 100)
            {
                settings.renderDistance += value;
                if (settings.renderDistance < 100) settings.renderDistance = 200;
            }
        }

        if (CameraControllerBase.current != null) CameraControllerBase.current.ChangeRenderDistance();
        Camera.main.farClipPlane = settings.renderDistance;

        rd_value.text = "" + settings.renderDistance;

        ApplySettings();
    }
}

[System.Serializable]
public class GlobalSettings
{
    public int targetFrameRate = 60;
    public int vSyncCount = 0;
    public int masterTextureLimit = 0;

    public float renderDistance = 1000f;
    public float renderScale = 1.0f;

    public int skinWeights = 4;

    public GlobalSettings()
    {
        renderDistance = 1000f;
        renderScale = 1.0f;
    }
}