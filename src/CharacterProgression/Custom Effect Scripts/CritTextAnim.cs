using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace DeepWolf.CharacterProgressionMod
{
    public class CritTextAnim : MonoBehaviour
    {
        private readonly float fullAnimTime = 3f;
        private readonly float spread = 0.5f;
        private AnimationCurve animCurve;
        private float damageSizeScale = 1f;
        private float maxCritSize = 1f;

        private IEnumerator Start()
        {
            var spawnSpread = Vector3.zero;
            spawnSpread += Random.Range(-spread, spread) * Camera.main.transform.right; // Randomize x
            spawnSpread += Random.Range(0f, spread) * Camera.main.transform.up; // Randomize y

            transform.position += spawnSpread;


            var keyFrames = new[] {
                new Keyframe(0f, 0f, 0f, 1f),
                new Keyframe(0.08f, 1.2f, 0f, 2f),
                new Keyframe(0.15f, 1f, -2f, 0f),
                new Keyframe(0.75f, 1f, 0f, -0.5f),
                new Keyframe(1f, 0f, 0f, 0f)
            };
            animCurve = new AnimationCurve(keyFrames);

            var startSize = transform.localScale;

            for (var f = 0f; f < fullAnimTime; f += Time.deltaTime) {
                // Movement
                var perc = f / fullAnimTime;
                transform.localScale = startSize * damageSizeScale * animCurve.Evaluate(perc);

                yield return null;
            }

            Destroy(gameObject);
        }

        private void Update()
        {
            transform.eulerAngles = Camera.main.transform.eulerAngles;
        }

        private void SetColorAndScale(float damage)
        {
            var textComp = GetComponentInChildren<Text>();
            var color = new Color(0.8f, 0.6f, 0.15f, 1f);

            // Fixed intervals of scale and color
            if (damage < 30f) {
                damageSizeScale = 1f;
                color.g = 0.6f;
            }
            else if (damage < 100f) {
                damageSizeScale = 1.2f;
                color.g = 0.45f;
            }
            else if (damage < 200f) {
                damageSizeScale = 1.4f;
                color.g = 0.3f;
            }
            else if (damage < 300f) {
                damageSizeScale = 1.6f;
                color.g = 0.15f;
            }
            else {
                damageSizeScale = 1.8f;
                color.g = 0f;
            }

            textComp.color = color;

            // Gradient scale and color
            // ------------------------
            /*
            float value = Mathf.InverseLerp(30f, 300f, damage);
            color.g = (1 - value) * 0.6f;

            textComp.color = color;
            damageSizeScale = 1 + (value * maxCritSize);
            */
        }

        public void SetText(string value)
        {
            GetComponentInChildren<Text>().text = value.ToString(CultureInfo.GetCultureInfo("en-US"));
        }

        public void SetText(float value, int decimals = 0)
        {
            decimals = Mathf.Max(0, decimals);
            var decimalsFormat = "N" + decimals;
            SetText(value.ToString(decimalsFormat));
            SetColorAndScale(value);
        }

        public void SetText(int value)
        {
            SetText(value.ToString());
        }
    }
}