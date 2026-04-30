using BepInEx.Logging;
using CrewBoom.Behaviours;
using CrewBoom.Compatibility;
using CrewBoom.Data;
using CrewBoom.Database;
using CrewBoomAPI;
using HarmonyLib;
using Reptile;
using System;
using UnityEngine;

namespace CrewBoom.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.SetCharacter))]
    public class PlayerInitOverridePatch
    {
        public static void Prefix(Player __instance, ref Characters setChar, int setOutfit)
        {
            if (CharacterDatabase.HasCharacterOverride)
            {
                if (CharacterDatabase.GetCharacterValueFromGuid(CharacterDatabase.CharacterOverride, out Characters character))
                {
                    if (character > Characters.MAX)
                    {
                        setChar = character;
                    }
                }
            }

            var preload = false;
            if (CharacterDatabase.GetCharacter(setChar, out var customChar))
            {
                if (customChar.Loaded)
                {
                    preload = true;
                }
                else if (!CrewBoomSettings.LoadCharactersAsync)
                {
                    customChar.WaitForLoadSync();
                    preload = true;
                }
            }
            var streamingComp = PlayerStreamingComponent.GetOrCreate(__instance);
            streamingComp.SetCharacter(setChar, setOutfit, preload);
        }

        public static void Postfix(Player __instance, Characters setChar)
        {
            if (CharacterDatabase.HasCharacterOverride)
            {
                CharacterDatabase.SetCharacterOverrideDone();
            }

            if (__instance == WorldHandler.instance.GetCurrentPlayer())
            {
                if (CharacterDatabase.GetCharacter(setChar, out CustomCharacter character))
                {
                    var info = new CrewBoomAPI.CharacterInfo(character.StreamData.Name, character.StreamData.GrafTitle);
                    CrewBoomAPIDatabase.UpdatePlayerCharacter(info);
                }
                else
                {
                    CrewBoomAPIDatabase.UpdatePlayerCharacter(null);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.SetOutfit))]
    public class PlayerSetOutfitPatch
    {
        public static bool Prefix(int setOutfit, Player __instance, CharacterVisual ___characterVisual, Characters ___character)
        {
            var streamingComp = PlayerStreamingComponent.GetOrCreate(__instance);
            streamingComp.SetOutfit(setOutfit);

            if (!CharacterDatabase.HasCharacter(___character))
            {
                return true;
            }

            if (!__instance.isAI)
            {
                Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(___character).outfit = setOutfit;

                if (___character > Characters.MAX)
                {
                    if (CharacterDatabase.GetFirstOrConfigCharacterId(___character, out Guid guid))
                    {
                        CharacterSaveSlots.SaveCharacterData(guid);
                    }
                }
            }

            if (CharUtil.TrySetCustomOutfit(___characterVisual, setOutfit, out SkinnedMeshRenderer firstActiveRenderer))
            {
                ___characterVisual.mainRenderer = firstActiveRenderer;
            }

            return false;
        }
    }
    [HarmonyPatch(typeof(Player), nameof(Player.SetCurrentMoveStyleEquipped))]
    public class PlayerSetMovestyleEquipped
    {
        public static void Postfix(Player __instance, MoveStyle setMoveStyleEquipped)
        {
            if (!__instance.isAI)
            {
                if (__instance.character > Characters.MAX)
                {
                    if (CharacterDatabase.GetFirstOrConfigCharacterId(__instance.character, out Guid guid))
                    {
                        if (CharacterSaveSlots.GetCharacterData(guid, out CharacterProgress progress))
                        {
                            progress.moveStyle = setMoveStyleEquipped;
                            CharacterSaveSlots.SaveCharacterData(guid);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.SaveSelectedCharacter))]
    public class PlayerSaveCharacterPatch
    {
        public static bool Prefix(Player __instance, ref Characters selectedCharacter)
        {
            bool runOriginal = true;

            bool isNew = selectedCharacter > Characters.MAX;
            if (!__instance.isAI)
            {
                CharacterSaveSlots.CurrentSaveSlot.LastPlayedCharacter = Guid.Empty;

                if (isNew)
                {
                    if (CharacterDatabase.GetFirstOrConfigCharacterId(selectedCharacter, out Guid guid))
                    {
                        CharacterSaveSlots.CurrentSaveSlot.LastPlayedCharacter = guid;
                    }
                    runOriginal = false;
                }

                CharacterSaveSlots.SaveSlot();
            }
            else if (selectedCharacter > Characters.MAX)
            {
                runOriginal = false;
            }

            return runOriginal;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.PlayAnim))]
    public class PlayerPlayAnimPatch
    {
        public static bool Prefix(Player __instance, int newAnim)
        {
            var doingboedance = false;

            if (CharacterDatabase.GetCharacter(__instance.character, out var customChar))
            {
                if (customChar.StreamData.BoEIdleDanceVanilla && __instance.curAnim == __instance.characterVisual.bounceAnimHash)
                    doingboedance = true;
                else if (!customChar.StreamData.BoEIdleDanceVanilla && BunchOfEmotesSupport.Installed && BunchOfEmotesSupport.IsCustomAnimation(__instance.curAnim) && __instance.curAnim == __instance.characterVisual.bounceAnimHash)
                    doingboedance = true;
            }

            if (doingboedance && (newAnim == __instance.idleHash || newAnim == __instance.idleFidget1Hash || newAnim == __instance.stopRunHash))
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.PlayVoice))]
    public class PlayerVoicePatch
    {
        public static bool Prefix(AudioClipID audioClipID,
                                  VoicePriority voicePriority,
                                  bool fromPlayer,
                                  AudioManager ___audioManager,
                                  ref VoicePriority ___currentVoicePriority,
                                  Characters ___character,
                                  AudioSource ___playerGameplayVoicesAudioSource)
        {
            if (___character > Characters.MAX && CharacterDatabase.GetCharacter(___character, out CustomCharacter customCharacter))
            {
                if (fromPlayer)
                {
                    //ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("Test");
                    //log.LogMessage(___currentVoicePriority);

                    //___audioManager.InvokeMethod("PlayVoice",
                    //    new Type[] { typeof(VoicePriority).MakeByRefType(), typeof(Characters), typeof(AudioClipID), typeof(AudioSource), typeof(VoicePriority) },
                    //    ___currentVoicePriority, ___character, audioClipID, ___playerGameplayVoicesAudioSource, voicePriority);

                    //log.LogMessage(___currentVoicePriority);
                }
                else
                {
                    ___audioManager.PlaySfxGameplay(customCharacter.SfxID, audioClipID, 0.0f);
                    return false;
                }

            }

            return true;
        }
    }
}
