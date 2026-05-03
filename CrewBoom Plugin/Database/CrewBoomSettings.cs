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
        public static bool UpdateCBBs
        {
            get
            {
                return _updateCbbs.Value;
            }

            set
            {
                _updateCbbs.Value = value;
            }
        }
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

        public static bool UnloadCharacters
        {
            get
            {
                return _unloadCharacters.Value;
            }

            set
            {
                _unloadCharacters.Value = value;
            }
        }

        public static float KeepAliveTime
        {
            get
            {
                return _keepAliveTime.Value;
            }

            set
            {
                _keepAliveTime.Value = value;
            }
        }

        private static ConfigEntry<bool> _streamCharacters;
        private static ConfigEntry<bool> _loadCharactersAsync;
        private static ConfigEntry<bool> _unloadCharacters;
        private static ConfigEntry<float> _keepAliveTime;
        private static ConfigEntry<bool> _updateCbbs;

        public static void Initialize(ConfigFile configFile)
        {
            _streamCharacters = configFile.Bind("General", "Stream Characters", true, "If true, custom characters are dynamically loaded during gameplay, as necessary. If false, all custom characters are loaded at startup.");
            _unloadCharacters = configFile.Bind("General", "Unload Characters", true, "If true, and Stream Characters is also true, characters will be dynamically unloaded during gameplay as they're no longer used. If false, characters will never unload, lowering pop-in as characters load for the first time but increasing memory usage.");
            _loadCharactersAsync = configFile.Bind("General", "Stream Characters Async", true, "If true, and Stream Characters is also true, custom characters will be loaded in the background in order to keep gameplay smooth. This means there might be some pop-in as character models won't always be readily available in memory.");
            _keepAliveTime = configFile.Bind("General", "Keep Alive Time", 0.5f, "How long to keep characters lingering in memory after they've gone unused. Should reduce stutters and such as it keeps recent characters in memory, especially between transitions. Set to 0 to disable.");
            _updateCbbs = configFile.Bind("General", "Update CBBs", true, "If true, will update and overwrite your old .cbb files to embed .ldcs data into them. This makes them load way faster and streamable, while still keeping them compatible with original CrewBoom. If false, instead generates .ldcs data separate from the .cbb, for the same purpose.");
        }
    }
}
