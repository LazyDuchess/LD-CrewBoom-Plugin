using CrewBoom.Compatibility;
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

    [HarmonyPatch(typeof(CharacterSelectCharacter), nameof(CharacterSelectCharacter.SetState))]
    public class CharacterSelectCharacterSetStatePatch
    {
        public static void Prefix(CharacterSelectCharacter __instance, CharacterSelectCharacter.CharSelectCharState setState)
        {
            if (!BunchOfEmotesSupport.Installed || !BunchOfEmotesSupport.IsCustomAnimation(__instance.visual.bounceAnimHash)) return;
            if (setState == CharacterSelectCharacter.CharSelectCharState.TALKING || setState == CharacterSelectCharacter.CharSelectCharState.BOUNCING)
            {
                __instance.visual.anim.runtimeAnimatorController = BunchOfEmotesSupport.AnimatorController;
            }
        }
    }
}
