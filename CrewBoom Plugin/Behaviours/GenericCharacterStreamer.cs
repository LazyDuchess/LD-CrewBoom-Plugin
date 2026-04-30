using CrewBoom.Data;
using CrewBoom.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CrewBoom.Behaviours
{
    public class GenericCharacterStreamer : MonoBehaviour
    {
        private CustomCharacter _targetCustomCharacter = null;
        public static GenericCharacterStreamer Create(GameObject go, CustomCharacter character)
        {
            var cmp = go.AddComponent<GenericCharacterStreamer>();
            cmp._targetCustomCharacter = character;
            character.AddReference();
            return cmp;
        }

        private void OnDestroy()
        {
            if (_targetCustomCharacter != null)
            {
                CharacterStreamer.KeepAlive(_targetCustomCharacter, CrewBoomSettings.KeepAliveTime);
                _targetCustomCharacter.RemoveReference();
            }
        }
    }
}
