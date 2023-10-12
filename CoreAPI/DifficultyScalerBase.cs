using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cozyheim.DifficultyScaler;

public class DifficultyScalerBase : MonoBehaviour
{
	private readonly Dictionary<DifficultyScalerMultiplier, float> _multipliers = new();

	public float StartHealth { get; private set; }
	public int Level { get; private set; }

	private void Awake()
	{
		var characterComponent = GetComponent<Character>();
		if (characterComponent == null) return;

		StartHealth = characterComponent.m_health;
	}

	private void Start() { Setup(); }

	private void Setup()
	{
		var characterComponent = GetComponent<Character>();
		if (characterComponent == null) return;

		if (!TryGetComponent<ZNetView>(out var nview)) return;

		var zdo = nview.GetZDO();
		if (zdo == null) return;

		Level = zdo.GetInt(ZDOVars.s_level, 1);
		var currentNetHealth = zdo.GetFloat(ZDOVars.s_maxHealth, StartHealth);
		var shouldScale = Mathf.Approximately(currentNetHealth, StartHealth) || Mathf.Approximately(currentNetHealth, StartHealth * Level);
		if (!shouldScale)
			//Debug.Log($"Skipping scaling for {name}. NetHealth: {currentNetHealth} | StartHealth: {StartHealth} | StartHealth*Level: {StartHealth*Level}");
			return;

		StartHealth *= Level;

		//Debug.Log($"{characterComponent.name}: Level = {Level}");
		var multiplierSum = 1f + GetTotalHealthMultiplier();
		characterComponent.SetMaxHealth(StartHealth * multiplierSum);

		foreach (var keyValuePair in _multipliers) {
			var multiplier = GetMultiplier(keyValuePair.Key);
			//Debug.Log($"{name} -> {keyValuePair.Key.ToString()} Bonus: +{multiplier * 100f:N0}%");
		}

		/*Debug.Log($"{name} -> Total health bonus: +{GetTotalHealthMultiplier() * 100f:N0}%");
		Debug.Log($"{name} -> Total damage bonus: +{GetTotalDamageMultiplier() * 100f:N0}%");
		Debug.Log($"{characterComponent.name} -> Health: {StartHealth} -> {StartHealth * multiplierSum}");*/
	}

    /// <summary>
    ///     Sets the value of the specified multiplier type.
    /// </summary>
    /// <param name="multiplierType">The multiplier type to get the value of.</param>
    /// <param name="multiplierValue">The value to set the multiplier to.</param>
    public void SetMultiplier(DifficultyScalerMultiplier multiplierType, float multiplierValue)
	{
		if (_multipliers.ContainsKey(multiplierType)) {
			_multipliers[multiplierType] = multiplierValue;
			return;
		}

		_multipliers.Add(multiplierType, multiplierValue);
	}

    /// <summary>
    ///     Returns the multiplier value for the specified type.
    ///     The default return value will be 0 If there are no multiplier for the specified type.
    /// </summary>
    /// <param name="multiplierType">The multiplier type to get the value of.</param>
    /// <returns>
    ///     The multiplier value for the specified type.
    ///     The default return value will be 0 If there are no multiplier for the specified type.
    /// </returns>
    public float GetMultiplier(DifficultyScalerMultiplier multiplierType)
	{
		return _multipliers.TryGetValue(multiplierType, out var multiplier)
			? IsMultiplierBasedOnLevel(multiplierType)
				? (Level - 1) * multiplier
				: multiplier
			: 0.0f;
	}

	public float GetSumOfMultipliers(IEnumerable<DifficultyScalerMultiplier> multiplierTypes) { return multiplierTypes.Sum(GetMultiplier); }

	public float GetTotalDamageMultiplier()
	{
		return _multipliers.Sum(keyValuePair =>
			keyValuePair.Key == DifficultyScalerMultiplier.HealthMultiplier
				? 0.0f
				: GetMultiplier(keyValuePair.Key));
	}

	public float GetTotalHealthMultiplier()
	{
		return _multipliers.Sum(keyValuePair =>
			keyValuePair.Key == DifficultyScalerMultiplier.DamageMultiplier
				? 0.0f
				: GetMultiplier(keyValuePair.Key));
	}

	public bool IsMultiplierBasedOnLevel(DifficultyScalerMultiplier multiplierType) { return multiplierType == DifficultyScalerMultiplier.StarMultiplier; }
}