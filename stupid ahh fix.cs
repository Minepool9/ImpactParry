namespace ImpactParry;

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> Really stupidly fixes the issue with certain enemies being hidden, fixes by changing their obj's layer. </summary>
[HarmonyPatch]
public static class FixOutdoorEnemies
{
    /// <summary> Logger, it does the log. </summary>
    private static plog.Logger log = new("FixOutdoorEnemies::");

    /// <summary> All the GameObjects to force onto the Default layer. </summary>
    private static readonly List<ForceDefaultOnActive> forcers = [];

    /// <summary> Tells all the forcers to go to the Default layer. </summary>
    public static void ApplyDefaultLayers() => 
        forcers.ForEach(forcer => forcer.SetDefault());

    /// <summary> Tells all the forcers to go to the Previous layer. </summary>
    public static void ApplyPreviousLayers() =>
        forcers.ForEach(forcer => forcer.ResetToPrevious());

    /// <summary> Adds a Component to the specified GameObjects that forces them to be on the Default layer when needed. </summary>
    /// <param name="gameObjects">The specified GameObjects to force to Default layer.</param>
    public static void AddForcer(params List<GameObject> gameObjects) => 
        gameObjects.ForEach(obj => forcers.Add(obj.AddComponent<ForceDefaultOnActive>()));

    #region Patches & Acquiring GameObjects
    /// <summary> Fixes the visibility of the Guttertank in an Impact Frame. </summary>
    /// <param name="__instance">Instance of the Guttertank.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Guttertank), "Start")]
    public static void GuttertankFix(Guttertank __instance)
    {
        var gameobject = new ChildGetter<Guttertank>(__instance).Get("Guttertank/Guttertank");
        log.Info($"GuttertankFix:: gameobject.name: {gameobject.name} gameobject.layer: {gameobject.layer}");
        AddForcer(gameobject);
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
            : [];

        AddForcer(gameObjects);
    }

    /// <summary> Gets one of the child GameObjects in an enemy. (even more ass) </summary>
    /// <typeparam name="T">The enemy component type to get children from.</typeparam>
    /// <param name="instance">The enemy component to get children from.</param>
    private class ChildGetter<T>(T instance) where T : Component
    {
        /// <summary> The enemy component to get children from. </summary>
        private readonly T instance = instance;

        /// <summary> Gets one of the children of the enemy. </summary>
        /// <param name="path">Path to the children, relative to the enemy.</param>
        /// <returns>The child.</returns>
        public GameObject Get(string path)
        {
            string[] childrenPaths = path.Split('/');
            Transform Child = null;

            foreach (string childPath in childrenPaths)
            {
                Child = Child == null ? instance.transform.Find(childPath) : Child = Child.Find(childPath);
            }

            return Child?.gameObject;
        }
    }
    #endregion
}

/// <summary> Forces the GameObjects that this is a Component of to be on the Default layer when active. </summary>
public class ForceDefaultOnActive : MonoBehaviour
{
    /// <summary> Previous layer of the GameObject so it can return to it's Previous layer, when active. </summary>
    public int PrevLayer;

    /// <summary> Whether the forcer is currently active. </summary>
    public bool Active;

    /// <summary> Sets the GameObjects layer to the Default layer. </summary>
    public void SetDefault()
    {
        Active = true;
        PrevLayer = gameObject.layer;
        gameObject.layer = 0;
    }

    /// <summary> Resets the GameObjects layer to what it was previously. </summary>
    public void ResetToPrevious()
    {
        Active = false;
        gameObject.layer = PrevLayer;
    }

    /// <summary> Makes sure the enemy doesn't change layers while the forcer is active. </summary>
    public void Update()
    {
        if (Active && gameObject.layer != 0) SetDefault();
    }
}