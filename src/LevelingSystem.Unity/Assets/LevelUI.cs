using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    RectTransform categoryContainer;

    public List<Button> skillsCategoryButtons;
    public List<RectTransform> skillsCategories;

    public GameObject skillUI;

    private SkillCategory[] skills = new SkillCategory[]
    {
        SkillCategory.Offensive,
        SkillCategory.Offensive,
        SkillCategory.Offensive,
        SkillCategory.Defensive,
        SkillCategory.Defensive,
        SkillCategory.Core,
        SkillCategory.Core,
        SkillCategory.Core,
        SkillCategory.Utility,
        SkillCategory.Utility,
        SkillCategory.Utility,
        SkillCategory.Utility,
        SkillCategory.Utility,
        SkillCategory.Utility,
        SkillCategory.Utility,
        SkillCategory.Utility,
        SkillCategory.Utility
    };

    // Start is called before the first frame update
    void Awake()
    {
        RectTransform buttonContainer = transform.Find("Skills UI/Scroll View/Category Buttons").GetComponent<RectTransform>();
        categoryContainer = transform.Find("Skills UI/Scroll View/Viewport/Content").GetComponent<RectTransform>();

        for (int i = 0; i < buttonContainer.childCount; i++)
        {
            Button button = buttonContainer.GetChild(i).GetComponent<Button>();
            int index = i;
            button.onClick.AddListener(delegate ()
            {
                OpenCategory(index);
            });

            skillsCategoryButtons.Add(button);
        }

        for (int i = 0; i < categoryContainer.childCount; i++)
        {
            skillsCategories.Add(categoryContainer.GetChild(i).GetComponent<RectTransform>());
        }

        OpenCategory(0);
    }



    private Transform[] GenerateSkills(int index)
    {
        DeleteAllChildren(skillsCategories[index]);

        List<Transform> list = new List<Transform>();

        foreach(SkillCategory sk in skills)
        {
            if((int)sk == index)
            {
                list.Add(Instantiate(skillUI, skillsCategories[index]).GetComponent<Transform>());
            }
        }

        return list.ToArray();
    }

    private void DeleteAllChildren(Transform trans)
    {
        foreach(Transform child in trans)
        {
            Destroy(child.gameObject);
        }
    }

    public void OpenCategory(int index)
    {
        // Enable/Disable the categories
        for (int i = 0; i < skillsCategories.Count; i++)
        {
            if (i == index)
            {
                skillsCategories[i].gameObject.SetActive(true);
            }
            else
            {
                skillsCategories[i].gameObject.SetActive(false);
            }
        }

        // Resize the content transform
        Transform[] skillsGenerated = GenerateSkills(index);
        float height = Mathf.Ceil(skillsGenerated.Length / 3f) * 215f;

        categoryContainer.sizeDelta = new Vector2(categoryContainer.sizeDelta.x, height);
        categoryContainer.anchoredPosition = Vector2.zero;

        // Set the color of the skills to match the button
        Color color = skillsCategoryButtons[index].GetComponent<Image>().color;
        foreach(Transform t in skillsGenerated) { 
            t.GetComponent<Image>().color = color;
        }
    }

}


public enum SkillCategory
{
    Offensive,
    Defensive,
    Core,
    Utility
}