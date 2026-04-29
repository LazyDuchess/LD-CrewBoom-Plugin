using HarmonyLib;
using Reptile;

namespace CrewBoom.Patches
{
    [HarmonyPatch(typeof(CharacterSelectCharacter), nameof(CharacterSelectCharacter.Init))]
    public class CharacterSelectCharacterInitPatch
    {
        public static void Postfix(CharacterVisual ___visual, Characters setCharacter)
        {
            CharUtil.TrySetCustomOutfit(___visual, CharUtil.GetSavedCharacterOutfit(setCharacter), out _);
        }
    }
}
