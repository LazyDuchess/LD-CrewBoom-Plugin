using CrewBoom.Data;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace CrewBoom.Database
{
    public static class CharacterStreamer
    {
        private static HashSet<AssetBundleCreateRequest> _cancelledRequests = new();
        private static HashSet<CustomCharacter> _characters = new();
        private static Dictionary<CustomCharacter, float> _keepAlives = new();
        public static GameObject StreamingVisuals
        {
            get
            {
                if (_streamingVisuals == null)
                    MakeStreamingVisuals();
                return _streamingVisuals;
            }
        }

        private static GameObject _streamingVisuals;

        private static void MakeStreamingVisuals()
        {
            _streamingVisuals = UnityEngine.Object.Instantiate(Core.Instance.BaseModule.StageManager.characterConstructor.GetCharacterVisual(Characters.metalHead));
            _streamingVisuals.name = "Streaming Visuals";
            var skinnedRenda = _streamingVisuals.GetComponentInChildren<SkinnedMeshRenderer>(true);
            skinnedRenda.sharedMesh = null;
            _streamingVisuals.SetActive(false);
        }

        public static void AddToStreamQueue(CustomCharacter character)
        {
            _characters.Add(character);
        }

        public static void CancelBundleLoadRequest(CustomCharacter character)
        {
            _characters.Remove(character);
            if (character.BundleRequest.isDone)
            {
                character.BundleRequest.assetBundle.Unload(true);
            }
            else
            {
                _cancelledRequests.Add(character.BundleRequest);
            }
        }

        public static void Update(float delta)
        {
            _cancelledRequests.RemoveWhere(req =>
            {
                if (req.isDone)
                {
                    req.assetBundle.Unload(true);
                    return true;
                }
                return false;
            });

            _characters.RemoveWhere(req =>
            {
                return req.UpdateAsyncLoad();
            });

            var keys = new List<CustomCharacter>(_keepAlives.Keys);

            foreach (var key in keys)
            {
                var val = _keepAlives[key] - delta;
                _keepAlives[key] = val;

                if (val <= 0f)
                {
                    key.RemoveReference();
                    _keepAlives.Remove(key);
                }
            }

            if (Input.GetKeyDown(KeyCode.F8))
                LogStreamingStats();
        }

        public static void LogStreamingStats()
        {
            var beginIndex = Characters.MAX + 1;

            var loadedAmount = 0;
            var outOf = CharacterDatabase.NewCharacterCount;

            Console.WriteLine("----- CrewBoom Streaming Stats -----");
            for (var i = 0; i < CharacterDatabase.NewCharacterCount; i++)
            {
                if (CharacterDatabase.GetCharacter(beginIndex + i, out var customChar)) {
                    Console.WriteLine($"{customChar.StreamData.Name} - Loaded: " + (customChar.Loaded ? "YES" : "NO")  + $" - Refs: {customChar.References}");
                    if (customChar.Loaded)
                        loadedAmount++;
                }
            }
            Console.WriteLine($"In short, {loadedAmount} characters are loaded out of {outOf}");
            Console.WriteLine("--------------- END ----------------");
        }

        public static void KeepAlive(CustomCharacter character, float time)
        {
            if (time <= 0f) return;
            if (!character.Loaded) return;
            if (!_keepAlives.ContainsKey(character))
            {
                character.AddReference();
            }
            _keepAlives[character] = time;
        }
    }
}
