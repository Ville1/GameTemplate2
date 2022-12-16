using System;
using System.Collections.Generic;

namespace Game.Utils.Config
{
    [Serializable]
    public class Config
    {
        public LogLevel LogLevel;
        public LogLevel LogConsoleLevel;
        public bool LogPrefix;
        public bool LogMethod;
        public string SaveFolder;
        public float SavingTargetFPS;
        public bool ConsoleEnabled;
        public float MusicVolume;
        public bool MuteMusic;
        public float SoundEffectVolume;
        public bool MuteSoundEffects;
        public List<SoundEffectConfig> SoundEffects;

        public static Config Default
        {
            get {
                return new Config() {
                    LogLevel = LogLevel.Debug,
                    LogConsoleLevel = LogLevel.Error,
                    LogPrefix = true,
                    LogMethod = true,
                    SaveFolder = "C:/",
                    SavingTargetFPS = 60.0f,
                    ConsoleEnabled = true,
                    MusicVolume = AudioManager.DEFAULT_MUSIC_VOLUME,
                    MuteMusic = false,
                    SoundEffectVolume = AudioManager.DEFAULT_SOUND_EFFECT_VOLUME,
                    MuteSoundEffects = false,
                    SoundEffects = new List<SoundEffectConfig>()
                };
            }
        }
    }

    [Serializable]
    public class SoundEffectConfig
    {
        public SoundEffectType Type;
        public float Volume;
        public bool Mute;
    }
}