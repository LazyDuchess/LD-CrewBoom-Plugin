using BepInEx.Logging;
using CrewBoom.Data;
using CrewBoom.Database;
using HarmonyLib;
using Reptile;
using UnityEngine;

namespace CrewBoom.Patches
{
    [HarmonyPatch(typeof(CharacterConstructor), nameof(CharacterConstructor.CreateNewCharacterVisual))]
    public class ConstructorCreateVisualPatch
    {
        public static bool Prefix(Characters character,
                                   RuntimeAnimatorController controller,
                                   bool IK,
                                   float setGroundAngleLimit,
                                   CharacterConstructor __instance,
                                   ref CharacterVisual __result)
        {
            if (CharacterDatabase.GetCharacter(character, out CustomCharacter customCharacter))
            {
                CharacterVisual characterVisual = null;
                if (customCharacter.Loaded)
                {
                    characterVisual = Object.Instantiate(customCharacter.Visual).AddComponent<CharacterVisual>();
                }
                else
                {
                    characterVisual = Object.Instantiate(CharacterStreamer.StreamingVisuals).AddComponent<CharacterVisual>();
                }
                characterVisual.Init(character, controller, IK, setGroundAngleLimit);
                characterVisual.gameObject.SetActive(true);
                __result = characterVisual;

                return false;
            }

            return true;
        }
    }
}
