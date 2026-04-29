using BepInEx;
using BepInEx.Bootstrap;
using CrewBoom.Compatibility;
using HarmonyLib;
using Reptile;

namespace CrewBoom
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(CharacterAPIGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Dragsun.BunchOfEmotes", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string CharacterAPIGuid = "com.Viliger.CharacterAPI";

        private void Awake()
        {
            if (Chainloader.PluginInfos.ContainsKey(CharacterAPIGuid))
            {
                Logger.LogWarning("LD CrewBoom is incompatible with CharacterAPI (viliger) and will not load!\nUninstall CharacterAPI and restart the game if you want to use LD CrewBoom.");
                return;
            }

            Logger.LogMessage($"LD {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} starting...");

            CharacterDatabaseConfig.Initialize(Config);

            if (CharacterDatabase.Initialize())
            {
                Harmony harmony = new Harmony("softGoat.crewBoom");
                harmony.PatchAll();

                Logger.LogMessage($"Loaded all available characters!");
            }

            if (Chainloader.PluginInfos.ContainsKey("com.Dragsun.BunchOfEmotes"))
            {
                BunchOfEmotesSupport.Initialize();
                StageManager.OnStagePostInitialization += BoE_StageManager_OnStagePostInitialization;
            }
        }

        private void BoE_StageManager_OnStagePostInitialization()
        {
            BunchOfEmotesSupport.CacheAnimations();
        }
    }
}
