using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    AsyncOperation gameLoad;
    public Image loadProgress;
    public Text hint_levelName;
    public Text hint_press;

    public GameObject menuContainer;

    private void Awake()
    {

    }

    private void Start()
    {
        SettingsManager.instance.closeSettings.onClick.AddListener(HideSettings);
    }

    public void PlayGame(SceneInfo scene)
    {
        SceneFade.current.FadeToGame(scene, 1f);
    }

    public void ShowSettings()
    {
        menuContainer.SetActive(false);
        SettingsManager.instance.ToggleSettingsWindow(true);
    }

    public void HideSettings()
    {
        menuContainer.SetActive(true);
        SettingsManager.instance.ToggleSettingsWindow(false);
    }

    private void GameLoad_completed(AsyncOperation obj)
    {

    }

    private void Update()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
