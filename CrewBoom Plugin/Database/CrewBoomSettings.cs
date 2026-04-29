using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewBoom.Database
{
    public static class CrewBoomSettings
    {
        public static bool StreamCharacters
        {
            get
            {
                return _streamCharacters.Value;
            }

            set
            {
                _streamCharacters.Value = value;
            }
        }

        public static bool LoadCharactersAsync
        {
            get
            {
                return _loadCharactersAsync.Value;
            }

            set
            {
                _loadCharactersAsync.Value = value;
            }
        }

        private static ConfigEntry<bool> _streamCharacters;
        private static ConfigEntry<bool> _loadCharactersAsync;

        public static void Initialize(ConfigFile configFile)
        {
            _streamCharacters = configFile.Bind("General", "Stream Characters", true, "If true, custom characters are dynamically loaded and unloaded during gameplay, as necessary. If false, all custom characters are loaded permanently at startup.");
            _loadCharactersAsync = configFile.Bind("General", "Stream Characters Async", true, "If true, and Stream Characters is also true, custom characters will be loaded in the background in order to keep gameplay smooth. This means there might be some pop-in as character models won't always be readily available in memory.");
        }
    }
}
