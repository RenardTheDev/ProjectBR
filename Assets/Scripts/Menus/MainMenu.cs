using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject menuContainer;

    public void PlayGame(SceneInfo scene)
    {
        SceneFade.current.FadeToGame(scene, 1f);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
