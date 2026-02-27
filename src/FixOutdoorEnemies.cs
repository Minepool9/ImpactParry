namespace ImpactParry;

using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/// <summary> Really stupidly fixes the issue with certain enemies being hidden, fixes by changing their obj's layer. </summary>
[HarmonyPatch]
public static class FixOutdoorEnemies
{
    /// <summary> Logger, it does the log. </summary>
    private static readonly plog.Logger log = new("FixOutdoorEnemies::");

    /// <summary> All the GameObjects to force onto the Default layer. </summary>
    private static readonly List<ForceDefaultOnActive> forcers = [];

    /// <summary> Tells all the forcers to go to the Default layer. </summary>
    public static void ApplyDefaultLayers() => 
        forcers.ForEach(forcer => { 
            if (forcer != null && forcer?.gameObject != null)
                forcer?.SetDefault(); 
        });

    /// <summary> Tells all the forcers to go to the Previous layer. </summary>
    public static void ApplyPreviousLayers() =>
        forcers.ForEach(forcer => {
            if (forcer != null && forcer?.gameObject != null)
                forcer?.ResetToPrevious();
        });

    /// <summary> Adds a Component to the specified GameObjects that forces them to be on the Default layer when needed. </summary>
    /// <param name="gameObjects">The specified GameObjects to force to Default layer.</param>
    public static void AddForcers(params List<GameObject> gameObjects) => 
        gameObjects.ForEach(obj => {
            if (obj != null)
                forcers.Add(obj.AddComponent<ForceDefaultOnActive>());
        });

    #region Patches & Acquiring GameObjects
    /// <summary> Fixes the visibility of the Guttertank in an Impact Frame. </summary>
    /// <param name="__instance">Instance of the Guttertank.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Guttertank), "Start")]
    public static void GuttertankFix(Guttertank __instance)
    {
        GameObject gameobject = new ChildGetter<Guttertank>(__instance).Get("RotationTransform (PortalOffset)/BodyCenterRotation (PortalOffset)/Guttertank/Guttertank");
        AddForcers(gameobject);
    }

    /// <summary> Fixes the visibility of the Gutterman in an Impact Frame. </summary>
    /// <param name="__instance">Instance of the Gutterman.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Gutterman), "Start")]
    public static void GuttermanFix(Gutterman __instance)
    {
        ChildGetter<Gutterman> getter = new(__instance);
        List<GameObject> gameObjects = 
            [getter.Get("RotationTransform (PortalOffset)/BodyCenterRotation (PortalOffset)/Gutterman"), getter.Get("RotationTransform (PortalOffset)/BodyCenterRotation (PortalOffset)/Gutterman/Casket_Door"), getter.Get("RotationTransform (PortalOffset)/BodyCenterRotation (PortalOffset)/Gutterman/Gutterman"), getter.Get("RotationTransform (PortalOffset)/BodyCenterRotation (PortalOffset)/Gutterman/Shield")];
        
        AddForcers(gameObjects);
    }

    /// <summary> Fixes the visibility of the MortarLauncher in an Impact Frame. </summary>
    /// <param name="__instance">Instance of the MortarLauncher.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MortarLauncher), "Start")]
    public static void MortarLauncherFix(MortarLauncher __instance)
    {
        GameObject gameobject = new ChildGetter<MortarLauncher>(__instance).Get("Mortar");
        AddForcers(gameobject);
    }

    /// <summary> Fixes the visibility of the Maurice in an Impact Frame. </summary>
    /// <param name="__instance">Instance of the Maurice.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MaliciousFace), "Start")]
    public static void MauriceFix(MaliciousFace __instance)
    {
        GameObject gameobject = new ChildGetter<MaliciousFace>(__instance).Get("MaliciousFace/MaliciousFace");
        AddForcers(gameobject);
    }

    /// <summary> Fixes the visibility of the Cerberi in an Impact Frame. </summary>
    /// <param name="__instance">Instance of the Enemy.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Enemy), "Start")]
    public static void CerberiFix(Enemy __instance)
    {
        if (!__instance.IsStatue()) return;

        ChildGetter<Enemy> getter = new(__instance);

        List<GameObject> gameObjects =
        [
            getter.Get("RotationTransform (PortalOffset)/BodyCenterRotation (PortalOffset)/Cerberus/Cerberus"),
            getter.Get("RotationTransform (PortalOffset)/BodyCenterRotation (PortalOffset)/Cerberus/Cerb_Apple")
        ];

        AddForcers(gameObjects);
    }

    /// <summary> Fixes the visibility of the Minotaur in an Impact Frame. </summary>
    /// <param name="__instance">Instance of the Minotaur.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Minotaur), "Start")]
    public static void MinotaurFix(Minotaur __instance)
    {
        ChildGetter<Minotaur> getter = new(__instance);
        List<GameObject> gameObjects =
            [getter.Get("Minotaur_Rigging02/Minotaur"), getter.Get("Minotaur_Rigging02/Minotaur_Staff")];

        AddForcers(gameObjects);
    }

    /// <summary> Fixes the visibility of the Mandalore in an Impact Frame. </summary>
    /// <param name="__instance">Instance of the Mandalore.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Mandalore), "Start")]
    public static void MandaloreFix(Mandalore __instance)
    {
        ChildGetter<Mandalore> getter = new(__instance);
        List<GameObject> gameObjects =
            [getter.Get("Mandalore (Skeleton)/Skeleton"), getter.Get("Mandalore (Skeleton)/Armature.001/Base/Hips/Chest/Shammy/ShammyMesh"), 
            getter.Get("Mandalore (Skeleton)/Armature.001/Base/Hips/Chest/Neck/Head/Cylinder/Mandalore_Head")];

        AddForcers(gameObjects);
    }

    /// <summary> Fixes the visibility of the Panopticon in an Impact Frame. </summary>
    /// <param name="__instance">Instance of the Panopticon.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FleshPrison), "Start")]
    public static void PanopticonFix(FleshPrison __instance)
    {
        ChildGetter<FleshPrison> getter = new(__instance);
        List<GameObject> gameObjects = __instance.eid.FullName == "FLESH PANOPTICON"
            ? [getter.Get("FleshPrison2"), getter.Get("FleshPrison2/FleshPrison2_Base"), getter.Get("FleshPrison2/FleshPrison2_Head")]
            : [getter.Get("fleshprisonrigged/FleshPrison")];

        AddForcers(gameObjects);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Ferryman), "Start")]
    public static void FerrymanMaterialCheck(Ferryman __instance)
    {
        ChildGetter<Ferryman> getter = new(__instance);

        GameObject ferrymanObj = getter.Get("RotationTransform (PortalOffset)/BodyCenterRotation (PortalOffset)/Ferryman2/Ferryman");
        GameObject oarObj = getter.Get("RotationTransform (PortalOffset)/BodyCenterRotation (PortalOffset)/Ferryman2/Oar");

        if (ferrymanObj == null || oarObj == null) return;

        SkinnedMeshRenderer ferrymanSMR = ferrymanObj.GetComponentInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer oarSMR = oarObj.GetComponentInChildren<SkinnedMeshRenderer>();

        if (ferrymanSMR == null || oarSMR == null) return;

        Material[] ferrymanMats = ferrymanSMR.materials;
        Material[] oarMats = oarSMR.sharedMaterials;

        if (ferrymanMats.Length == 0 || oarMats.Length == 0) return;

        for (int i = 0; i < ferrymanMats.Length; i++)
        {
            Material inst = ferrymanMats[i];
            Material src = oarMats.Length > i ? oarMats[i] : oarMats[0];

            if (inst == null || src == null) continue;
            if (inst.shader != src.shader) continue;

            inst.shader = src.shader;

            if (inst.HasProperty("_Opacity")) inst.SetFloat("_Opacity", src.HasProperty("_Opacity") ? src.GetFloat("_Opacity") : 1f);
            if (inst.HasProperty("_BlendMode")) inst.SetFloat("_BlendMode", src.HasProperty("_BlendMode") ? src.GetFloat("_BlendMode") : 0f);
            if (inst.HasProperty("_CullMode")) inst.SetFloat("_CullMode", src.HasProperty("_CullMode") ? src.GetFloat("_CullMode") : 2f);

            inst.renderQueue = src.renderQueue;
        }

        ferrymanSMR.materials = ferrymanMats;
    }

    /// <summary> Fixes the visibility of the MirrorReaper in an Impact Frame. </summary>
    /// <param name="__instance">Instance of the MirrorReaper.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MirrorReaper), "Start")]
    public static void MirrorReaperFix(MirrorReaper __instance)
    {
        ChildGetter<MirrorReaper> getter = new(__instance);
        List<GameObject> gameObjects =
        [
            getter.Get("Mirror Reaper_Visual/Mirror Reaper")
        ];

        AddForcers(gameObjects);
    }

    /// <summary> Gets one of the child GameObjects in an enemy. (even more ass) </summary>
    /// <typeparam name="T">The enemy component type to get children from.</typeparam>
    /// <param name="instance">The enemy component to get children from.</param>
    public class ChildGetter<T>(T instance) where T : Component
    {
        /// <summary> The enemy component to get children from. </summary>
        private readonly T instance = instance;

        /// <summary> Gets one of the children of the enemy. </summary>
        /// <param name="path">Path to the children, relative to the enemy.</param>
        /// <returns>The child.</returns>
        public GameObject Get(string path)
        {
            if (instance == null)
            {
                Debug.LogError($"Instance was null while trying to resolve path: {path}");
                return null;
            }

            string[] childrenPaths = path.Split('/');
            Transform current = instance.transform;

            foreach (string childPath in childrenPaths)
            {
                if (current == null)
                {
                    Debug.LogError($"Null transform while resolving '{path}' on {instance.GetType().Name}");
                    return null;
                }

                Transform next = current.Find(childPath);

                if (next == null)
                {
                    Debug.LogError($"Could not find child '{childPath}' in path '{path}' on {instance.GetType().Name}");
                    return null;
                }

                current = next;
            }

            if (current == null)
            {
                Debug.LogError($"Final transform null for path '{path}' on {instance.GetType().Name}");
                return null;
            }

            return current.gameObject;
        }
    }
    #endregion
}

/// <summary> Forces the GameObjects that this is a Component of to be on the Default layer when active. </summary>
public class ForceDefaultOnActive : MonoBehaviour
{
    /// <summary> Previous layer of the GameObject so it can return to it's Previous layer, when active. </summary>
    public int PrevLayer = 0;

    /// <summary> Whether the forcer is currently active. </summary>
    public bool Active = false;

    /// <summary> Sets the GameObjects layer to the Default layer. </summary>
    public void SetDefault()
    {
        Active = true;
        if (gameObject == null) return;

        PrevLayer = gameObject?.layer ?? PrevLayer;
        gameObject.layer = 0;
    }

    /// <summary> Resets the GameObjects layer to what it was previously. </summary>
    public void ResetToPrevious()
    {
        Active = false;
        if (gameObject == null) return;

        gameObject.layer = PrevLayer;
    }

    /// <summary> Makes sure the enemy doesn't change layers while the forcer is active. </summary>
    public void Update()
    {
        if (Active && gameObject != null && gameObject?.layer != 0) SetDefault();
    }
}

