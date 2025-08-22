using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Cozyheim.LevelingSystem
{
    public class XPText : MonoBehaviour
    {
        public int xpGained;

        private readonly float fadeInTime = 0.5f;
        private readonly float fadeOutTime = 1f;
        private readonly float moveDistance = 0.5f;
        private readonly float visibleTime = 1.5f;

        private IEnumerator Start()
        {
            transform.localScale *= Main.ModConfig.XpFontSize.Value / 100f;

            var canvasGroup = GetComponentInChildren<CanvasGroup>();
            var startSize = transform.localScale.x;

            var startPosition = transform.position;
            var deltaPosition = Vector3.up * moveDistance;
            var fullAnimTime = fadeInTime + visibleTime + fadeOutTime;

            for (var f = 0f; f < fullAnimTime; f += Time.deltaTime) {
                // Movement
                var perc = f / fullAnimTime;
                transform.position = startPosition + deltaPosition * perc;
                canvasGroup.alpha = 1f;

                transform.localScale = Vector3.one * startSize;
                // FadeIn
                if (f < fadeInTime) {
                    canvasGroup.alpha = f / fadeInTime;
                    transform.localScale = f / fadeInTime * Vector3.one * startSize;
                }

                // FadeOut
                var beforeFadeOutTime = fadeInTime + visibleTime;
                if (f > beforeFadeOutTime) {
                    var fadeDeltaTime = fullAnimTime - beforeFadeOutTime;
                    canvasGroup.alpha = 1 - (f - beforeFadeOutTime) / fadeDeltaTime;
                    transform.localScale = (1 - (f - beforeFadeOutTime) / fadeDeltaTime) * Vector3.one * startSize;
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        private void Update()
        {
            transform.eulerAngles = Camera.main.transform.eulerAngles;
        }

        public void XPGained(int xp)
        {
            GetComponentInChildren<Text>().text = "+" + xp + " xp";
        }
    }
}