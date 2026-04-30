using CrewBoom.Data;
using CrewBoom.Database;
using Reptile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CrewBoom.Behaviours
{
    public class PlayerStreamingComponent : MonoBehaviour
    {
        private CustomCharacter _targetCustomCharacter = null;
        private Characters _targetCharacter = Characters.NONE;
        private Player _player = null;
        private int _targetOutfit = 0;

        public static PlayerStreamingComponent GetOrCreate(Player player)
        {
            var comp = player.GetComponent<PlayerStreamingComponent>();
            if (comp != null) return comp;
            comp = player.gameObject.AddComponent<PlayerStreamingComponent>();
            comp._player = player;
            return comp;
        }

        public void WaitForLoadSync()
        {
            if (_targetCustomCharacter == null) return;
            _targetCustomCharacter.WaitForLoadSync();
            OnLoad(_targetCustomCharacter);
        }

        public void SetCharacter(Characters target, int outfit, bool preloaded = false)
        {
            _targetOutfit = outfit;
            if (target != _targetCharacter)
            {
                if (_targetCustomCharacter != null)
                {
                    CharacterStreamer.KeepAlive(_targetCustomCharacter, CrewBoomSettings.KeepAliveTime);
                    _targetCustomCharacter.OnLoadedCallback -= OnLoad;
                    _targetCustomCharacter.RemoveReference();
                }
            }
            else
            {
                return;
            }
            _targetCharacter = target;
            _targetCustomCharacter = null;
            if (target > Characters.MAX)
            {
                if (CharacterDatabase.GetCharacter(target, out var customChar))
                {
                    _targetCustomCharacter = customChar;
                    _targetCustomCharacter.AddReference();
                    if (!preloaded)
                        _targetCustomCharacter.OnLoadedCallback += OnLoad;
                }
            }
        }

        public void SetOutfit(int outfit)
        {
            _targetOutfit = outfit;
        }

        private void OnDestroy()
        {
            if (_targetCustomCharacter != null)
            {
                CharacterStreamer.KeepAlive(_targetCustomCharacter, CrewBoomSettings.KeepAliveTime);
                _targetCustomCharacter.OnLoadedCallback -= OnLoad;
                _targetCustomCharacter.RemoveReference();
            }
        }

        private void OnLoad(CustomCharacter customChar)
        {
            if (customChar != _targetCustomCharacter) return;
            _targetCustomCharacter.OnLoadedCallback -= OnLoad;
            UpdateCharacterModel();
        }

        private void UpdateCharacterModel()
        {
            if (_player.visualTf != null)
            {
                Destroy(_player.visualTf.gameObject);
            }
            _player.characterVisual = _player.characterConstructor.CreateNewCharacterVisual(_targetCharacter, _player.animatorController, !_player.isAI, _player.motor.groundDetection.groundLimit);
            _player.characterMesh = _player.characterVisual.mainRenderer.sharedMesh;
            if (_targetOutfit > 0)
            {
                _player.SetOutfit(_targetOutfit);
            }
            _player.characterVisual.transform.SetParent(_player.transform.GetChild(0), false);
            _player.characterVisual.transform.localPosition = Vector3.zero;
            _player.characterVisual.transform.rotation = Quaternion.LookRotation(base.transform.forward);
            _player.characterVisual.anim.gameObject.AddComponent<AnimationEventRelay>().Init();
            _player.visualTf = _player.characterVisual.transform;
            _player.headTf = _player.visualTf.FindRecursive("head");
            _player.phoneDirBone = _player.visualTf.FindRecursive("phoneDirection");
            _player.heightToHead = (_player.headTf.position - _player.visualTf.position).y;
            _player.isGirl = _player.characterVisual.isGirl;
            _player.anim = _player.characterVisual.anim;
            if (_player.curAnim != 0)
            {
                var newAnim = _player.curAnim;
                _player.curAnim = 0;
                _player.PlayAnim(newAnim, false, false, -1f);
            }
            _player.characterVisual.InitVFX(_player.VFXPrefabs);
            _player.characterVisual.InitMoveStyleProps(_player.MoveStylePropsPrefabs);
            _player.characterConstructor.SetMoveStyleSkinsForCharacter(_player, _targetCharacter);
            if (_player.characterVisual.hasEffects)
            {
                _player.boostpackTrail = _player.characterVisual.VFX.boostpackTrail.GetComponent<TrailRenderer>();
                _player.boostpackTrailDefaultWidth = _player.boostpackTrail.startWidth;
                _player.boostpackTrailDefaultTime = _player.boostpackTrail.time;
                _player.spraypaintParticles = _player.characterVisual.VFX.spraypaint.GetComponent<ParticleSystem>();
                _player.characterVisual.VFX.spraypaint.transform.localScale = Vector3.one * 0.5f;
                _player.SetDustEmission(0);
                _player.ringParticles = _player.characterVisual.VFX.ring.GetComponent<ParticleSystem>();
                _player.SetRingEmission(0);
            }
            var wasMovestyleEquipped = _player.usingEquippedMovestyle;
            var movestyleEquipped = _player.moveStyleEquipped;
            _player.SetMoveStyle(MoveStyle.ON_FOOT, true, true);
            _player.SetCurrentMoveStyleEquipped(movestyleEquipped, true, true);
            _player.InitVisual();
            if (wasMovestyleEquipped)
                _player.SwitchToEquippedMovestyle(true, false, true, false);
        }
    }
}
