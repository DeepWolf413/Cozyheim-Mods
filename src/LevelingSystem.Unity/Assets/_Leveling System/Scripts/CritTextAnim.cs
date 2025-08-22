using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CritTextAnim : MonoBehaviour
{
    private AnimationCurve animCurve;
    private float fullAnimTime = 2f;

    void Update() {
        transform.eulerAngles = Camera.main.transform.eulerAngles;
    }

    IEnumerator Start()
    {
        Keyframe[] keyFrames = new Keyframe[]
        {
            new Keyframe(0f, 0f, 0f, 1f),
            new Keyframe(0.1f, 1.2f, 0f, 2f),
            new Keyframe(0.2f, 1f, -2f, 0f),
            new Keyframe(0.75f, 1f, 0f, -0.5f),
            new Keyframe(1f, 0f, 0f, 0f)
        };
        animCurve = new AnimationCurve(keyFrames);

        SetText(42.1f, 1);

        CanvasGroup canvasGroup = GetComponentInChildren<CanvasGroup>();
        Vector3 startSize = transform.localScale;

        for(float f = 0f; f < fullAnimTime; f += Time.deltaTime) {
            // Movement
            float perc = f / fullAnimTime;
            transform.localScale = startSize * animCurve.Evaluate(perc);

            yield return null;
        }

        Destroy(gameObject);
    }

    public void SetText(string value) {
        GetComponentInChildren<Text>().text = value;
    }

    public void SetText(float value, int decimals = 0)
    {
        decimals = Mathf.Max(0, decimals);
        string decimalsFormat = "N" + decimals.ToString();
        SetText(value.ToString(decimalsFormat));
    }

    public void SetText(int value)
    {
        SetText(value);
    }
}
