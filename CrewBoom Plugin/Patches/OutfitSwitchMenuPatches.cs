

using BepInEx.Logging;
using CrewBoom.Data;
using CrewBoomMono;
using HarmonyLib;
using Reptile;
using System.Numerics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CrewBoom.Patches
{
    [HarmonyPatch(typeof(OutfitSwitchMenu), nameof(OutfitSwitchMenu.SkinButtonSelected))]
    public class OutfitSwitchSelectPatch
    {
        public static bool Prefix(OutfitSwitchMenu __instance, MenuTimelineButton ___buttonClicked, CharacterVisual ___previewCharacterVisual, int skinIndex)
        {
            if (__instance.IsTransitioning || ___buttonClicked != null)
            {
                return false;
            }

            if (CharUtil.TrySetCustomOutfit(___previewCharacterVisual, skinIndex, out _))
            {
                return false;
            }

            return true;
        }
    }
    [HarmonyPatch(typeof(OutfitSwitchMenu), nameof(OutfitSwitchMenu.SkinButtonClicked))]
    public class OutfitSwitchClickPatch
    {
        public static bool Prefix(OutfitSwitchMenu __instance,
                                  MenuTimelineButton clickedButton,
                                  int skinIndex,
                                  ref MenuTimelineButton ___buttonClicked,
                                  Player ___player)
        {
            if (___player.character > Characters.MAX)
            {
                if (CharacterDatabase.GetCharacter(___player.character, out CustomCharacter customCharacter))
                {
                    if (__instance.IsTransitioning || ___buttonClicked != null)
                    {
                        return false;
                    }

                    ___buttonClicked = clickedButton;
                    Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.confirm, 0.0f);

                    ___player.SetOutfit(skinIndex);
                    WantedManager.instance.StopPlayerWantedStatus(true, true);
                    __instance.StartCoroutine(__instance.clipBehaviour.FlickerDelayedButtonPress(clickedButton, new UnityAction(__instance.clipBehaviour.ExitMenu)));
                }
                return false;
            }

            return true;
        }
    }
    [HarmonyPatch(typeof(OutfitSwitchMenu), nameof(OutfitSwitchMenu.Activate))]
    public class OutfitSwitchActivatePatch
    {
        private static OutfitSwitchMenu _lastMenu;

        public static bool Prefix(OutfitSwitchMenu __instance,
                                  Player ___player,
                                  MenuTimelineButton[] ___buttons,
                                  TMProLocalizationAddOn[] ___texts,
                                  GameFontType ___normalGameFontType,
                                  GameFontType ___selectedGameFontType,
                                  float ___nonSelectableAlphaValue)
        {
            _lastMenu = __instance;

            SkinTextPatch.Character = ___player.character;

            if (___player.character > Characters.MAX)
            {
                if (CharacterDatabase.GetCharacter(___player.character, out CustomCharacter customCharacter))
                {
                    CharacterProgress characterProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(___player.character);
                    CharacterConstructor characterConstructor = ___player.CharacterConstructor;
                    MenuTimelineButton nextButtonUp = null;
                    for (int outfitIndex = 0; outfitIndex < 4; outfitIndex++)
                    {
                        string tagAtBegin = null;
                        if (characterProgress.outfit == outfitIndex)
                        {
                            tagAtBegin = "<u>";
                        }
                        ___texts[outfitIndex].AssignAndUpdateTextWithTags(SkinTextPatch.OUTFIT_NAMES[outfitIndex], GroupOptions.Skin, tagAtBegin, null);

                        MenuTimelineButton nextButtonDown = null;
                        if (outfitIndex < 3)
                        {
                            nextButtonDown = ___buttons[outfitIndex + 1];
                        }

                        MenuTimelineButton button = ___buttons[outfitIndex];
                        int skinIndex = outfitIndex;

                        button.SetButtonVariables(() => __instance.SkinButtonSelected(button, skinIndex), true, nextButtonUp, nextButtonDown, ___normalGameFontType, ___selectedGameFontType, ___nonSelectableAlphaValue);
                        button.interactable = true;
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() => __instance.SkinButtonSelected(button, skinIndex));
                        button.gameObject.SetActive(true);
                        nextButtonUp = button;
                    }
                }

                return false;
            }

            return true;
        }
    }
}
