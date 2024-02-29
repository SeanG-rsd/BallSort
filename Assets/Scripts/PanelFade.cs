using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelFade : MonoBehaviour
{
    private float fadeTime = 0.4f;
    [SerializeField] private Image fadeImage;
    private bool fadeIn = true;

    private void Awake()
    {
        StartCoroutine(DoFade(0, 1));

    }

    private IEnumerator DoFade(int start, int end)
    {
        float fade = 0;

        while (fade < fadeTime)
        {
            fade += Time.deltaTime;
            Color c = fadeImage.color;
            c.a = Mathf.Lerp(start, end, fade /  fadeTime);
            fadeImage.color = c;

            yield return null;
        }
    }
}
