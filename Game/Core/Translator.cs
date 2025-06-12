using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Представляет систему перевода, реализованную через загрузку данных из файла формата XLSX.
    /// </summary>
    public static class Translator
    {
		private const string DEF_LANG_ID = "rus";
		
        public static string[] SupportedLanguages => _supportedLanguages;
        public static string CurrentLanguage 
        {
            get => _currentLanguage;
            set
            {
                if (_supportedLanguages.Contains(value))
                    _currentLanguage = value;
                else throw new System.NotSupportedException("Chosen language is not supported.");
            }
        }

        private static string[] _supportedLanguages;
        private static string _currentLanguage;
        private static TranslatorCollection _translations;

        private class TranslatorCollection : Dictionary<string, LanguageCollection> { }
        private class LanguageCollection : Dictionary<string, LanguageString> { }
        private class LanguageString
        {
            public readonly string id;
            public readonly string value;
            public readonly int argsCount;

            public LanguageString(string id, string value)
            {
                this.id = id;
                this.value = value;
                List<int> indecies = new();
                for (int i = 0; i < 10; i++)
                {
                    if (!value.Contains($"{{{i}}}")) continue;
                    indecies.Add(i);
                    argsCount++;
                }
                for (int i = 0; i < indecies.Count; i++)
                {
                    if (indecies[i] != i)
                        TableConsole.Log($"Translation {id} args format error. Args were passed in incorrect order, it can cause errors.", UnityEngine.LogType.Error);
                }
            }
            public string Format(params object[] args)
            {
                if (argsCount != args.Length)
                    TableConsole.Log($"Translation {id} args count mismatch. Required: {argsCount}, Passed: {args.Length}.", UnityEngine.LogType.Warning);
                if (argsCount == 1)
                    return value.Replace("{0}", args[0].ToString());
                StringBuilder sb = new();
                for (int i = 0; i < argsCount; i++)
                    sb.Replace($"{{{i}}}", args[i].ToString());
                return sb.ToString();
            }
        }

        public static void Initialize()
        {
			if (_translations != null) return;
            _translations = new TranslatorCollection();
            Dictionary<string, LanguageCollection> languages = LoadLangCollectionsFromFolder("Localization") ?? new();
            foreach (KeyValuePair<string, LanguageCollection> pair in languages)
                _translations.Add(pair.Key, pair.Value);

            if (_translations.Count == 0 || !_translations.Keys.Contains(DEF_LANG_ID))
            {
                TextAsset defaultLanguage = Resources.Load<TextAsset>($"Localization/{DEF_LANG_ID}");
                LanguageCollection defaultCollection = CreateLangCollectionFromJson(defaultLanguage.text);
                _translations.Add(DEF_LANG_ID, defaultCollection);
                TableConsole.Log($"Некоторые файлы локализации не обнаружены, будет загружена локализация по умолчанию.", LogType.Warning);
            }

            _supportedLanguages = _translations.Keys.ToArray();
            _currentLanguage = DEF_LANG_ID;
        }
        public static string GetString(string id, params object[] args)
        {
            if (_translations == null)
                throw new Exception("Translator is not initialized.");
            LanguageCollection langWords = _translations[CurrentLanguage];
            if (langWords.TryGetValue(id, out LanguageString langStr) && !string.IsNullOrWhiteSpace(langStr.value))
                 return langStr.Format(args);
            else return _translations[DEF_LANG_ID][id].Format(args);
        }

        private static Dictionary<string, LanguageCollection> LoadLangCollectionsFromFolder(string folder)
        {
            if (!Directory.Exists(folder))
                return null;
            Dictionary<string, LanguageCollection> languages = new();
            foreach (string file in Directory.EnumerateFiles(folder))
            {
                try
                {
                    string languageName = Path.GetFileNameWithoutExtension(file).ToLower();
                    string languageJson = File.ReadAllText(file);
                    LanguageCollection languageCollection = CreateLangCollectionFromJson(languageJson);
                    languages.Add(languageName, languageCollection);
                }
                catch
                {
                    TableConsole.Log($"Не удалось обработать файл локализации {file}, он будет проигнорирован.", LogType.Warning);
                }
            }
            return languages;
        }
        private static LanguageCollection CreateLangCollectionFromJson(string json)
        {
            Dictionary<string, string> languageStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            LanguageCollection languageCollection = new();
            foreach (KeyValuePair<string, string> pair in languageStrings)
                languageCollection.Add(pair.Key, new LanguageString(pair.Key, pair.Value));
            return languageCollection;
        }
    }
}
