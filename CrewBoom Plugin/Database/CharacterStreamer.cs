using CrewBoom.Data;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CrewBoom.Database
{
    public static class CharacterStreamer
    {
        private static HashSet<AssetBundleCreateRequest> _cancelledRequests = new();
        private static HashSet<CustomCharacter> _characters = new();
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

        public static void Update()
        {
            var remainingRequests = new HashSet<AssetBundleCreateRequest>();
            foreach(var req in _cancelledRequests)
            {
                if (req.isDone)
                {
                    req.assetBundle.Unload(true);
                }
                else
                {
                    remainingRequests.Add(req);
                }
            }
            _cancelledRequests = remainingRequests;

            var remainingCharacters = new HashSet<CustomCharacter>();
            foreach(var ch in _characters)
            {
                var result = ch.UpdateAsyncLoad();
                if (!result)
                {
                    remainingCharacters.Add(ch);
                }
            }
            _characters = remainingCharacters;
        }
    }
}
