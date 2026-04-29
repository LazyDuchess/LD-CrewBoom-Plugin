using CrewBoom.Data;
using HarmonyLib;
using Reptile;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Playables;
using UnityEngine;

namespace CrewBoom.Patches
{
    [HarmonyPatch(typeof(CharacterSelect), nameof(CharacterSelect.PopulateListOfSelectableCharacters))]
    public class CharacterSelectPopulateListPatch
    {
        public static void Postfix(Player player, List<Characters> ___selectableCharacters, CharacterSelect __instance)
        {
            if (CharacterDatabase.NewCharacterCount == 0)
            {
                return;
            }

            int max = (int)Characters.MAX;
            for (int i = max + 1; i <= max + CharacterDatabase.NewCharacterCount; i++)
            {
                Characters character = (Characters)i;
                if (player.character != character && CharacterDatabase.HasCypherEnabledForCharacter(character))
                {
                    ___selectableCharacters.Add(character);
                }
            }

            __instance.Shuffle(___selectableCharacters);
        }
    }

    [HarmonyPatch(typeof(CharacterSelect), nameof(CharacterSelect.SetPlayerToCharacter))]
    public class CharacterSelectSetPlayerPatch
    {
        public static bool Prefix(int index,
                                  out Characters __state,
                                  CharacterSelect __instance,
                                  CharacterSelectCharacter[] ___charactersInCircle,
                                  Player ___player)
        {
            CharacterSelectCharacter characterSelectCharacter = ___charactersInCircle[index];
            __state = characterSelectCharacter.character;

            if (__state > Characters.MAX)
            {
                Characters character1 = ___player.character;
                Characters character2 = characterSelectCharacter.character;
                ___player.SetCharacter(character2);
                ___player.InitVisual();
                CharacterVisual playerVisual = ___player.characterVisual;
                ___player.PlayAnim(playerVisual.bounceAnimHash);
                ___player.SetCurrentMoveStyleEquipped(Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character2).moveStyle);
                CharacterVisual swapVisual = characterSelectCharacter.visual;
                ___player.SetRotHard(swapVisual.tf.forward);
                UnityEngine.Object.Destroy(characterSelectCharacter.gameObject);
                __instance.CreateCharacterSelectCharacter(character1, index, CharacterSelectCharacter.CharSelectCharState.BOUNCING);
                return false;
            }

            return true;
        }

        public static void Postfix(Player ___player, Characters __state)
        {
            int outfit = CharUtil.GetSavedCharacterOutfit(__state);

            CharacterVisual visual = ___player.characterVisual;

            CharUtil.TrySetCustomOutfit(visual, outfit, out _);
        }
    }

    [HarmonyPatch(typeof(CharacterSelect), nameof(CharacterSelect.CreateCharacterSelectCharacter))]
    public class CharacterSelectCreateCharacterPatch
    {
        public static bool Prefix(Characters character,
                                  int numInCircle,
                                  CharacterSelectCharacter.CharSelectCharState startState,
                                  CharacterSelect __instance,
                                  Player ___player,
                                  CharacterSelectCharacter[] ___charactersInCircle,
                                  List<Vector3> ___characterPositions,
                                  List<Vector3> ___characterDirections,
                                  Transform ___tf,
                                  GameObject ___charCollision,
                                  GameObject ___charTrigger,
                                  GameObject ___swapSequence)
        {
            if (character > Characters.MAX)
            {
                CharacterVisual newCharacterVisual = ___player.CharacterConstructor.CreateNewCharacterVisual(character, ___player.animatorController, true, ___player.motor.groundDetection.groundLimit);
                ___charactersInCircle[numInCircle] = newCharacterVisual.gameObject.AddComponent<CharacterSelectCharacter>();
                ___charactersInCircle[numInCircle].transform.position = ___characterPositions[numInCircle];
                ___charactersInCircle[numInCircle].transform.rotation = Quaternion.LookRotation(___characterDirections[numInCircle] * -1f);
                ___charactersInCircle[numInCircle].transform.parent = ___tf;
                ___charactersInCircle[numInCircle].Init(__instance, character, ___charCollision, ___charTrigger, ___tf.position, UnityEngine.Object.Instantiate(___swapSequence, ___charactersInCircle[numInCircle].transform).GetComponent<PlayableDirector>());
                ___charactersInCircle[numInCircle].SetState(startState);

                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CharacterSelect), nameof(CharacterSelect.GetSelectSequenceAnim))]
    public class CharacterSelectSequencePatch
    {
        public static void Prefix(ref Characters c)
        {
            if (CharacterDatabase.GetCharacter(c, out CustomCharacter customCharacter))
            {
                c = (Characters)customCharacter.Definition.FreestyleAnimation;
            }
        }
    }

    [HarmonyPatch(typeof(CharacterSelectUI), nameof(CharacterSelectUI.SetCharacterInformation))]
    public class CharacterSelectUIInfoPatch
    {
        public static bool Prefix(Characters character,
                                  TextMeshProUGUI ___characterNameLabel,
                                  TextMeshProUGUI ___characterUnlockedOutfitCountLabel,
                                  CharacterSelectUI __instance)
        {
            if (character > Characters.MAX && CharacterDatabase.GetCharacter(character, out CustomCharacter customCharacter))
            {
                //SetCharacterName
                ___characterNameLabel.text = customCharacter.Definition.CharacterName;
                //SetCharacterOutfitsUnlocked
                ___characterUnlockedOutfitCountLabel.text = "4/4";
                if (CharacterSaveSlots.GetCharacterData(Guid.Parse(customCharacter.Definition.Id), out CharacterProgress data))
                {
                    __instance.SetCharacterSelectUIMoveStyle(data.moveStyle);
                }
                return false;
            }

            return true;
        }
    }
}
