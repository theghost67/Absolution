using System.Collections.Generic;

namespace Game.Effects
{
    /// <summary>
    /// Статический класс, содержащий коллекции всех существующих игровых звуков и музыки в исходном состоянии.
    /// </summary>
    public static class AudioBrowser
    {
        public static IReadOnlyCollection<Music> Music => _music.Values;
        public static IReadOnlyCollection<Sound> Sounds => _sounds.Values;

        static readonly Dictionary<string, Music> _music = new();
        static readonly Dictionary<string, Sound> _sounds = new();

        public static void Initialize() 
        {
            AddMusic(new mMenuCardChoose());
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