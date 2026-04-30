using CrewBoom.Behaviours;
using CrewBoom.Database;
using HarmonyLib;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewBoom.Patches
{
    [HarmonyPatch(typeof(PublicToilet), nameof(PublicToilet.DoSequence))]
    public class PublicToiletDoSequencePatch
    {
        public static void Prefix()
        {
            // Don't start the toilet sequence until our player model is fully loaded.
            var ply = WorldHandler.instance.GetCurrentPlayer();
            if (ply != null)
            {
                var stream = PlayerStreamingComponent.GetOrCreate(ply);
                stream.WaitForLoadSync();
            }
        }
    }
}
