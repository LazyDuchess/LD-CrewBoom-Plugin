using BepInEx.Bootstrap;
using CrewBoom.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CrewBoom.Compatibility
{
    public static class BunchOfEmotesSupport
    {
        public static bool Installed { get; private set; } = false;
        public static RuntimeAnimatorController AnimatorController => CustomAnimControllerField.GetValue(BoEPluginInstance) as RuntimeAnimatorController;
        private static object BoEPluginInstance = null;
        private static Type BoEPluginType = null;
        private static FieldInfo CustomAnimsField = null;
        private static FieldInfo CustomAnimControllerField = null;
        private static bool Cached = false;
        private static Dictionary<string, int> GameAnimationByCustomAnimationName = new();
        private static HashSet<int> GameAnimations = new();

        public static void Initialize()
        {
            Installed = true;
            BoEPluginInstance = Chainloader.PluginInfos["com.Dragsun.BunchOfEmotes"].Instance;
            BoEPluginType = ReflectionUtility.GetTypeByName("BunchOfEmotes.BunchOfEmotesPlugin");
            CustomAnimsField = BoEPluginType.GetField("myCustomAnims2");
            CustomAnimControllerField = BoEPluginType.GetField("myAnim");
        }

        public static bool IsCustomAnimation(int animHash)
        {
            if (!Installed) return false;
            CacheAnimationsIfNecessary();
            return GameAnimations.Contains(animHash);
        }

        public static void CacheAnimations()
        {
            Cached = true;
            var boeDictionary = CustomAnimsField.GetValue(BoEPluginInstance) as Dictionary<int, string>;
            foreach (var customAnim in boeDictionary)
            {
                var gameAnim = customAnim.Key;
                GameAnimationByCustomAnimationName[customAnim.Value] = gameAnim;
                GameAnimations.Add(gameAnim);
            }
        }

        public static void CacheAnimationsIfNecessary()
        {
            if (Cached) return;
            CacheAnimations();
        }

        public static bool TryGetGameAnimationForCustomAnimationName(string name, out int gameAnim)
        {
            if (!Installed)
            {
                gameAnim = 0;
                return false;
            }
            CacheAnimationsIfNecessary();
            if (GameAnimationByCustomAnimationName.TryGetValue(name, out gameAnim))
                return true;
            return false;
        }
    }
}
