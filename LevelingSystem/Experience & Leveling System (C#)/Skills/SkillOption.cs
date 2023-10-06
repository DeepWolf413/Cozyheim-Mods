using UnityEngine;
using UnityEngine.UI;

namespace Cozyheim.LevelingSystem;

internal class SkillOption : MonoBehaviour
{
	public Button addPointButton;

	public Button removePointButton;

	public Button resetPointButton;
	private CanvasGroup addPointButtonGroup;
	private Text bonusText, bonusTextShadow;
	private Text description;

	private Image iconImage;
	private Image levelFillBar;
	private Text levelText, levelTextShadow;
	private Text maxLevelText;
	private Text nameText, nameTextShadow;
	private CanvasGroup removePointButtonGroup;
	private CanvasGroup resetPointButtonGroup;
	private GameObject resetTextGO;

	public void Setup()
	{
		nameText = transform.Find("Skill Name").GetComponent<Text>();
		nameTextShadow = transform.Find("Skill Name/Skill Name Shadow").GetComponent<Text>();

		bonusText = transform.Find("Bonus Text").GetComponent<Text>();
		bonusTextShadow = transform.Find("Bonus Text/Bonus Text Shadow").GetComponent<Text>();

		levelText = transform.Find("Skill Level").GetComponent<Text>();
		levelTextShadow = transform.Find("Skill Level/Skill Level Shadow").GetComponent<Text>();

		maxLevelText = transform.Find("Skill Level Ring/Skill Level Max").GetComponent<Text>();

		description = transform.Find("Description").GetComponent<Text>();

		iconImage = transform.Find("Skill Icon").GetComponent<Image>();
		levelFillBar = transform.Find("Skill Level Ring/Skill Level Fill Slider").GetComponent<Image>();

		addPointButton = transform.Find("Add Skill Point (Mask)/Add Skill Point").GetComponent<Button>();
		addPointButtonGroup = transform.Find("Add Skill Point (Mask)/Add Skill Point").GetComponent<CanvasGroup>();

		removePointButton = transform.Find("Remove Skill Point (Mask)/Remove Skill Point").GetComponent<Button>();
		removePointButtonGroup = transform.Find("Remove Skill Point (Mask)/Remove Skill Point").GetComponent<CanvasGroup>();

		resetPointButton = transform.Find("Reset Skill Point (Mask)/Reset Skill Point").GetComponent<Button>();
		resetPointButtonGroup = transform.Find("Reset Skill Point (Mask)/Reset Skill Point").GetComponent<CanvasGroup>();
		resetTextGO = transform.Find("Reset Skill Point (Mask)/Reset Text").gameObject;
	}

	private void ToggleButtonVisibility(CanvasGroup cnvGroup, bool value)
	{
		cnvGroup.alpha = value ? 1f : 0f;
		cnvGroup.interactable = value;
		//cnvGroup.blocksRaycasts = value;
	}

	public void UpdateAllButtonVisibility(SkillBase skillInfo)
	{
		ToggleButtonVisibility(addPointButtonGroup, !skillInfo.IsLevelMax());
		ToggleButtonVisibility(removePointButtonGroup, !skillInfo.IsLevelZero());
		ToggleButtonVisibility(resetPointButtonGroup, !skillInfo.IsLevelZero());
		resetTextGO.SetActive(!skillInfo.IsLevelZero());
	}

	public void UpdateInformation(SkillBase skillInfo)
	{
//            ConsoleLog.Print("UpdateInformation called", LogType.Message);

		UpdateAllButtonVisibility(skillInfo);

		nameText.text = skillInfo.GetName();
		nameTextShadow.text = skillInfo.GetName();

		bonusText.text = "+" + skillInfo.GetBonus() + skillInfo.bonusUnit;
		bonusTextShadow.text = "+" + skillInfo.GetBonus() + skillInfo.bonusUnit;

		levelText.text = skillInfo.GetLevel().ToString();
		levelTextShadow.text = skillInfo.GetLevel().ToString();

		maxLevelText.text = skillInfo.GetMaxLevel().ToString();

		iconImage.sprite = Main.assetBundle.LoadAsset<Sprite>(Main.assetsPath + "Sprites/Skill Icons/" + skillInfo.iconName + ".png");

		description.text = "+" + skillInfo.bonusPerLevel + skillInfo.bonusUnit + " / Level";

		var baseValue = 0.1245f;
		var fillAmount = skillInfo.GetLevel() / (float)skillInfo.GetMaxLevel() * 0.75f;
		levelFillBar.fillAmount = baseValue + fillAmount;
	}
}