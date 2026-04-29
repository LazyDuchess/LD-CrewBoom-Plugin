using HarmonyLib;
using Reptile;
using System;

namespace CrewBoom.Patches
{
    [HarmonyPatch(typeof(StyleSwitchMenu), nameof(StyleSwitchMenu.SkinButtonClicked))]
    public class StyleSwitchMenuPatch
    {
        public static void Postfix()
        {
            Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
            if (currentPlayer.character > Characters.MAX)
            {
                if (CharacterDatabase.GetFirstOrConfigCharacterId(currentPlayer.character, out Guid guid))
                {
                    CharacterSaveSlots.SaveCharacterData(guid);
                }
            }
        }
    }
}
