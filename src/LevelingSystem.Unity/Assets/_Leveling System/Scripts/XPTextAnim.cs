using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class XPTextAnim : MonoBehaviour
{
    public float fadeInTime = 0.5f;
    public float visibleTime = 1.5f;
    public float fadeOutTime = 1f;

    public float moveDistance = 0.5f;
    public int xpGained;

    void Update() {
        transform.eulerAngles = Camera.main.transform.eulerAngles;
    }

    IEnumerator Start()
    {
        SetXPText(xpGained);

        CanvasGroup canvasGroup = GetComponentInChildren<CanvasGroup>();
        float startSize = transform.localScale.x;

        Vector3 startPosition = transform.position;
        Vector3 deltaPosition = Vector3.up * moveDistance;
        float fullAnimTime = fadeInTime + visibleTime + fadeOutTime;

        for(float f = 0f; f < fullAnimTime; f += Time.deltaTime) {
            // Movement
            float perc = f / fullAnimTime;
            transform.position = startPosition + deltaPosition * perc;

            canvasGroup.alpha = 1f;

            transform.localScale = Vector3.one * startSize;
            // FadeIn
            if(f < fadeInTime) {
                canvasGroup.alpha = (f / fadeInTime);
                transform.localScale = (f / fadeInTime) * Vector3.one * startSize;
            }

            // FadeOut
            float beforeFadeOutTime = fadeInTime + visibleTime;
            if(f > beforeFadeOutTime) {
                float fadeDeltaTime = fullAnimTime - beforeFadeOutTime;
                canvasGroup.alpha = (1 - (f - beforeFadeOutTime) / fadeDeltaTime);
                transform.localScale = (1 - (f - beforeFadeOutTime) / fadeDeltaTime) * Vector3.one * startSize;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    public void SetXPText(int xp) {
        GetComponentInChildren<Text>().text = "+" + xp.ToString() + " xp";
    }

}
