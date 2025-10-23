using BepInEx;
using GameConsole.pcon;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using ULTRAKILL.Cheats;
using Configgy;
using System;

[BepInPlugin("doomahreal.ultrakill.impactparry", "ImpactParry", "1.0.0")]
[BepInDependency("Hydraxous.ULTRAKILL.Configgy", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    private ConfigBuilder config;
    private Harmony harmony;
    [Configgable(path: "Shader", displayName: "Reset Values To Defaults")]
    public static ConfigButton ResetMyCoolBluuToonButton = new ConfigButton(() => OHMYFUCKINGGODBRUH());

    void Awake()
    {
        Logger.LogInfo("hreoo worrl");
        config = new ConfigBuilder("doomahreal.ultrakill.impactparry", "ImpactParry");
        config.BuildAll();
        harmony = new Harmony("doomahreal.ultrakill.impactparry");
        harmony.PatchAll();
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryCreateManager();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var sceneName = SceneHelper.CurrentScene;
        if (!string.IsNullOrEmpty(sceneName) && sceneName != "Bootstrap" && sceneName != "Main Menu" && sceneName != "Intro")
        {
            if (notabigfanofthegovernment.Instance == null) TryCreateManager();
        }
    }

    static void TryCreateManager()
    {
        if (notabigfanofthegovernment.Instance != null) return;
        GameObject g = new GameObject("skibidi dop dop yes yes");
        g.AddComponent<notabigfanofthegovernment>();
    }

    [HarmonyPatch(typeof(TimeController), "TimeIsStopped")]
    static class whyareyoulookingatmelikethat
    {
        static void Prefix(float length)
        {
            if (!MyCoolBluuToon.enabled.Value) return;
            var mgr = notabigfanofthegovernment.Instance;
            if (mgr == null) return;

            if (length >= 0.15f)
                mgr.StartCoroutine(HandleImpactWindow(length));
        }

        static IEnumerator HandleImpactWindow(float length)
        {
            var mgr = notabigfanofthegovernment.Instance;
            mgr.EnableEffect();
            yield return new WaitForSecondsRealtime(length);
            mgr.DisableEffect();
        }
    }

    [HarmonyPatch(typeof(CheatsManager), "Start")]
    static class sothatgoesthatwayandigothatwayohgodohmygodoh
    {
        static void Prefix(CheatsManager __instance)
        {
            __instance.RegisterExternalCheat(new sdiybtwiltedemoji());
        }
    }

    private static void OHMYFUCKINGGODBRUH()
    {
        var t = typeof(MyCoolBluuToon);
        var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        var elements = t.GetFields(flags).Select(f => f.GetValue(null))
            .Concat(t.GetProperties(flags).Select(p => { try { return p.GetValue(null); } catch { return null; } }))
            .Where(o => o is Configgy.ConfigValueElement)
            .Cast<Configgy.ConfigValueElement>();

        foreach (var elem in elements)
        {
            var resetMethod = HarmonyLib.AccessTools.Method(elem.GetType(), "ResetValue", new Type[0]);
            if (resetMethod != null)
            {
                resetMethod.Invoke(elem, null);
            }
        }
    }
}

public class notabigfanofthegovernment : MonoBehaviour
{
    public static notabigfanofthegovernment Instance { get; private set; }
    public bool forceEffect = false;
    Shader blackShader;
    Shader whiteShader;
    AssetBundle loadedBundle;
    Camera hudCamera;
    Camera mainCamera;
    int activeCount = 0;
    CameraClearFlags hudPrevClear;
    Color hudPrevBg;
    CameraClearFlags mainPrevClear;
    Color mainPrevBg;

    void Awake()
    {
        Instance = this;
        LoadEmbeddedBundleAndShaders();
        DiscoverCamera();
        SubscribeToConfigChanges();
    }

    void SubscribeToConfigChanges()
    {
        MyCoolBluuToon.PosterizeLevels.OnValueChanged += (_) => UpdateShaderValues();
        MyCoolBluuToon.PosterizeStrength.OnValueChanged += (_) => UpdateShaderValues();
        MyCoolBluuToon.ShadingBlend.OnValueChanged += (_) => UpdateShaderValues();
        MyCoolBluuToon.Contrast.OnValueChanged += (_) => UpdateShaderValues();
        MyCoolBluuToon.Brightness.OnValueChanged += (_) => UpdateShaderValues();

        MyCoolBluuToon.WhiteTint.OnValueChanged += (_) => { if (!MyCoolBluuToon.UseIndividualInputs.Value) UpdateShaderValues(); };
        MyCoolBluuToon.BlackTint.OnValueChanged += (_) => { if (!MyCoolBluuToon.UseIndividualInputs.Value) UpdateShaderValues(); };

        MyCoolBluuToon.UseIndividualInputs.OnValueChanged += (_) => UpdateShaderValues();

        MyCoolBluuToon.WhiteR.OnValueChanged += (_) => { if (MyCoolBluuToon.UseIndividualInputs.Value) UpdateShaderValues(); };
        MyCoolBluuToon.WhiteG.OnValueChanged += (_) => { if (MyCoolBluuToon.UseIndividualInputs.Value) UpdateShaderValues(); };
        MyCoolBluuToon.WhiteB.OnValueChanged += (_) => { if (MyCoolBluuToon.UseIndividualInputs.Value) UpdateShaderValues(); };
        MyCoolBluuToon.BlackR.OnValueChanged += (_) => { if (MyCoolBluuToon.UseIndividualInputs.Value) UpdateShaderValues(); };
        MyCoolBluuToon.BlackG.OnValueChanged += (_) => { if (MyCoolBluuToon.UseIndividualInputs.Value) UpdateShaderValues(); };
        MyCoolBluuToon.BlackB.OnValueChanged += (_) => { if (MyCoolBluuToon.UseIndividualInputs.Value) UpdateShaderValues(); };
    }

    void LoadEmbeddedBundleAndShaders()
    {
        var asm = Assembly.GetExecutingAssembly();
        string targetName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("ImpactParry.doomahfunnyshaders.bundle"));
        if (targetName == null) return;
        using (var s = asm.GetManifestResourceStream(targetName))
        {
            if (s == null) return;
            byte[] data = new byte[s.Length];
            s.Read(data, 0, data.Length);
            loadedBundle = AssetBundle.LoadFromMemory(data);
            if (loadedBundle != null)
            {
                blackShader = loadedBundle.LoadAsset<Shader>("assets/custom/hiddenblackoutreplacement.shader");
                whiteShader = loadedBundle.LoadAsset<Shader>("assets/custom/whiteshaded.shader");
            }
        }
    }

    void DiscoverCamera()
    {
        GameObject hudObj = GameObject.Find("Player/Main Camera/HUD Camera");
        if (hudObj != null) hudCamera = hudObj.GetComponent<Camera>();
        GameObject mainObj = GameObject.Find("Player/Main Camera");
        if (mainObj != null) mainCamera = mainObj.GetComponent<Camera>();
    }

    public void EnableEffect()
    {
        if (activeCount > 0) return;
        activeCount = 1;
        ApplyReplacementShaderToHUD();
        SetUIVisible(false);
    }

    public void DisableEffect()
    {
        if (activeCount <= 0 || forceEffect) return;
        activeCount = 0;
        ResetReplacementShaderFromHUD();
        SetUIVisible(true);
    }

    void SetUIVisible(bool visible)
    {
        var cheatsManager = MonoSingleton<CheatsManager>.Instance;
        if (cheatsManager == null) return;

        var hideUI = cheatsManager.GetCheatInstance<HideUI>();
        if (hideUI == null) return;

        if (visible)
            hideUI.Disable();
        else
            hideUI.Enable(cheatsManager);
    }

    void ApplyReplacementShaderToHUD()
    {
        if (hudCamera != null)
        {
            hudPrevClear = hudCamera.clearFlags;
            hudPrevBg = hudCamera.backgroundColor;
            if (whiteShader != null) hudCamera.SetReplacementShader(whiteShader, "RenderType");
            else if (blackShader != null) hudCamera.SetReplacementShader(blackShader, "RenderType");
            hudCamera.clearFlags = CameraClearFlags.SolidColor;
            hudCamera.backgroundColor = Color.black;
            int mask = 0;
            mask |= (1 << 0) | (1 << 9) | (1 << 10) | (1 << 11) | (1 << 12) | (1 << 13) | (1 << 25) | (1 << 27);
            hudCamera.cullingMask = mask;
        }
        if (mainCamera != null)
        {
            mainPrevClear = mainCamera.clearFlags;
            mainPrevBg = mainCamera.backgroundColor;
            if (blackShader != null) mainCamera.SetReplacementShader(blackShader, "RenderType");
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
        }
        UpdateShaderValues();
    }

    void UpdateShaderValues()
    {
        Shader.SetGlobalFloat("_PosterizeLevels", MyCoolBluuToon.PosterizeLevels.Value);
        Shader.SetGlobalFloat("_PosterizeStrength", MyCoolBluuToon.PosterizeStrength.Value);
        Shader.SetGlobalFloat("_ShadingBlend", MyCoolBluuToon.ShadingBlend.Value);
        Shader.SetGlobalFloat("_Contrast", MyCoolBluuToon.Contrast.Value);
        Shader.SetGlobalFloat("_Brightness", MyCoolBluuToon.Brightness.Value);

        Color whiteColor;
        Color blackColor;

        if (MyCoolBluuToon.UseIndividualInputs.Value)
        {
            whiteColor = new Color(
                MyCoolBluuToon.WhiteR.Value / 255f,
                MyCoolBluuToon.WhiteG.Value / 255f,
                MyCoolBluuToon.WhiteB.Value / 255f
            );

            blackColor = new Color(
                MyCoolBluuToon.BlackR.Value / 255f,
                MyCoolBluuToon.BlackG.Value / 255f,
                MyCoolBluuToon.BlackB.Value / 255f
            );
        }
        else
        {
            whiteColor = new Color(
                MyCoolBluuToon.WhiteTint.ValueArray[0] / 255f,
                MyCoolBluuToon.WhiteTint.ValueArray[1] / 255f,
                MyCoolBluuToon.WhiteTint.ValueArray[2] / 255f
            );

            blackColor = new Color(
                MyCoolBluuToon.BlackTint.ValueArray[0] / 255f,
                MyCoolBluuToon.BlackTint.ValueArray[1] / 255f,
                MyCoolBluuToon.BlackTint.ValueArray[2] / 255f
            );
        }

        Shader.SetGlobalColor("_WhiteTint", whiteColor);
        Shader.SetGlobalColor("_BlackTint", blackColor);
    }

    void ResetReplacementShaderFromHUD()
    {
        if (hudCamera != null)
        {
            hudCamera.ResetReplacementShader();
            hudCamera.clearFlags = hudPrevClear;
            hudCamera.backgroundColor = hudPrevBg;
            hudCamera.cullingMask = (1 << 13);
        }
        if (mainCamera != null)
        {
            mainCamera.ResetReplacementShader();
            mainCamera.clearFlags = mainPrevClear;
            mainCamera.backgroundColor = mainPrevBg;
        }
    }

    void OnDestroy()
    {
        if (loadedBundle != null) loadedBundle.Unload(false);
    }
}
