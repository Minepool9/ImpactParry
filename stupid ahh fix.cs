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
    #region GameObject list related
    /// <summary> Logger, it does the log. </summary>
    static plog.Logger log = new("FixOutdoorEnemies::");
    /// <summary> All GameObjects which need their layer changed to default on flash. </summary>
    public static Dictionary<GameObject, int> OutdoorObjects = [];

    // extra tools
    /// <summary> Adds an object to the end of GameObject List. </summary>
    /// <param name="obj"> The object to be added to the end of the GameObject List.</param>
    public static void Add(GameObject obj)
    {
        log.Info($"ADD:: obj.name: {obj.name} layer: {obj.layer}");
        OutdoorObjects.Add(obj, obj.layer);
    }
    /// <summary> Adds the elements of the specified collection to the end of the GameObject List. </summary>
    /// <param name="objs">The collection whose elements should be added to the end of the GameObject List.</param>
    public static void AddRange(params List<GameObject> objs) => objs.ForEach(Add);
    /// <summary> Performs the specified action on each element of the GameObject List. </summary>
    /// <param name="act">The action delegate to perform on each element of the GameObject List.</param>
    public static void ForEach(Action<GameObject> act) => ForEach((obj, _) => act(obj));
    /// <summary> Performs the specified action on each element of the GameObject List. </summary>
    /// <param name="act">The action delegate to perform on each element of the GameObject List.</param>
    public static void ForEach(Action<GameObject, int> act)
    {
        foreach (var pair/*(key)obj, (value)layer*/ in OutdoorObjects) if (pair.Key/*obj*/ != null) act(pair.Key/*obj*/, pair.Value/*layer*/);
    }
    #endregion

    #region Applying Layers
    /// <summary> Take all GameObjects in OutdoorObjects and set their layer to be Default. </summary>
    public static void ApplyDefaultLayer() => 
        ForEach((EnemyObj, PrevLayer) => EnemyObj.layer = 0/*LayerMask.NameToLayer("Default")*/);

    /// <summary> Take all GameObjects in OutdoorObjects and set their layer to be its previous layer before being set to default. </summary>
    public static void ApplyPreviousLayer() =>
        ForEach((EnemyObj, PrevLayer) => EnemyObj.layer = PrevLayer);
    #endregion

    #region Patches & Acquiring GameObjects
    /// <summary> Fixes the visibility of the Guttertank in an Impact Frame. </summary>
    /// <param name="__instance">Instance of the Guttertank.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Guttertank), "Start")]
    public static void GuttertankFix(Guttertank __instance)
    {
        var gameobject = new ChildGetter<Guttertank>(__instance).Get("Guttertank/Guttertank");
        log.Info($"GuttertankFix:: gameobject.name: {gameobject.name} gameobject.layer: {gameobject.layer}");
        Add(gameobject);
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

        AddRange(gameObjects);
        // check if the impact is active and set layer to default if so
        if (notabigfanofthegovernment.Instance.activeCount > 0) 
            gameObjects.ForEach(EnemyObj => EnemyObj.layer = 0);
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