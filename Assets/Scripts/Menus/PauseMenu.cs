using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu inst;

    public bool isPaused;
    public bool isInventoryActive;
    public bool isSettingsActive;
    public bool isPhotomodeActive;

    public GameObject MenuContainer;
    public Image bgFade;

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (isPaused && bgFade.fillAmount < 1 && unpause == null)
        {
            bgFade.fillAmount = Mathf.MoveTowards(bgFade.fillAmount, 1f, Time.unscaledDeltaTime * 3f);
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
                PlayerUI.current.ToggleUIElement(player_ui_element.all, true);
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
