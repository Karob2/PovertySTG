using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace Engine
{
    public static class SoundManager
    {
        static GameServices gs;
        static string currentMusic = "";
        static float musicVolume;
        static float currentMusicVolume;
        static float soundVolume;

        static Dictionary<string, SoundEffectInstance> sounds;

        public static float MusicVolume { get => musicVolume; set { musicVolume = value; MediaPlayer.Volume = value * currentMusicVolume; } }
        public static float SoundVolume { get => soundVolume; set => soundVolume = value; }

        public static void Initialize(GameServices gs)
        {
            SoundManager.gs = gs;
            sounds = new Dictionary<string, SoundEffectInstance>();
        }

        public static void Initialize(GameServices gs, float musicVolume, float soundVolume)
        {
            SoundManager.gs = gs;
            sounds = new Dictionary<string, SoundEffectInstance>();
            MusicVolume = musicVolume;
            SoundVolume = soundVolume;
        }

        public static void PlayMusic(string name, float volume = 1f, bool repeat = true)
        {
            if (currentMusic == name) return;
            currentMusic = name;
            currentMusicVolume = volume;
            MediaPlayer.Stop();
            MediaPlayer.Volume = musicVolume * volume;
            MediaPlayer.IsRepeating = repeat;
            MediaPlayer.Play(gs.ResourceManager.Songs.Get(name));
        }

        public static void StopMusic()
        {
            currentMusic = "";
            MediaPlayer.Stop();
        }

        public static void PlaySound(string name, float volume = 1f, bool looped = false)
        {
            if (!sounds.TryGetValue(name, out SoundEffectInstance fx))
            {
                fx = gs.ResourceManager.SoundEffects.Get(name).CreateInstance();
                sounds.Add(name, fx);
            }
            fx.Stop();
            fx.Volume = soundVolume * volume;
            fx.IsLooped = looped;
            fx.Play();
        }

        public static void StopSound(string name)
        {
            if (!sounds.TryGetValue(name, out SoundEffectInstance fx)) return;
            fx.Stop();
        }

        public static void PlayLayeredSound(string name, float volume = 1f)
        {
            SoundEffect fx = gs.ResourceManager.SoundEffects.Get(name);
            fx.Play(soundVolume * volume, 0, 0);
        }
    }
}