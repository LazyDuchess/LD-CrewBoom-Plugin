using BepInEx;
using BepInEx.Bootstrap;
using CrewBoom.Behaviours;
using CrewBoom.Compatibility;
using CrewBoom.Database;
using HarmonyLib;
using Reptile;
using UnityEngine;

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

            CrewBoomSettings.Initialize(Config);
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
            StageManager.OnStagePostInitialization += StageManager_OnStagePostInitialization;
        }

        private void Update()
        {
            CharacterStreamer.Update(Time.deltaTime);
        }

        private void BoE_StageManager_OnStagePostInitialization()
        {
            BunchOfEmotesSupport.CacheAnimations();
        }

        private void StageManager_OnStagePostInitialization()
        {
            CharacterDatabase.RefreshShaders();

            // Wait for our character to finish loading so we avoid pop in.
            var ply = WorldHandler.instance.GetCurrentPlayer();
            if (ply != null)
            {
                var stream = PlayerStreamingComponent.GetOrCreate(ply);
                stream.WaitForLoadSync();
            }
        }
    }
}
