namespace ImpactParry;

using HarmonyLib;
using System.Collections;
using UnityEngine;

/// <summary> Simpler Harmony Patches. </summary>
[HarmonyPatch]
public static class Patches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TimeController), "TimeIsStopped")]
    private static void ImpactPatch(float length)
    {
        if (!MyCoolBluuToon.enabled.Value) return;
        var mgr = ImpactManager.Instance;
        if (mgr == null) return;

        if (length >= 0.15f)
            mgr.StartCoroutine(HandleImpactWindow(length));
    }

    private static IEnumerator HandleImpactWindow(float length)
    {
        var mgr = ImpactManager.Instance;
        mgr.EnableEffect();
        yield return new WaitForSecondsRealtime(length);
        mgr.DisableEffect();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CheatsManager), "Start")]
    private static void CreateCheat(CheatsManager __instance) =>
        __instance.RegisterExternalCheat(new ImpactCheat());
}