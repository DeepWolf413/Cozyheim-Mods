using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentResize : MonoBehaviour
{
    private RectTransform rectTransform;

    public static Color skillColor;

    public List<RectTransform> skillCategories = new List<RectTransform>();


    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // Get all rect transform children
        for (int i = 0; i < transform.childCount; i++)
        {
            skillCategories.Add(transform.GetChild(i).GetComponent<RectTransform>());
        }
    }

    public void OpenCategory(int index)
    {
        for(int i = 0; i < skillCategories.Count; i++)
        {
            if (i == index)
            {
                skillCategories[i].gameObject.SetActive(true);
            }
            else
            {
                skillCategories[i].gameObject.SetActive(false);
            }
        }

        int skillCount = skillCategories[index].childCount;
        float height = Mathf.Ceil(skillCount / 3f) * 215f;

        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
        rectTransform.anchoredPosition = Vector2.zero;

        for (int i = 0; i < skillCount; i++)
        {
            skillCategories[index].GetChild(i).GetComponent<Image>().color = Color.red;
        }
    }
}
