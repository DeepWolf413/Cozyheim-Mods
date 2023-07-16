using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace Cozyheim.DifficultyScaler.Patches;

[HarmonyPatch(typeof(SpawnSystem))]
public sealed class SpawnAreaPatch
{
	/*[HarmonyPatch("SpawnOne"), HarmonyTranspiler, HarmonyPriority(Priority.Last)]
	static IEnumerable<CodeInstruction> CallSetupMonster(IEnumerable<CodeInstruction> instructions)
	{
		ConsoleLog.Print("Setting up to call 'setup monster'.", LogType.Info);
		var codeMatcher = new CodeMatcher(instructions).End();
		ConsoleLog.Print($"At: {codeMatcher.Instruction.ToString()}");
		codeMatcher = codeMatcher.Advance(-3);
		ConsoleLog.Print($"At: {codeMatcher.Instruction.ToString()}");
		codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 5));
		ConsoleLog.Print($"At: {codeMatcher.Instruction.ToString()}");
		codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(SetupMonster).operand));
		ConsoleLog.Print($"[{codeMatcher.Remaining}] At: {codeMatcher.Instruction.ToString()}");

		foreach (var instruction in codeMatcher.InstructionEnumeration()) {
			ConsoleLog.Print($"{instruction.ToString()}");
		}
		
		return codeMatcher.InstructionEnumeration();
	}*/
}