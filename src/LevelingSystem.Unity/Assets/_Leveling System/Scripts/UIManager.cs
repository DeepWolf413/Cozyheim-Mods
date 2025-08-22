using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public CanvasGroup skillsUI;
    public Text remainingPoints;
    public Button closeButton;
    public Button resetPointsButton;
    public RectTransform viewportContent;
    public Sprite unusedPointSprite, usedPointSprite;

    public GameObject skillPrefab;
    public GameObject skillPointPrefab;

    public List<SkillOption> skillOptions = new List<SkillOption>();

    private bool skillsUIVisible = false;
    private bool initializedSkillList = false;

    void Start()
    {
        skillsUI = transform.Find("Skills UI").GetComponent<CanvasGroup>();
        remainingPoints = transform.Find("Skills UI/Remaining Points").GetComponent<Text>();
        closeButton = transform.Find("Skills UI/Close Menu").GetComponent<Button>();
        resetPointsButton = transform.Find("Skills UI/Reset Skills Button").GetComponent<Button>();
        viewportContent = transform.Find("Skills UI/Viewport/Content").GetComponent<RectTransform>();
        unusedPointSprite = transform.Find("Skills UI/UnusedPoint").GetComponent<Image>().sprite;
        usedPointSprite = transform.Find("Skills UI/UsedPoint").GetComponent<Image>().sprite;
        
        ToggleSkillsUI(false);

        int skillsCount = 14;

        // Add skills to list
        if(!initializedSkillList) {
            for(int i = 0; i < skillsCount; i++) {
                RectTransform rectTrans = Instantiate(skillPrefab, viewportContent).GetComponent<RectTransform>();
                rectTrans.anchoredPosition3D = Vector3.down * 100f * i;

                SkillOption option = rectTrans.gameObject.AddComponent<SkillOption>();
                option.Setup(skillPointPrefab, 10 + i, unusedPointSprite, usedPointSprite);
                option.UpdateSkillPointsColor(5);
                skillOptions.Add(option);
            }

            initializedSkillList = true;
        }

        Vector2 tempSize = viewportContent.sizeDelta;
        tempSize.y = 100f * skillsCount + 20f;
        viewportContent.sizeDelta = tempSize;

        // Add listener for the buttons
        closeButton.onClick.AddListener(delegate() {
            ToggleSkillsUI(false);
        });
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I)) {
            ToggleSkillsUI(!skillsUIVisible);
        }
    }

    void ToggleSkillsUI(bool value) {
        skillsUI.alpha = value ? 1f : 0f;
        skillsUI.interactable = value;
        skillsUI.blocksRaycasts = value;

        skillsUIVisible = value;
        viewportContent.anchoredPosition3D = Vector3.zero;
    }

    void SetupSkillButton() {
        skillsUI = transform.Find("Skills UI").GetComponent<CanvasGroup>();
    }
}


public class SkillOption : MonoBehaviour {
    private GameObject skillPointGO;
    private int pointsAmount;
    private Sprite unusedPointSprite, usedPointSprite;

    private Text nameText, nameTextShadow;
    private Text bonusText, bonusTextShadow;
    private Text description;
    private Button addPointButton;
    private CanvasGroup addPointButtonGroup;
    private RectTransform skillPointContainer;
    private List<Image> skillPoints = new List<Image>();

    public void Setup(GameObject skillPointGO, int pointsAmount, Sprite unused, Sprite used) {
        nameText = transform.Find("Skill Name").GetComponent<Text>();
        nameTextShadow = transform.Find("Skill Name/Skill Name Shadow").GetComponent<Text>();
        bonusText = transform.Find("Bonus Text").GetComponent<Text>();
        bonusTextShadow = transform.Find("Bonus Text/Bonus Text Shadow").GetComponent<Text>();
        description = transform.Find("Description").GetComponent<Text>();
        addPointButton = transform.Find("Add Skill Point").GetComponent<Button>();
        addPointButtonGroup = transform.Find("Add Skill Point").GetComponent<CanvasGroup>();
        skillPointContainer = transform.Find("Skill Points").GetComponent<RectTransform>();

        unusedPointSprite = unused;
        usedPointSprite = used;

        this.skillPointGO = skillPointGO;
        this.pointsAmount = pointsAmount;

        SetupSkillPoints();
    }

    private void SetupSkillPoints() {
        float fraction = 1f / (float) pointsAmount;

        for(int i = 0; i < pointsAmount; i++) {
            RectTransform rectTrans = Instantiate(skillPointGO, skillPointContainer).GetComponent<RectTransform>();
            rectTrans.anchorMin = new Vector2(fraction * i, 0f);
            rectTrans.anchorMax = new Vector2(fraction * (i + 1), 1f);

            skillPoints.Add(rectTrans.GetComponent<Image>());
        }
    }

    public void UpdateSkillPointsColor(int amount) {
        for(int i = 0; i < skillPoints.Count; i++) {
            skillPoints[i].sprite = i < amount ? usedPointSprite : unusedPointSprite;
        }
    }
}
