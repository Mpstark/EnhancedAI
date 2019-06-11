﻿using System.Reflection;
using BattleTech;
using AIToolkit.Features;
using Harmony;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace AIToolkit.Patches
{
    [HarmonyPatch]
    public static class SortMoveCandidatesByInfMapNode_drawDebugLines_Patch
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("SortMoveCandidatesByInfMapNode");
            return AccessTools.Method(type, "drawDebugLines");
        }

        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch]
    public static class SortMoveCandidatesByInfMapNode_Tick_Patch
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("SortMoveCandidatesByInfMapNode");
            return AccessTools.Method(type, "Tick");
        }

        public static void Postfix(object __instance, ref BehaviorTreeResults __result, AbstractActor ___unit)
        {
            if (__result.nodeState != BehaviorNodeState.Success || !Main.Settings.ShouldPauseAI)
                return;

            AIPause.InfluenceMapVisual.OnInfluenceMapSort(___unit);
        }
    }
}
