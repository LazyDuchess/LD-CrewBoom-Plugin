using BepInEx.Logging;
using CrewBoom.Compatibility;
using CrewBoom.Data;
using HarmonyLib;
using Reptile;

namespace CrewBoom.Patches
{
    [HarmonyPatch(typeof(CharacterVisual), nameof(CharacterVisual.GetCharacterFreestyleAnim))]
    public class CharacterFreestylePatch
    {
        public static void Prefix(ref Characters c)
        {
            if (CharacterDatabase.GetCharacter(c, out CustomCharacter customCharacter))
            {
                //c = (Characters)customCharacter.Definition.FreestyleAnimation; - TODO
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
                /*
                if (!string.IsNullOrWhiteSpace(customCharacter.Definition.BoEBounceAnimation) && BunchOfEmotesSupport.Installed)
                {
                    if (BunchOfEmotesSupport.TryGetGameAnimationForCustomAnimationName(customCharacter.Definition.BoEBounceAnimation, out var gameAnim))
                    {
                        __result = gameAnim;
                        return false;
                    }
                }
                c = (Characters)customCharacter.Definition.BounceAnimation; -- TODO
                */
                c = Characters.metalHead;
            }
            return true;
        }
    }
}
