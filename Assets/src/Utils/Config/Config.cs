using System;
using System.Collections.Generic;
using System.Reflection;

namespace Game.Utils.Config
{
    [Serializable]
    public class Config
    {
        public LogLevel LogLevel;
        public LogLevel LogConsoleLevel;
        public bool LogPrefix;
        public bool LogMethod;
        [ConfigUIData(InputType.Text, ConfigCategory.General, "Save folder")]
        public string SaveFolder;
        public float SavingTargetFPS;
        public bool ConsoleEnabled;
        [ConfigUIData(InputType.Slider, ConfigCategory.Audio, "Music volume", 0.0f, 1.0f, false, true)]
        public float MusicVolume;
        [ConfigUIData(InputType.Toggle, ConfigCategory.Audio, "Mute music")]
        public bool MuteMusic;
        [ConfigUIData(InputType.Slider, ConfigCategory.Audio, "Sound effect volume", 0.0f, 1.0f, false, true)]
        public float SoundEffectVolume;
        [ConfigUIData(InputType.Toggle, ConfigCategory.Audio, "Mute sound effects")]
        public bool MuteSoundEffects;
        public List<SoundEffectConfig> SoundEffects;

        [ConfigUIData(InputType.Number, ConfigCategory.General, "Number test", -10.0f, 10.0f, true, true)]
        public float TestValue;

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

        public Config Clone()
        {
            Config clone = new Config();
            foreach(FieldInfo fieldInfo in GetType().GetFields()) {
                fieldInfo.SetValue(clone, fieldInfo.GetValue(this));
            }
            clone.SoundEffects = SoundEffects == null ? null : SoundEffects.Copy();
            return clone;
        }
    }

    [Serializable]
    public class SoundEffectConfig
    {
        public SoundEffectType Type;
        [ConfigUIData(InputType.Slider, ConfigCategory.Audio, "Sound effect volume", 0.0f, 1.0f, false, true)]
        public float Volume;
        [ConfigUIData(InputType.Toggle, ConfigCategory.Audio, "Mute sound effects")]
        public bool Mute;
    }

    public class ConfigUIDataAttribute : Attribute
    {
        public InputType Type { get; private set; }
        public ConfigCategory Category { get; private set; }
        public string LabelText { get; private set; } = null;
        public string LabelLocalizationKey { get; private set; } = null;
        public string LabelLocalizationTable { get; private set; } = null;
        public LString Label { get; private set; } = null;

        public float MinValue { get; private set; } = float.MinValue;
        public float MaxValue { get; private set; } = float.MaxValue;
        public bool AllowDecimals { get; private set; } = false;
        public bool IsPercentage { get; protected set; } = false;
        public int MaxLenght { get; private set; } = int.MaxValue;

        public ConfigUIDataAttribute(InputType type, ConfigCategory category, string label, int maxLenght = int.MaxValue)
        {
            Type = type;
            Category = category;
            LabelText = label;
            Label = label;
            MaxLenght = maxLenght;
        }

        public ConfigUIDataAttribute(InputType type, ConfigCategory category, string labelKey, string labelTable, int maxLenght = int.MaxValue)
        {
            Type = type;
            Category = category;
            Label = new LString(labelKey, labelTable);
            LabelText = Label;
            MaxLenght = maxLenght;
        }

        public ConfigUIDataAttribute(InputType type, ConfigCategory category, string label, float minValue, float maxValue, bool allowDecimals = true, bool isPercentage = false)
        {
            Type = type;
            Category = category;
            LabelText = label;
            Label = label;
            MinValue = minValue;
            MaxValue = maxValue;
            AllowDecimals = allowDecimals;
            IsPercentage = isPercentage;
        }

        public ConfigUIDataAttribute(InputType type, ConfigCategory category, string labelKey, string labelTable, float minValue, float maxValue, bool allowDecimals = true,
            bool isPercentage = false)
        {
            Type = type;
            Category = category;
            Label = new LString(labelKey, labelTable);
            LabelText = Label;
            MinValue = minValue;
            MaxValue = maxValue;
            AllowDecimals = allowDecimals;
            IsPercentage = isPercentage;
        }
    }
}