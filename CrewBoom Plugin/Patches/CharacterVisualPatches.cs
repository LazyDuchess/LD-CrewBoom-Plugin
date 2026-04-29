using BepInEx.Logging;
using CrewBoom.Compatibility;
using CrewBoom.Data;
using HarmonyLib;
using Reptile;
using UnityEngine;

namespace CrewBoom.Patches
{
    [HarmonyPatch(typeof(CharacterVisual), nameof(CharacterVisual.GetCharacterFreestyleAnim))]
    public class CharacterFreestylePatch
    {
        public static void Prefix(ref Characters c)
        {
            if (CharacterDatabase.GetCharacter(c, out CustomCharacter customCharacter))
            {
                if (customCharacter.Loaded)
                    c = (Characters)customCharacter.Definition.FreestyleAnimation;
                else
                    c = Characters.metalHead;
            }
        }
    }
    [HarmonyPatch(typeof(CharacterVisual), nameof(CharacterVisual.GetCharacterBounceAnim))]
    public class CharacterBouncePatch
    {
        public static bool Prefix(ref Characters c, ref int __result)
        {
            if (CharacterDatabase.GetCharacter(c, out CustomCharacter customCharacter))
            {
                if (!string.IsNullOrWhiteSpace(customCharacter.StreamData.BoEIdleDance))
                {
                    if (!customCharacter.StreamData.BoEIdleDanceVanilla && BunchOfEmotesSupport.Installed)
                    {
                        if (BunchOfEmotesSupport.TryGetGameAnimationForCustomAnimationName(customCharacter.StreamData.BoEIdleDance, out var gameAnim))
                        {
                            __result = gameAnim;
                            return false;
                        }
                    }
                    else if (customCharacter.StreamData.BoEIdleDanceVanilla)
                    {
                        __result = Animator.StringToHash(customCharacter.StreamData.BoEIdleDance);
                        return false;
                    }
                }
                c = (Characters)customCharacter.StreamData.IdleDance;
            }
            return true;
        }
    }
}
