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
        if (!Settings.enabled.Value) return;
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TimeController), "ParryFlash")]
    static bool Prefix(TimeController __instance)
    {
        if (!Settings.enabled.Value) return true;
        __instance.parryFlashEnabled = false;
        if (__instance.parryLight && Object.Instantiate(__instance.parryLight, MonoSingleton<PlayerTracker>.Instance.GetTarget().position, Quaternion.identity, MonoSingleton<PlayerTracker>.Instance.GetTarget()).TryGetComponent<Light>(out var light))
            light.enabled = false;
        __instance.TrueStop(0.25f);
        MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
        MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.ParryFlash);
        return false;
    }
}