using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace Numeira;

[InitializeOnLoad]
[HarmonyPatch]
internal static class Patcher
{
    private const string ID = "numeira.ndmf-manual-bake-assets";
    private const string TargetDirectoryGUID = "c6e0bbb2f6b5d72468e103059284e277";

    static Patcher()
    {
        var harmony = new Harmony(ID);
        AssemblyReloadEvents.beforeAssemblyReload += () => { harmony.UnpatchAll(ID); };
        harmony.PatchAll();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(AvatarProcessor), nameof(AvatarProcessor.ManualProcessAvatar))]
    public static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> instructions)
    {
        var array = instructions.ToArray();
        var path = AssetDatabase.GUIDToAssetPath(TargetDirectoryGUID);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"[{ID}] Patch Failed: Target directory is not found");
        }

        foreach(var instr in array.AsSpan())
        {
            if (instr.opcode == OpCodes.Ldstr && instr.operand is "Assets/ZZZ_GeneratedAssets")
            {
                instr.operand = path;
                Debug.Log($"[{ID}] Patch Success: Target directory is now `{path}`");
                break;
            }
        }

        return array;
    }
}