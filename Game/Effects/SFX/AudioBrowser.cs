using MyBox;
using System.Collections.Generic;

namespace Game.Effects
{
    /// <summary>
    /// Статический класс, содержащий коллекции всех существующих игровых звуков и музыки в исходном состоянии.
    /// </summary>
    public static class AudioBrowser
    {
        public static IReadOnlyCollection<Music[]> MusicMixes => _musicMixes.Values;
        public static IReadOnlyCollection<Music> Music => _music.Values;
        public static IReadOnlyCollection<Sound> Sounds => _sounds.Values;

        static readonly Dictionary<string, Music[]> _musicMixes = new();
        static readonly Dictionary<string, Music> _music = new();
        static readonly Dictionary<string, Sound> _sounds = new();

        public static void Initialize() 
        {
            Music[] mainMenuMix = new Music[] { new mDevour() };
            AddMusicMix("main", mainMenuMix);

            Music[] battleMix = new Music[] { new mWhereTheWorldEnds(), new mForget(), new mEyesThroat() };
            AddMusicMix("battle", battleMix);

            Music[] peaceMix = new Music[] { new m9thDimension(), new mFloatingPoint() };
            AddMusicMix("peace", peaceMix);
        }
        public static void Shuffle()
        {
            string[] keys = new string[_musicMixes.Count];
            _musicMixes.Keys.CopyTo(keys, 0);
            foreach (string key in keys)
                _musicMixes[key] = (Music[])_musicMixes[key].Shuffle();
        }

        public static Music[] GetMusicMix(string id)
        {
            if (_musicMixes.TryGetValue(id, out Music[] mix))
                return mix;
            else throw new System.NullReferenceException($"Music with specified id was not found: {id}.");
        }
        public static Music GetMusic(string id)
        {
            if (_music.TryGetValue(id, out Music music))
                return music;
            else throw new System.NullReferenceException($"Music with specified id was not found: {id}.");
        }
        public static Sound GetSound(string id)
        {
            if (_sounds.TryGetValue(id, out Sound sound))
                return sound;
            else throw new System.NullReferenceException($"Sound with specified id was not found: {id}.");
        }

        static void AddMusicMix(string id, Music[] mix)
        {
            _musicMixes.Add(id, mix);
            foreach (Music music in mix)
                AddMusic(music);
        }
        static void AddMusic(Music music)
        {
            _music.Add(music.id, music);
        }
        static void AddSound(Sound sound)
        {
            _sounds.Add(sound.id, sound);
        }
    }
}