using CrewBoom.Data;
using HarmonyLib;
using Reptile;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace CrewBoom.Patches
{
    [HarmonyPatch(typeof(GraffitiLoader), nameof(GraffitiLoader.LoadGraffitiArtInfoAsync))]
    public class GraffitiLoadAsyncPatch
    {
        private static IEnumerator Postfix(IEnumerator __result, Assets assets, GraffitiLoader __instance)
        {
            AssetBundleRequest graffitiArtInfoRequest = assets.LoadAssetFromBundleASync<GraffitiArtInfo>("graffiti", "graffitiartinfo");
            yield return graffitiArtInfoRequest;

            GraffitiArtInfo info = (GraffitiArtInfo)graffitiArtInfoRequest.asset;

            // Manually find specifically red's default graffiti because we patch FindByCharacter and Title
            var redGraffiti = info.graffitiArt.Find(g => g.title == "Red");
            Shader shader = redGraffiti.graffitiMaterial.shader;
            CharacterDatabase.SetGraffitiShader(shader);

            for (int i = 0; i < System.Enum.GetValues(typeof(Characters)).Length - 1; i++)
            {
                Characters character = (Characters)i;
                if (CharacterDatabase.GetCharacter(character, out CustomCharacter customCharacter))
                {
                    if (customCharacter.StreamData.HasGraffiti)
                    {
                        GraffitiArt graffiti = info.FindByCharacter(character);

                        var mainTex = customCharacter.StreamData.GraffitiTexture;
                        graffiti.graffitiMaterial.mainTexture = mainTex;
                    }
                }
            }

            __instance.graffitiArtInfo = graffitiArtInfoRequest.asset as GraffitiArtInfo;
            yield break;
        }
    }

    [HarmonyPatch(typeof(GraffitiArtInfo), nameof(GraffitiArtInfo.FindByCharacter))]
    public class GraffitiFindCharacterPatch
    {
        public static bool Prefix(ref Characters character, ref GraffitiArt __result)
        {
            if (character > Characters.MAX)
            {
                if (CharacterDatabase.GetCharacter(character, out CustomCharacter customCharacter))
                {
                    __result = customCharacter.Graffiti;
                    return false;
                }
                character = Characters.metalHead;
            }

            return true;
        }
    }
    [HarmonyPatch(typeof(GraffitiArtInfo), nameof(GraffitiArtInfo.FindByTitle))]
    public class GraffitiFindTitlePatch
    {
        public static void Postfix(ref GraffitiArt __result, string grafTitle)
        {
            if (__result != null || !string.IsNullOrEmpty(grafTitle))
            {
                if (CharacterDatabase.GetCharacterWithGraffitiTitle(grafTitle, out CustomCharacter customCharacter))
                {
                    __result = customCharacter.Graffiti;
                }
            }
        }
    }

    //Note:
    //Patching just the UI title since the characters are replacements
    [HarmonyPatch(typeof(GraffitiGame), nameof(GraffitiGame.SetStateVisual))]
    public class GraffitiVisualPatch
    {
        public static void Postfix(GraffitiGame.GraffitiGameState setState, Player ___player, GraffitiArt ___grafArt, GraffitiArtInfo ___graffitiArtInfo)
        {
            if (setState == GraffitiGame.GraffitiGameState.SHOW_PIECE)
            {
                if (CharacterDatabase.GetCharacter(___player.character, out CustomCharacter customCharacter))
                {
                    if (customCharacter.StreamData.HasGraffiti)
                    {
                        if (___grafArt == ___graffitiArtInfo.FindByCharacter(___player.character))
                        {
                            ___player.ui.graffitiTitle.text = $"'{customCharacter.StreamData.GrafTitle}'";
                        }
                    }
                }
            }
        }
    }
}