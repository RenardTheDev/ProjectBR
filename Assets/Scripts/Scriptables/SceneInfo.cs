using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
[CreateAssetMenu(fileName = "scene", menuName = "Scriptables/Scene info")]
public class SceneInfo : ScriptableObject
{
    public string sceneName = "Scene";
    public int sceneBuildID = 0;

    public Sprite cover_image;
}
