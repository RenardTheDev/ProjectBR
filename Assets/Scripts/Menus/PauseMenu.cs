using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu inst;

    public bool isPaused;
    public bool isSettingsActive;
    public bool isPhotomodeActive;

    public GameObject MenuContainer;
    public Image bgFade;

    private void Awake()
    {
        inst = this;
        SettingsManager.instance.closeSettings.onClick.AddListener(() => ToggleSettings(false));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPhotomodeActive)
            {
                TogglePhotomode(false);
            }
            else if (isSettingsActive)
            {
                ToggleSettings(false);
            }
            else
            {
                TogglePause(!isPaused);
            }
        }

        if (isPaused && bgFade.fillAmount < 1 && unpause == null)
        {
            bgFade.fillAmount = Mathf.MoveTowards(bgFade.fillAmount, 1f, Time.unscaledDeltaTime * 3f);
        }
    }

    public void ToggleSettings(bool state)
    {
        if (state)
        {
            MenuContainer.SetActive(false);
            SettingsManager.instance.ToggleSettingsWindow(true);
        }
        else
        {
            MenuContainer.SetActive(true);
            SettingsManager.instance.ToggleSettingsWindow(false);
        }
        isSettingsActive = state;
    }

    public void TogglePhotomode(bool state)
    {
        MenuContainer.transform.parent.gameObject.SetActive(!state);

        if(state) PhotomodeMenu.inst.EnterPhotomode();
        else PhotomodeMenu.inst.ExitPhotomode();

        isPhotomodeActive = state;
    }

    public void TogglePause(bool state)
    {
        if (state)
        {
            isPaused = true;
            Time.timeScale = 0;
            MenuContainer.SetActive(true);

            PlayerUI.current.ToggleUIElement(ui_element.all, false);
        }
        else
        {
            unpause = StartCoroutine(UnpauseWait());
        }
    }

    Coroutine unpause;
    IEnumerator UnpauseWait()
    {
        yield return new WaitForEndOfFrame();

        if (bgFade.fillAmount > 0)
        {
            bgFade.fillAmount = Mathf.MoveTowards(bgFade.fillAmount, 0f, Time.unscaledDeltaTime * 3f);
            unpause = StartCoroutine(UnpauseWait());
        }
        else
        {
            MenuContainer.SetActive(false);
            isPaused = false;
            Time.timeScale = 1;
            unpause = null;

            if (Actor.PLAYERACTOR.isAlive && Actor.PLAYERACTOR != null)
            {
                PlayerUI.current.ToggleUIElement(ui_element.gameplay, true);
                PlayerUI.current.ToggleUIElement(ui_element.controls, true);
                PlayerUI.current.ToggleUIElement(ui_element.stats, true);
                PlayerUI.current.ToggleUIElement(ui_element.actionlog, true);
                PlayerUI.current.ToggleUIElement(ui_element.messages, true);
            }
        }
    }

    public void ExitGame()
    {
        //Time.timeScale = 1.0f;
        //SceneManager.LoadScene(0);
        SceneFade.current.FadeToMenu(1f);
    }
}
