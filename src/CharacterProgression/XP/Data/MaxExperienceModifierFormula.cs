using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace CharacterProgressionMod
{
    public class MaxExperienceModifierFormula
    {
        /*
         * Format examples to parse:
         * 0=15
         * 0=15;10=[5,15%]
         * 0=15;10%=[5,15%]
         *
         * Flat values are indicated by using only a number (eg. 10, 50, 32).
         * Percentage values are indicated by a number with a percentage sign after it (eg. 5%, 30%, 78%).
         * A collection of values are surrounded by square-brackets [].
         * Example1: 0=15, the left(0) value determines when the increase value to the right(15) should be used. In this case, beginning at level 0, max experience will increase by 15.
         * Example2: 0=15%, the right value(15%) is a percentage value and will be used from level 0. The max experience will increase by 15% based on the old max experience, resulting in an exponential increase.
         * Example3: 10=[5,10%], the right value represents an array of values. Levels after level 10 will increase max experience by 5+10% of the old max experience.
         */
        private readonly Modifier[] _modifiers;
        
        /// <summary>
        /// Parses the <see cref="formula"/> and initializes the <see cref="MaxExperienceModifierFormula"/>.
        /// </summary>
        /// <param name="formula"></param>
        public MaxExperienceModifierFormula(string formula)
        {
            var modifiers = new List<Modifier>(5);
            var cleanedData = new string(formula.Where(c => !char.IsWhiteSpace(c)).ToArray());
            var entries = cleanedData.Split(';');
            foreach (var entry in entries) {
                var entryValues = entry.Split('=');
                if (entryValues.Length != 2) {
                    continue;
                }

                var level = entryValues[0];
                int parsedPosition;
                if (IsInteger(level)) {
                    parsedPosition = int.Parse(level, NumberStyles.Integer);
                }
                else {
                    continue;
                }
                
                var value = entryValues[1];
                bool isPercentage = false;
                int parsedValue;
                if (IsPercentage(value)) {
                    value = value.TrimEnd('%');
                    parsedValue = int.Parse(value, NumberStyles.Integer);
                    isPercentage = true;
                }
                else if (IsInteger(value)) {
                    parsedValue = int.Parse(value, NumberStyles.Integer);
                }
                else if (IsArray(value)) {
                    value = value.Trim('[', ']');
                    var arrayValues = value.Split(',');
                    foreach (var arrayValue in arrayValues) {
                        if (IsPercentage(arrayValue) && int.TryParse(arrayValue.TrimEnd('%'), out var percentageValue)) {
                            modifiers.Add(new Modifier(parsedPosition, percentageValue, true));
                            continue;
                        }

                        if (!IsInteger(arrayValue)) {
                            continue;
                        }

                        parsedValue = int.Parse(arrayValue, NumberStyles.Integer);
                        modifiers.Add(new Modifier(parsedPosition, parsedValue, false));
                    }

                    continue;
                }
                else {
                    continue;
                }
                
                modifiers.Add(new Modifier(parsedPosition, parsedValue, isPercentage));
            }

            _modifiers = modifiers.ToArray();

            static bool IsArray(string value) => value.StartsWith("[");

            static bool IsInteger(string value) => value.All(char.IsDigit);

            static bool IsPercentage(string value) => value.EndsWith("%");
        }

        public int Evaluate(int level, int oldMaxExperience)
        {
            var lastValidKey = _modifiers.LastOrDefault(key => level >= key.Level);
            var validKeys = _modifiers.Where(key => key.Level == lastValidKey.Level);
            var result = oldMaxExperience;
            foreach (var key in validKeys) {
                if (key.IsValuePercentage) {
                    result += Mathf.CeilToInt(oldMaxExperience * (key.Value / 100.0f));
                    continue;
                }

                result += key.Value;
            }

            return result;
        }

        private struct Modifier
        {
            public Modifier(int level, int value, bool isValuePercentage)
            {
                Level = level;
                Value = value;
                IsValuePercentage = isValuePercentage;
            }

            public int Level { get; }
            public int Value { get; }
            public bool IsValuePercentage { get; }
        }
    }
}