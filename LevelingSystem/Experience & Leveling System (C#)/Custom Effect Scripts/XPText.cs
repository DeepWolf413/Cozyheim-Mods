using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Cozyheim.LevelingSystem
{
    public class XPText : MonoBehaviour
    {
        public int xpGained;

        private float fadeInTime = 0.5f;
        private float visibleTime = 1.5f;
        private float fadeOutTime = 1f;
        private float moveDistance = 0.5f;

        public void XPGained(int xp)
        {
            GetComponentInChildren<Text>().text = "+" + xp.ToString() + " xp";
        }

        void Update()
        {
            transform.eulerAngles = Camera.main.transform.eulerAngles;
        }

        IEnumerator Start()
        {
            transform.localScale *= Main.ModConfig.XpFontSize.Value / 100f;

            CanvasGroup canvasGroup = GetComponentInChildren<CanvasGroup>();
            float startSize = transform.localScale.x;

            Vector3 startPosition = transform.position;
            Vector3 deltaPosition = Vector3.up * moveDistance;
            float fullAnimTime = fadeInTime + visibleTime + fadeOutTime;

            for (float f = 0f; f < fullAnimTime; f += Time.deltaTime)
            {
                // Movement
                float perc = f / fullAnimTime;
                transform.position = startPosition + deltaPosition * perc;
                canvasGroup.alpha = 1f;

                transform.localScale = Vector3.one * startSize;
                // FadeIn
                if (f < fadeInTime)
                {
                    canvasGroup.alpha = (f / fadeInTime);
                    transform.localScale = (f / fadeInTime) * Vector3.one * startSize;
                }

                // FadeOut
                float beforeFadeOutTime = fadeInTime + visibleTime;
                if (f > beforeFadeOutTime)
                {
                    float fadeDeltaTime = fullAnimTime - beforeFadeOutTime;
                    canvasGroup.alpha = (1 - (f - beforeFadeOutTime) / fadeDeltaTime);
                    transform.localScale = (1 - (f - beforeFadeOutTime) / fadeDeltaTime) * Vector3.one * startSize;
                }

                yield return null;
            }

            Destroy(gameObject);
        }

    }

}
