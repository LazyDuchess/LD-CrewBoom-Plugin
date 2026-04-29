using HarmonyLib;
using Reptile;

namespace CrewBoom.Patches
{
    [HarmonyPatch(typeof(SaveSlotHandler), nameof(SaveSlotHandler.SetCurrentSaveSlotDataBySlotId))]
    public class SaveSlotHandlerLoadPatch
    {
        public static void Postfix(int saveSlotId)
        {
            CharacterSaveSlots.LoadSlot(saveSlotId);
        }
    }
}
