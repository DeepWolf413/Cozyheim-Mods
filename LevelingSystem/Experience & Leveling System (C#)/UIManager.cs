﻿using System.Collections;
using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Cozyheim.LevelingSystem
{
	internal class UIManager : MonoBehaviour
	{
		private static readonly int s_statusEffectRested = "Rested".GetStableHashCode();

		// Floating text
		private static GameObject xpTextFloating;
		private static GameObject levelUpEffect;
		private static Vector3 lastXPTextSpawnPosition = Vector3.zero;

		public static CustomRPC rpc_AddExperienceMonster;
		public static CustomRPC rpc_AddExperience;
		public static CustomRPC rpc_ReloadConfig;
		public static CustomRPC rpc_LevelUpEffect;

		public static UIManager Instance;

		public int playerXP;
		public int playerLevel = 1;

		// Skills UI
		public CanvasGroup skillsUI;
		public Text remainingPoints;
		public Button closeButton;
		public Button resetPointsButton;
		public RectTransform viewportContent;
		public ScrollRect skillsScrollRect;
		public Scrollbar skillsScrollbar;
		public RectTransform buttonContainer;

		public GameObject skillPrefab;

		public bool skillsUIVisible;
		private readonly List<RectTransform> skillsCategories = new();

		private readonly List<Button> skillsCategoryButtons = new();
		private readonly float smoothMaxSpeed = 10f;
		private readonly float smoothTime = 0.15f;

		private bool gainedNewLevel;

		// XP Bar
		private Text levelText, levelTextShadow;
		private Text levelUpText, levelUpTextShadow;
		private CanvasGroup xpBarGroup, levelUpGroup;
		private RectTransform xpBarRect, levelTextRect, xpBarContainerRect;

		private bool xpBarVisible;
		private Image xpFill;
		private float xpFillTarget;
		private float xpFillVel;
		private Text xpText, xpTextShadow;

		private void Awake()
		{
			Instance = this;

			levelText = transform.Find("XP Bar/LevelText").GetComponent<Text>();
			levelTextShadow = transform.Find("XP Bar/LevelText/Shadow").GetComponent<Text>();
			xpText = transform.Find("XP Bar/XP Bar/XPText").GetComponent<Text>();
			xpTextShadow = transform.Find("XP Bar/XP Bar/XPText/Shadow").GetComponent<Text>();
			xpFill = transform.Find("XP Bar/XP Bar/XPFill").GetComponent<Image>();

			xpBarGroup = transform.Find("XP Bar").GetComponent<CanvasGroup>();
			xpBarContainerRect = transform.Find("XP Bar").GetComponent<RectTransform>();
			xpBarRect = transform.Find("XP Bar/XP Bar").GetComponent<RectTransform>();
			levelTextRect = transform.Find("XP Bar/LevelText").GetComponent<RectTransform>();

			levelUpText = transform.Find("LevelUp Pop-Up/LevelUpText").GetComponent<Text>();
			levelUpTextShadow = transform.Find("LevelUp Pop-Up/LevelUpText/Shadow").GetComponent<Text>();
			levelUpGroup = transform.Find("LevelUp Pop-Up").GetComponent<CanvasGroup>();

			// Skills UI
			skillsUI = transform.Find("Skills UI").GetComponent<CanvasGroup>();
			remainingPoints = transform.Find("Skills UI/Remaining Points").GetComponent<Text>();
			closeButton = transform.Find("Skills UI/Close Menu").GetComponent<Button>();
			resetPointsButton = transform.Find("Skills UI/Reset Skills Button").GetComponent<Button>();
			viewportContent = transform.Find("Skills UI/Scroll View/Viewport/Content").GetComponent<RectTransform>();
			skillsScrollRect = transform.Find("Skills UI").GetComponent<ScrollRect>();
			skillsScrollbar = transform.Find("Skills UI/Scrollbar").GetComponent<Scrollbar>();
			buttonContainer = transform.Find("Skills UI/Scroll View/Category Buttons").GetComponent<RectTransform>();

			skillPrefab = PrefabManager.Instance.GetPrefab("SkillUI");

			SetupAllUISkillButtons();
		}

		private void Start()
		{
			ToggleSkillsUI(false);

			playerLevel = XPManager.Instance.GetPlayerLevel();
			playerXP = XPManager.Instance.GetPlayerXP();

			XPTable.UpdateMonsterXPTable();
			XPTable.UpdatePlayerXPTable();
			XPTable.UpdatePickableXPTable();
			XPTable.UpdateMiningXPTable();
			XPTable.UpdateWoodcuttingXPTable();

			levelUpGroup.alpha = 0f;

			xpTextFloating = PrefabManager.Instance.GetPrefab("XPText");
			levelUpEffect = PrefabManager.Instance.GetPrefab("LevelUpEffectNew");

			// Set the size and position of the xp bar / level text
			RepositionXPBar();

			UpdateUI(true);
			StartCoroutine(XPBarFadeIn(3f));
		}

		private void Update()
		{
			if (!IsPlayerMaxLevel()) {
				var tempFillTarget = gainedNewLevel ? 1f : xpFillTarget;
				xpFill.fillAmount = Mathf.SmoothDamp(xpFill.fillAmount, tempFillTarget, ref xpFillVel, smoothTime, smoothMaxSpeed);

				if (gainedNewLevel && xpFill.fillAmount > 0.99f) {
					gainedNewLevel = false;
					xpFill.fillAmount = 0f;
				}
			}

			if (skillsUIVisible)
				if (Input.GetKeyDown(KeyCode.Escape))
					ToggleSkillsUI(false);
		}

		public static void Init()
		{
			rpc_AddExperienceMonster = NetworkManager.Instance.AddRPC("AddExperienceMonster", RPC_AddExperienceMonster, RPC_AddExperienceMonster);
			rpc_AddExperience = NetworkManager.Instance.AddRPC("AddExperience", RPC_AddExperience, RPC_AddExperience);
			rpc_ReloadConfig = NetworkManager.Instance.AddRPC("ReloadConfig", RPC_ReloadConfig, RPC_ReloadConfig);
			rpc_LevelUpEffect = NetworkManager.Instance.AddRPC("LevelUpEffect", RPC_LevelUpEffect, RPC_LevelUpEffect);
		}

		private void SetupAllUISkillButtons()
		{
			for (var i = 0; i < buttonContainer.childCount; i++) {
				var button = buttonContainer.GetChild(i).GetComponent<Button>();
				button.onClick.RemoveAllListeners();

				var index = i;
				button.onClick.AddListener(delegate { OpenCategory(index); });

				skillsCategoryButtons.Add(button);
			}

			for (var i = 0; i < viewportContent.childCount; i++) skillsCategories.Add(viewportContent.GetChild(i).GetComponent<RectTransform>());
		}

		public void OpenCategory(int index)
		{
			// Enable/Disable the categories
			for (var i = 0; i < skillsCategories.Count; i++)
				if (i == index)
					skillsCategories[i].gameObject.SetActive(true);
				else
					skillsCategories[i].gameObject.SetActive(false);

			// Resize the content transform
			var skillsGenerated = GenerateSkills(index);
			var height = Mathf.Ceil(skillsGenerated.Length / 3f) * 215f;

			viewportContent.sizeDelta = new Vector2(viewportContent.sizeDelta.x, height);
			viewportContent.anchoredPosition = Vector2.zero;

			// Set the color of the skills to match the button
			var color = skillsCategoryButtons[index].GetComponent<Image>().color;
			foreach (var t in skillsGenerated) t.GetComponent<Image>().color = color;

			UpdateUIInformation();
		}

		private Transform[] GenerateSkills(int index)
		{
			if (skillsCategories.Count > 0) DeleteAllChildren(skillsCategories[index]);

			var list = new List<Transform>();

			foreach (var skillSetting in SkillConfig.skillSettings) {
				if (!skillSetting.GetEnabled()) continue;

				if (index == (int)skillSetting.category) {
					var skill = SkillManager.Instance.GetSkillByType(skillSetting.skillType);
					var newSkill = Instantiate(skillPrefab, skillsCategories[index]);
					list.Add(newSkill.GetComponent<Transform>());

					var option = newSkill.gameObject.AddComponent<SkillOption>();
					option.Setup();
					skill.SetSkillUI(option);
				}
			}

			return list.ToArray();
		}

		private void DeleteAllChildren(Transform trans)
		{
			foreach (Transform child in trans) Destroy(child.gameObject);
		}

		private void RepositionXPBar()
		{
			var tempSize = xpBarRect.sizeDelta;
			tempSize.x *= Main.xpBarSize.Value / 100f;
			xpBarRect.sizeDelta = tempSize;

			if (Main.xpBarLevelTextPosition.Value == Main.Position.Below) {
				var tempPos = levelTextRect.anchoredPosition;
				tempPos.y *= -1f;
				levelTextRect.anchoredPosition = tempPos;
			}

			xpBarContainerRect.anchoredPosition = Main.xpBarPosition.Value;
		}

		private void CreateSkillUI()
		{
			skillsScrollRect.verticalScrollbar = Main.showScrollbar.Value ? skillsScrollbar : null;
			skillsScrollbar.gameObject.SetActive(Main.showScrollbar.Value);

			closeButton.onClick.RemoveAllListeners();
			resetPointsButton.onClick.RemoveAllListeners();

			// Add listener for the buttons
			closeButton.onClick.AddListener(delegate { ToggleSkillsUI(false); });

			resetPointsButton.onClick.AddListener(delegate { SkillManager.Instance.SkillResetAll(); });

			OpenCategory(0);
		}

		public void ToggleSkillsUI(bool value)
		{
			skillsUI.alpha = value ? 1f : 0f;
			skillsUI.interactable = value;
			skillsUI.blocksRaycasts = value;

			skillsUIVisible = value;
			viewportContent.anchoredPosition3D = Vector3.zero;

			skillsUI.gameObject.SetActive(value);
			GUIManager.BlockInput(value);

			if (value) {
				rpc_ReloadConfig.SendPackage(ZRoutedRpc.Everybody, new ZPackage());
				CreateSkillUI();
			}
		}

		private void UpdateUIInformation()
		{
			SkillManager.Instance.UpdateAllSkillInformation();
			SkillManager.Instance.UpdateUnspendPoints();
		}

		public void UpdateCategoryPoints()
		{
			for (var i = 0; i < skillsCategoryButtons.Count; i++) {
				var points = SkillManager.Instance.GetSkillPointSpendOnCategory((SkillCategory)i);
				var categoryName = ((SkillCategory)i).ToString();
				skillsCategoryButtons[i].GetComponentInChildren<Text>().text = categoryName + " (" + points + ")";
			}
		}

		public void FadeInXPBar(float fadeTime)
		{
			if (!xpBarVisible) StartCoroutine(XPBarFadeIn(fadeTime));
		}

		private IEnumerator XPBarFadeIn(float fadeTime)
		{
			xpBarVisible = true;
			ConsoleLog.Print("Showing xp bar!");
			xpBarGroup.alpha = 0f;

			for (var f = 0f; f < fadeTime; f += Time.deltaTime) {
				var perc = f / fadeTime;
				xpBarGroup.alpha = perc;
				yield return null;
			}

			xpBarGroup.alpha = 1f;
		}

		public void FadeOutXPBar(float fadeTime)
		{
			if (xpBarVisible) StartCoroutine(XPBarFadeOut(fadeTime));
		}

		private IEnumerator XPBarFadeOut(float fadeTime)
		{
			xpBarVisible = false;

			ConsoleLog.Print("Removing xp bar!");
			xpBarGroup.alpha = 1f;

			for (var f = 0f; f < fadeTime; f += Time.deltaTime) {
				var perc = f / fadeTime;
				xpBarGroup.alpha = 1 - perc;
				yield return null;
			}

			xpBarGroup.alpha = 0f;
		}

		private static IEnumerator RPC_LevelUpEffect(long sender, ZPackage package)
		{
			ConsoleLog.Print("Level up VFX: " + Main.levelUpVFX.Value);

			if (Main.levelUpVFX.Value) {
				ConsoleLog.Print("A");
				if (Player.m_localPlayer != null) {
					ConsoleLog.Print("B");
					var playerID = package.ReadLong();

					var colls = Physics.OverlapSphere(Player.m_localPlayer.transform.position, 40f);
					foreach (var coll in colls) {
						var player = coll.GetComponent<Player>();
						if (player == null) continue;

						if (player.GetPlayerID() != playerID) continue;

						ConsoleLog.Print("C");
						var newEffect = Instantiate(levelUpEffect, player.GetCenterPoint(), Quaternion.identity, player.transform);
						Destroy(newEffect, 6f);
						break;
					}
				}
			}

			yield return null;
		}

		private IEnumerator LevelUpFadeIn()
		{
			NetworkHandler.LevelUpVFX();

			levelUpGroup.alpha = 0f;
			levelUpText.text = "Level " + playerLevel;
			levelUpTextShadow.text = levelUpText.text;

			var fadeInTime = 1f;
			var fadeOutTime = 2f;
			var waitTime = 3f;

			for (var f = 0f; f < fadeInTime; f += Time.deltaTime) {
				var perc = f / fadeInTime;
				levelUpGroup.alpha = perc;
				yield return null;
			}

			levelUpGroup.alpha = 1f;
			yield return new WaitForSeconds(waitTime);

			for (var f = 0f; f < fadeOutTime; f += Time.deltaTime) {
				var perc = f / fadeOutTime;
				levelUpGroup.alpha = 1 - perc;
				yield return null;
			}

			levelUpGroup.alpha = 0f;
		}

		private static IEnumerator RPC_ReloadConfig(long sender, ZPackage package)
		{
			if (ZNet.instance.IsServer()) ConsoleLog.ReloadConfig();

			yield return null;
		}

		private static IEnumerator RPC_AddExperience(long sender, ZPackage package)
		{
			var playerID = package.ReadLong();
			var awardedXP = package.ReadInt();
			var itemType = package.ReadString();
			var restedBonusXP = package.ReadInt();

			if (Player.m_localPlayer != null)
				if (playerID == Player.m_localPlayer.GetPlayerID()) {
					ConsoleLog.Print("Received Experience");
					var totalXpAward = awardedXP;
					Instance.AddExperience(awardedXP);

					var restedStatusEffect = Player.m_localPlayer.GetSEMan().GetStatusEffect(s_statusEffectRested);
					if (restedStatusEffect != null) {
						Instance.AddExperience(restedBonusXP, XPType.Rested);
						totalXpAward += restedBonusXP;
					}

					switch (itemType) {
						case "Woodcutting":
							if (Main.displayWoodcuttingXPText.Value) SpawnFloatingXPText(totalXpAward);
							break;
						case "Mining":
							if (Main.displayMiningXPText.Value) SpawnFloatingXPText(totalXpAward);
							break;
						case "Pickable":
							if (Main.displayPickupXPText.Value) SpawnFloatingXPText(totalXpAward);
							break;
						default:
							yield break;
					}
				}

			yield return null;
		}

		private static IEnumerator RPC_AddExperienceMonster(long sender, ZPackage package)
		{
			var awardedXP = package.ReadInt();
			var monsterLevelBonusXp = package.ReadInt();
			var restedBonusXp = package.ReadInt();
			var playerID = package.ReadLong();
			var monsterName = package.ReadString();

			if (Player.m_localPlayer != null)
				if (playerID == Player.m_localPlayer.GetPlayerID()) {
					ConsoleLog.Print("Received Expereience from " + monsterName);

					var totalXpGained = 0;

					var SERested = Player.m_localPlayer.GetSEMan().GetStatusEffect(s_statusEffectRested);

					if (awardedXP > 0) {
						Instance.AddExperience(awardedXP);
						totalXpGained += awardedXP;
					}

					if (monsterLevelBonusXp > 0) {
						Instance.AddExperience(monsterLevelBonusXp, XPType.MonsterLevel);
						totalXpGained += monsterLevelBonusXp;
					}

					if (SERested != null) {
						Instance.AddExperience(restedBonusXp, XPType.Rested);
						totalXpGained += restedBonusXp;
					}

					if (Main.displayMonsterXPText.Value) SpawnFloatingXPText(totalXpGained);
				}

			yield return null;
		}


		private static void SpawnFloatingXPText(int totalXpGained)
		{
			if (totalXpGained > 0 && Main.displayXPFloatingText.Value) {
				var spread = 0.35f;
				Vector3 spawnSpread;

				do {
					spawnSpread = Vector3.zero;
					spawnSpread += Random.Range(-spread, spread) * Camera.main.transform.right; // Randomize x
					spawnSpread += Random.Range(0f, spread) * Camera.main.transform.up; // Randomize y
					spawnSpread += Random.Range(0f, spread / 2f) * Camera.main.transform.forward; // Randomize z 
				} while (Vector3.Distance(spawnSpread, lastXPTextSpawnPosition) < spread * 0.5f);

				lastXPTextSpawnPosition = spawnSpread;

				var spawnPosition = Player.m_localPlayer.GetTopPoint() + spawnSpread;
				var xpText = Instantiate(xpTextFloating, spawnPosition, Quaternion.identity).GetComponent<XPText>();
				xpText.XPGained(totalXpGained);
			}
		}

		public void AddExperience(int xp, XPType type = XPType.Regular)
		{
			if (!IsPlayerMaxLevel() && xp > 0) {
				if (Main.displayXPInCorner.Value)
					switch (type) {
						case XPType.Regular:
							Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "You gained +" + xp + "xp");
							break;
						case XPType.MonsterLevel:
							Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "-> Monster level bonus: +" + xp + "xp");
							break;
						case XPType.Rested:
							Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "-> Rested bonus: +" + xp + "xp");
							break;
					}

				playerXP += xp;

				while (playerXP >= XPTable.playerXPTable[playerLevel - 1]) {
					gainedNewLevel = true;
					playerXP -= XPTable.playerXPTable[playerLevel - 1];
					playerLevel++;

					XPManager.Instance.SetPlayerLevel(playerLevel);
					StartCoroutine(LevelUpFadeIn());

					SkillManager.Instance.UpdateUnspendPoints();

					if (IsPlayerMaxLevel()) break;
				}

				XPManager.Instance.SetPlayerXP(playerXP);

				UpdateUI();

				XPManager.Instance.SavePlayerLevel();
				XPManager.Instance.SavePlayerXP();
			}
		}

		public void UpdateUI(bool instantUpdate = false)
		{
			UpdateLevelText();

			if (IsPlayerMaxLevel()) {
				SetXPText("Max Level");
				playerXP = 0;
				xpFill.fillAmount = 1f;
				return;
			}

			float xpToNextLevel = XPTable.playerXPTable[playerLevel - 1];
			var xpPercentage = playerXP / xpToNextLevel;

			xpFillTarget = xpPercentage;

			var xpString = "";

			xpString += Main.showXp.Value ? playerXP.ToString() : "";
			if (Main.showXp.Value) {
				xpString += Main.showRequiredXp.Value ? " / " + xpToNextLevel : "";
				xpString += Main.showPercentageXP.Value ? " (" + (xpPercentage * 100).ToString("N0") + "%)" : "";
			}
			else {
				xpString += Main.showPercentageXP.Value ? (xpPercentage * 100).ToString("N0") + "%" : "";
			}

			if (instantUpdate) xpFill.fillAmount = xpFillTarget;

			SetXPText(xpString);
		}

		public void SetXPText(string text)
		{
			text = Localization.instance.Localize(text);
			xpText.text = text;
			xpTextShadow.text = text;
		}

		public void UpdateLevelText()
		{
			levelText.text = Main.showLevel.Value ? "Level " + playerLevel : "";
			levelTextShadow.text = levelText.text;
		}

		public bool IsPlayerMaxLevel() { return playerLevel > XPTable.playerXPTable.Length; }

		public void DestroySelf()
		{
			XPManager.Instance.SetPlayerLevel(playerLevel);
			XPManager.Instance.SetPlayerXP(playerXP);
			Destroy(gameObject);
		}
	}
}

public enum XPType
{
	Regular,
	MonsterLevel,
	Rested
}