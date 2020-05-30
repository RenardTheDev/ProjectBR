using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fpsCounter : MonoBehaviour
{
    public Text text;
    int frames;

    private void Awake()
    {
        StartCoroutine(Refresh());
    }

    private void Update()
    {
        frames++;
    }

    IEnumerator Refresh()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);

            text.text = frames + "fps";
            frames = 0;
        }
    }
}
