using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFade : MonoBehaviour
{
    public static SceneFade current;

    public Image loadProgress;
    public Image fader;

    public Image MapCover;
    public Text MapName;
    public Text TipBox;
    public Text hintOnReady;
    public Animation loadingAnim;

    public float transitionTime = 8f;

    public string[] tips;
    private void Awake()
    {
        if (current == null)
        {
            current = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeToGame(SceneInfo scene, float transitionTime = 1.0f)
    {
        this.transitionTime = transitionTime;

        MapCover.sprite = scene.cover_image;
        MapName.text = scene.sceneName;
        TipBox.text = tips[Random.Range(0, tips.Length)];

        MapCover.gameObject.SetActive(true);
        MapName.gameObject.SetActive(true);
        TipBox.gameObject.SetActive(true);
        hintOnReady.enabled = false;

        if (IN_Coroutine == null) IN_Coroutine = StartCoroutine(FadeInSync(scene.sceneBuildID));

        waitForInput = true;
    }

    public void FadeToMenu(float transitionTime = 1.0f)
    {
        this.transitionTime = transitionTime;

        MapCover.gameObject.SetActive(false);
        MapName.gameObject.SetActive(false);
        TipBox.gameObject.SetActive(false);
        hintOnReady.enabled = false;

        if (IN_Coroutine == null) IN_Coroutine = StartCoroutine(FadeInSync(0));

        waitForInput = false;
    }

    private void Update()
    {

    }

    Coroutine IN_Coroutine;
    Coroutine SLoad_Coroutine;
    AsyncOperation gameLoad;
    bool waitForInput;

    IEnumerator FadeInSync(int scene)
    {
        loadProgress.fillAmount = 0f;
        fader.fillOrigin = 0;
        loadingAnim.Play("wait_for_loading");

        while (fader.fillAmount < 1f)
        {
            fader.fillAmount = Mathf.MoveTowards(fader.fillAmount, 1f, Time.unscaledDeltaTime / transitionTime);
            yield return new WaitForEndOfFrame();
        }

        loadingAnim.Play("loading");
        gameLoad = SceneManager.LoadSceneAsync(scene);
        gameLoad.allowSceneActivation = false;

        IN_Coroutine = null;
        Time.timeScale = 0.0f;
        SLoad_Coroutine = StartCoroutine(SceneLoad());
    }

    IEnumerator SceneLoad()
    {
        while (!gameLoad.isDone)
        {
            loadProgress.fillAmount = gameLoad.progress;
            if (gameLoad.progress >= 0.9f)
            {
                loadProgress.fillAmount = 1f;
                gameLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        StartCoroutine(FadeOut(transitionTime));
    }

    IEnumerator FadeOut(float transitionTime)
    {
        fader.fillOrigin = 1;

        Time.timeScale = 1.0f;

        loadingAnim.Play("wait_for_loading");

        if (waitForInput)
        {
            hintOnReady.enabled = true; 
            while (!Input.anyKeyDown && waitForInput)
            {
                yield return null;
            }
        }

        SLoad_Coroutine = null;

        while (fader.fillAmount > 0f)
        {
            fader.fillAmount = Mathf.MoveTowards(fader.fillAmount, 0f, Time.unscaledDeltaTime / transitionTime);
            yield return new WaitForEndOfFrame();
        }
    }
}