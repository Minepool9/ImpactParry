namespace ImpactParry;

using BepInEx;
using Configgy;
using HarmonyLib;
using ImpactParry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.SceneManagement;

[BepInPlugin("doomahreal.ultrakill.impactparry", "ImpactParry", "1.0.0")]
[BepInDependency("Hydraxous.ULTRAKILL.Configgy", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    private ConfigBuilder config;
    private Harmony harmony;

    [Configgable(path: "Shader", displayName: "Reset Values To Defaults", orderInList:10)]
    public static ConfigButton ResetMySettingsButton = new(ResetSettings);

    private void Awake()
    {
        Logger.LogInfo("hreoo worrl");
        config = new ConfigBuilder("doomahreal.ultrakill.impactparry", "ImpactParry");
        config.BuildAll();
        harmony = new Harmony("doomahreal.ultrakill.impactparry");
        harmony.PatchAll();
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryCreateManager();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ImpactManager.Instance != null) 
            ImpactManager.Instance.forceEffect = false;

        var sceneName = SceneHelper.CurrentScene;
        if (!string.IsNullOrEmpty(sceneName) && sceneName != "Bootstrap" && sceneName != "Main Menu" && sceneName != "Intro")
        {
            if (ImpactManager.Instance == null) TryCreateManager();
        }
    }

    private static void TryCreateManager()
    {
        if (ImpactManager.Instance != null) return;

        GameObject mgrObj = new("ImpactManager");
        mgrObj.AddComponent<ImpactManager>();
        DontDestroyOnLoad(mgrObj);
    }

    private static void ResetSettings()
    {
        Type UItype = typeof(Settings);
        BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        IEnumerable<ConfigValueElement> elements = UItype.GetFields(flags).Select(f => f.GetValue(null))
            .Concat(UItype.GetProperties(flags).Select(p => { try { return p.GetValue(null); } catch { return null; } }))
            .Where(o => o is ConfigValueElement)
            .Cast<ConfigValueElement>();

        foreach (var elem in elements)
        {
            MethodInfo resetMethod = AccessTools.Method(elem.GetType(), "ResetValue", []);
            resetMethod?.Invoke(elem, null);
        }
    }
}

public class ImpactManager : MonoBehaviour
{
    public static ImpactManager Instance { get; private set; }
    public bool forceEffect = false;
    public Shader blackShader;
    public Shader whiteShader;
    public AssetBundle loadedBundle;
    public Camera hudCamera;
    public Camera mainCamera;
    public int activeCount = 0;
    public CameraClearFlags mainPrevClear;
    public Color mainPrevBg;
    public int mainPrevMask;

    private void Awake()
    {
        Instance = this;
        LoadEmbeddedBundleAndShaders();
        DiscoverCamera();
        SubscribeToConfigChanges();
    }

    private void SubscribeToConfigChanges()
    {
        Settings.PosterizeLevels.OnValueChanged += (_) => UpdateShaderValues();
        Settings.PosterizeStrength.OnValueChanged += (_) => UpdateShaderValues();
        Settings.ShadingBlend.OnValueChanged += (_) => UpdateShaderValues();
        Settings.Contrast.OnValueChanged += (_) => UpdateShaderValues();
        Settings.Brightness.OnValueChanged += (_) => UpdateShaderValues();

        Settings.WhiteTint.OnValueChanged += (_) => { if (!Settings.UseIndividualInputs.Value) UpdateShaderValues(); };
        Settings.BlackTint.OnValueChanged += (_) => { if (!Settings.UseIndividualInputs.Value) UpdateShaderValues(); };
        Settings.BColor.OnValueChanged += (_) => { if (!Settings.UseIndividualInputs.Value) UpdateShaderValues(); };

        Settings.UseIndividualInputs.OnValueChanged += (_) => UpdateShaderValues();

        Settings.WhiteR.OnValueChanged += (_) => { if (Settings.UseIndividualInputs.Value) UpdateShaderValues(); };
        Settings.WhiteG.OnValueChanged += (_) => { if (Settings.UseIndividualInputs.Value) UpdateShaderValues(); };
        Settings.WhiteB.OnValueChanged += (_) => { if (Settings.UseIndividualInputs.Value) UpdateShaderValues(); };
        Settings.BlackR.OnValueChanged += (_) => { if (Settings.UseIndividualInputs.Value) UpdateShaderValues(); };
        Settings.BlackG.OnValueChanged += (_) => { if (Settings.UseIndividualInputs.Value) UpdateShaderValues(); };
        Settings.BlackB.OnValueChanged += (_) => { if (Settings.UseIndividualInputs.Value) UpdateShaderValues(); };
        Settings.BGR.OnValueChanged += (_) => { if (Settings.UseIndividualInputs.Value) UpdateShaderValues(); };
        Settings.BGG.OnValueChanged += (_) => { if (Settings.UseIndividualInputs.Value) UpdateShaderValues(); };
        Settings.BGB.OnValueChanged += (_) => { if (Settings.UseIndividualInputs.Value) UpdateShaderValues(); };
    }

    private void LoadEmbeddedBundleAndShaders()
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

    private void DiscoverCamera()
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
        ApplyShaders();
        SetUIVisible(false);
    }

    public void DisableEffect()
    {
        if (activeCount <= 0 || forceEffect) return;
        activeCount = 0;
        ResetReplacementShaderFromHUD();
        SetUIVisible(true);
    }

    private void SetUIVisible(bool visible)
    {
        CheatsManager cheatsManager = MonoSingleton<CheatsManager>.Instance;
        if (cheatsManager == null) return;

        HideUI hideUICheat = cheatsManager.GetCheatInstance<HideUI>();
        if (hideUICheat == null) return;

        if (visible)
            hideUICheat.Disable();
        else
            hideUICheat.Enable(cheatsManager);
    }

    private void ApplyShaders()
    {
        if (whiteShader != null) hudCamera?.SetReplacementShader(whiteShader, "RenderType");
        else if (blackShader != null) hudCamera?.SetReplacementShader(blackShader, "RenderType");
        if (mainCamera != null)
        {
            // set up previous
            mainPrevClear = mainCamera.clearFlags;
            mainPrevBg = mainCamera.backgroundColor;
            mainPrevMask = mainCamera.cullingMask;

            // set shaders if not null
            if (whiteShader != null) mainCamera.SetReplacementShader(whiteShader, "RenderType");
            else if (blackShader != null) mainCamera.SetReplacementShader(blackShader, "RenderType");

            // fancy extra stuff to hide the enviroment and like make the background black
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
            mainCamera.cullingMask ^= LayerMaskDefaults.Get(LMD.Environment) | LayerMask.GetMask("Outdoors Non-solid");
        }
        UpdateShaderValues();
        FixOutdoorEnemies.ApplyDefaultLayers();
    }

    private void UpdateShaderValues()
    {

        if (blackShader == null && whiteShader == null)
            return;

        Shader.SetGlobalFloat("_PosterizeLevels", Settings.PosterizeLevels.Value);
        Shader.SetGlobalFloat("_PosterizeStrength", Settings.PosterizeStrength.Value);
        Shader.SetGlobalFloat("_ShadingBlend", Settings.ShadingBlend.Value);
        Shader.SetGlobalFloat("_Contrast", Settings.Contrast.Value);
        Shader.SetGlobalFloat("_Brightness", Settings.Brightness.Value);

        Color whiteColor;
        Color blackColor;
        Color bgColor;

        if (Settings.UseIndividualInputs.Value)
        {
            whiteColor = new Color(
                Settings.WhiteR.Value / 255f,
                Settings.WhiteG.Value / 255f,
                Settings.WhiteB.Value / 255f
            );

            blackColor = new Color(
                Settings.BlackR.Value / 255f,
                Settings.BlackG.Value / 255f,
                Settings.BlackB.Value / 255f
            );

            bgColor = new Color(
                Settings.BGR.Value / 255f,
                Settings.BGG.Value / 255f,
                Settings.BGB.Value / 255f
            );
        }
        else
        {
            whiteColor = new Color(
                Settings.WhiteTint.Value[0] / 255f,
                Settings.WhiteTint.Value[1] / 255f,
                Settings.WhiteTint.Value[2] / 255f
            );

            blackColor = new Color(
                Settings.BlackTint.Value[0] / 255f,
                Settings.BlackTint.Value[1] / 255f,
                Settings.BlackTint.Value[2] / 255f
            );

            bgColor = new Color(
                Settings.BColor.Value[0] / 255f,
                Settings.BColor.Value[1] / 255f,
                Settings.BColor.Value[2] / 255f
            );
        }

        Shader.SetGlobalColor("_WhiteTint", whiteColor);
        Shader.SetGlobalColor("_BlackTint", blackColor);
        if (mainCamera != null)
            mainCamera.backgroundColor = bgColor;
    }

    private void ResetReplacementShaderFromHUD()
    {
        FixOutdoorEnemies.ApplyPreviousLayers();
        hudCamera?.ResetReplacementShader();
        if (mainCamera != null)
        {
            mainCamera.ResetReplacementShader();
            mainCamera.clearFlags = mainPrevClear;
            mainCamera.backgroundColor = mainPrevBg;
            mainCamera.cullingMask = mainPrevMask;
        }
    }

    private void OnDestroy() => loadedBundle?.Unload(false);
}
