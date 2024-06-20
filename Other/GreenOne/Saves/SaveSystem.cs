using Game.Cards;
using Game;
using Game.Environment;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace GreenOne
{
    /// <summary>
    /// Статический класс, представляющий систему сохранений данных.
    /// </summary>
    [Obsolete] public static class SaveSystem
    {
        public static string saveSlotFolder = string.Empty;
        const string ENCRYPTION_KEY = "StkmAdv";

        // should be implemented differently in each project
        public static void SaveAll()
        {
            return; // TODO: implement as JSON
            Player.Save();
            //Location.Save();
            World.Save();

            double time = World.PlayTime + Time.realtimeSinceStartup;
            CardDeck deck = Player.Deck;
            SerializationDict analyticsDict = new()
            {
                { "play_version", Application.version },
                { "play_time_hours", time / 3600 },
                { "player_travels_finished", Player.TravelsFinished },
                { "player_travels_failed", Player.TravelsFailed },
                { "player_deck_count", deck.Count },
                { "player_deck_threat", deck.Points },
                { "player_deck_traits", deck.Traits },
            };
            SaveDict(analyticsDict, "analytics", useCryptography: false);
        }
        public static void LoadAll()
        {
            return; // TODO: implement as JSON
            Player.Load();
            //Location.Load();
            World.Load();
        }

        // typeof(T) must be serializable
        public static void Save<T>(T data, string path, bool useCryptography = true)
        {
            path = GetFullFilePath(path);
            FileCreateWithDirs(path);

            using var stream = new StreamWriter(path);
            string inputString = JsonConvert.SerializeObject(data, Formatting.Indented);
            if (useCryptography)
                inputString = Cryptography.Encrypt(inputString, ENCRYPTION_KEY);

            stream.Write(inputString);
        }
        public static T Load<T>(string path, bool useCryptography = true)
        {
            path = GetFullFilePath(path);
            if (!File.Exists(path)) return default;

            using var stream = new StreamReader(path);
            var outputString = stream.ReadToEnd();
            if (useCryptography)
                outputString = Cryptography.Decrypt(outputString, ENCRYPTION_KEY);

            return JsonConvert.DeserializeObject<T>(outputString);
        }

        public static void SaveDict(SerializationDict dict, string path, bool useCryptography = true)
        {
            Save(dict, path, useCryptography);
        }
        public static SerializationDict LoadDict(string path, bool useCryptography = true)
        {
            return Load<SerializationDict>(path, useCryptography);
        }

        static string GetFullFilePath(string path)
        {
            if (string.IsNullOrEmpty(saveSlotFolder))
                 return $"{Application.persistentDataPath}/{path}.dat";
            else return $"{Application.persistentDataPath}/{saveSlotFolder}/{path}.dat";
        }
        static void FileCreateWithDirs(string path)
        {
            if (!File.Exists(path))
            {
                var dirPath = path[..(path.LastIndexOf('/') + 1)];
                Directory.CreateDirectory(dirPath);

                var fileStream = File.Create(path);
                    fileStream.Close();
                    fileStream.Dispose();
            }
        }
    }
}
