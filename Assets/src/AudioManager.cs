using Game.Utils;
using Game.Utils.Config;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AudioManager : MonoBehaviour, IConfigListener
    {
        public static readonly float DEFAULT_MUSIC_VOLUME = 0.25f;
        public static readonly float DEFAULT_SOUND_EFFECT_VOLUME = 0.5f;

        public static AudioManager Instance;

        public GameObject DefaultSoundEffectSource;
        public GameObject DefaultMusicSource;

        private Dictionary<SoundEffectType, AudioSource> defaultSoundEffectSource;
        private AudioSource defaultMusicSource;

        private Dictionary<string, AudioClip> soundEffects;
        private Dictionary<string, AudioClip> music;

        private bool initialized;

        private float musicVolume;
        private bool muteMusic;
        private Dictionary<SoundEffectType, float> soundEffectVolume;
        private List<SoundEffectType> mutedSoundEffects;
        private string currentMusic = null;
        private Dictionary<GameObject, string> currentMusicFromObjects = new Dictionary<GameObject, string>();

        /// <summary>
        /// Initializiation
        /// </summary>
        private void Start()
        {
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            Instance = this;

            defaultSoundEffectSource = DictionaryHelper.CreateNewFromEnum((SoundEffectType type) => {
                string gameObjectName = string.Format("{0}Sound Effect Source", type == SoundEffectType.None ? string.Empty : type.ToString() + " ");
                GameObject gameObject = GameObjectHelper.Find(DefaultSoundEffectSource, gameObjectName);
                if(gameObject != null) {
                    return GetAudioSource(gameObject);
                }
                gameObject = new GameObject(gameObjectName);
                gameObject.transform.parent = DefaultSoundEffectSource.transform;
                return GetAudioSource(gameObject);
            });

            GameObject musicGameObject = GameObjectHelper.Find(DefaultMusicSource, "Music Source");
            if(musicGameObject != null) {
                defaultMusicSource = GetAudioSource(musicGameObject);
            } else {
                musicGameObject = new GameObject("Music Source");
                musicGameObject.transform.parent = DefaultMusicSource.transform;
                defaultMusicSource = GetAudioSource(musicGameObject);
            }

            soundEffects = new Dictionary<string, AudioClip>();
            music = new Dictionary<string, AudioClip>();
            initialized = false;
        }

        private bool Initialize()
        {
            if (initialized) {
                return false;
            }

            foreach (AudioClip clip in Resources.LoadAll<AudioClip>("audio/soundEffects")) {
                soundEffects.Add(clip.name, clip);
                CustomLogger.Debug("{SoundEffectLoaded}", clip.name);
            }
            foreach (AudioClip clip in Resources.LoadAll<AudioClip>("audio/music")) {
                music.Add(clip.name, clip);
                CustomLogger.Debug("{MusicTrackLoaded}", clip.name);
            }
            initialized = true;
            ConfigManager.RegisterListener(this);

            return true;
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {

        }

        public void ReadConfig()
        {
            musicVolume = Mathf.Clamp01(ConfigManager.Config.MusicVolume);
            muteMusic = ConfigManager.Config.MuteMusic;

            soundEffectVolume = new Dictionary<SoundEffectType, float>();
            mutedSoundEffects = new List<SoundEffectType>();

            if (ConfigManager.Config.MuteSoundEffects) {
                mutedSoundEffects.Add(SoundEffectType.None);
            }
            soundEffectVolume.Add(SoundEffectType.None, ConfigManager.Config.SoundEffectVolume);

            if(ConfigManager.Config.SoundEffects != null) {
                foreach (SoundEffectConfig soundEffectConfig in ConfigManager.Config.SoundEffects) {
                    if(soundEffectConfig.Type == SoundEffectType.None || soundEffectVolume.ContainsKey(soundEffectConfig.Type)) {
                        CustomLogger.Error("{SoundEffectConfigError}", soundEffectConfig.Type.ToString(), soundEffectConfig.Type == SoundEffectType.None ? "Invalid type" : "Duplicated key");
                        continue;
                    }
                    if (soundEffectConfig.Mute) {
                        mutedSoundEffects.Add(soundEffectConfig.Type);
                    }
                    soundEffectVolume.Add(soundEffectConfig.Type, soundEffectConfig.Volume);
                }
            }

            foreach(SoundEffectType soundEffectType in Enum.GetValues(typeof(SoundEffectType))) {
                if(soundEffectType != SoundEffectType.None && !soundEffectVolume.ContainsKey(soundEffectType)) {
                    soundEffectVolume.Add(soundEffectType, DEFAULT_SOUND_EFFECT_VOLUME);
                }
            }
        }

        public bool PlaySoundEffect(string name, float volumeMultiplier = 1.0f, SoundEffectType type = SoundEffectType.None, GameObject source = null)
        {
            Initialize();
            if (!soundEffects.ContainsKey(name)) {
                CustomLogger.Warning("{SoundEffectDoesNotExist}", name);
                return false;
            }
            Play(soundEffects[name], volumeMultiplier, false, source, type);
            return true;
        }

        public bool PlayMusic(string track, float volumeMultiplier = 1.0f, GameObject source = null)
        {
            Initialize();
            if (!music.ContainsKey(track)) {
                CustomLogger.Warning("{MusicTrackDoesNotExist}", track);
                return false;
            }
            Play(music[track], volumeMultiplier, true, source, SoundEffectType.None);
            UpdateCurrentMusic(volumeMultiplier > 0.0f ? track : null, source);
            return true;
        }

        public string CurrentMusic(GameObject sourceGameObject = null)
        {
            if (Initialize()) {
                return null;
            }
            if(sourceGameObject == null) {
                return currentMusic;
            } else {
                if (currentMusicFromObjects.ContainsKey(sourceGameObject)) {
                    return currentMusicFromObjects[sourceGameObject];
                } else {
                    return null;
                }
            }
        }

        public bool StopMusic(GameObject sourceGameObject = null)
        {
            if (Initialize()) {
                return false;
            }
            if(sourceGameObject == null) {
                if (defaultMusicSource.isPlaying) {
                    defaultMusicSource.Stop();
                    currentMusic = null;
                    return true;
                } else {
                    return false;
                }
            } else {
                if (currentMusicFromObjects.ContainsKey(sourceGameObject)) {
                    GetAudioSource(sourceGameObject).Stop();
                    currentMusicFromObjects.Remove(sourceGameObject);
                    return true;
                } else {
                    return false;
                }
            }
        }

        private void Play(AudioClip audio, float volumeMultiplier, bool isMusic, GameObject sourceGameObject, SoundEffectType type)
        {
            AudioSource audioSource;
            if(sourceGameObject != null) {
                audioSource = GetAudioSource(sourceGameObject);
            } else {
                if (isMusic) {
                    audioSource = defaultMusicSource;
                } else {
                    audioSource = defaultSoundEffectSource[type];
                }
            }

            audioSource.clip = audio;
            audioSource.volume = Mathf.Clamp01(volumeMultiplier * (isMusic ? musicVolume : soundEffectVolume[type]));
            audioSource.mute = volumeMultiplier == 0.0f ? true : (isMusic ? muteMusic : mutedSoundEffects.Contains(type));
            audioSource.loop = isMusic;
            audioSource.Play();
        }

        private AudioSource GetAudioSource(GameObject sourceGameObject)
        {
            AudioSource audioSource;
            if(!sourceGameObject.TryGetComponent(out audioSource)) {
                audioSource = sourceGameObject.AddComponent<AudioSource>();
            }
            return audioSource;
        }

        private void UpdateCurrentMusic(string track, GameObject source)
        {
            if (source == null) {
                currentMusic = track == string.Empty ? null : track;
            } else {
                if (currentMusicFromObjects.ContainsKey(source)) {
                    if (string.IsNullOrEmpty(track)) {
                        currentMusicFromObjects.Remove(source);
                    } else {
                        currentMusicFromObjects[source] = track;
                    }
                } else if(!string.IsNullOrEmpty(track)) {
                    currentMusicFromObjects.Add(source, track);
                }
            }
        }
    }
}
