using Engine;
using Engine.Input;
using Engine.Util;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace PovertySTG
{
    public static class Config
    {
        static GameServices gs;
        const string configVersion = "b";
        static string path;
        //public static float MusicVolume { get; set; }
        //public static float SoundVolume { get; set; }
        public static bool Fullscreen { get; set; }
        public static int Zoom { get; set; }
        public static float LevelWidth => 1080;
        public static float LevelHeight => 1080;
        public static List<float> GameScales => new List<float> { 600f / 1080f, 720f / 1080f, 1};

        static Dictionary<GameCommand, string> commandName;

        public static void Initialize(GameServices gs, string path)
        {
            Config.gs = gs;
            Config.path = path;

            commandName = new Dictionary<GameCommand, string>();
            //commandName.Add(GameCommand.Action1, "Attack");
            //commandName.Add(GameCommand.Action2, "Bomb");
            //commandName.Add(GameCommand.Action3, "Talk");

#if !DEBUG
            LoadConfig();
#endif
        }

        public static string GetCommandName(GameCommand command)
        {
            if (commandName.TryGetValue(command, out string name)) return name;
            return ((GameCommand)command).ToString();
        }

        public static void Default()
        {
            //SoundManager.MusicVolume = 0.75f;
            //SoundManager.SoundVolume = 0.75f;
            //Fullscreen = false;
            //Zoom = 3;
        }

        public static void LoadConfig()
        {
            // Create default bindings file if no file exists.
            if (!File.Exists(path))
            {
                Default();
                SaveConfig();
                return;
            }

            ConfigOptions options = JsonHelper<ConfigOptions>.Load(path);
            if (options.Version != configVersion)
            {
                Default();
                SaveConfig();
                return;
            }

            SoundManager.MusicVolume = options.MusicVolume;
            SoundManager.SoundVolume = options.SoundVolume;
            Config.Fullscreen = options.Fullscreen;
            gs.DisplayManager.SetFullscreen(Config.Fullscreen);
            Config.Zoom = options.Zoom;
            gs.DisplayManager.SetZoom(Config.GameScales[Config.Zoom]);
        }

        public static void SaveConfig()
        {
            ConfigOptions options = new ConfigOptions();
            options.Version = configVersion;
            options.MusicVolume = SoundManager.MusicVolume;
            options.SoundVolume = SoundManager.SoundVolume;
            options.Fullscreen = Config.Fullscreen;
            options.Zoom = Config.Zoom;
            JsonHelper<ConfigOptions>.Save(path, options);
        }
    }

    [JsonObject]
    public class ConfigOptions
    {
        [JsonProperty]
        public string Version { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0.75f)]
        public float MusicVolume { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0.50f)]
        public float SoundVolume { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(false)]
        public bool Fullscreen { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(3)]
        public int Zoom { get; set; }
    }
}