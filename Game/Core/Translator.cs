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
                if (!_supportedLanguages.Contains(value))
                    throw new System.NotSupportedException("Chosen language is not supported.");
                _currentLanguage = value;
                SetDefaultLanguage();
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
                        TableConsole.Log($"Translation {id} args were passed in incorrect order, it can cause errors.", UnityEngine.LogType.Warning);
                }
            }
            public string Format(params object[] args)
            {
                if (argsCount != args.Length)
                    TableConsole.Log($"Translation {id} args count mismatch. Required: {argsCount}, Passed: {args.Length}.", UnityEngine.LogType.Warning);
                if (argsCount == 1)
                {
                    if (args.Length > 0)
                         return value.Replace("{0}", args[0].ToString());
                    else return value;
                }
                StringBuilder sb = new(value);
                for (int i = 0; i < argsCount; i++)
                    sb.Replace($"{{{i}}}", args[i].ToString());
                return sb.ToString();
            }
        }

        public static void Initialize()
        {
			if (_translations != null) return;
            _translations = new TranslatorCollection();
            Dictionary<string, LanguageCollection> languages = LoadLangCollectionsFromFolder() ?? new();
            foreach (KeyValuePair<string, LanguageCollection> pair in languages)
                _translations.Add(pair.Key, pair.Value);

            if (_translations.Count == 0 || !_translations.Keys.Contains(DEF_LANG_ID))
            {
                TextAsset defaultLanguage = Resources.Load<TextAsset>($"Localization/Languages/{DEF_LANG_ID}");
                LanguageCollection defaultCollection = CreateLangCollectionFromJson(defaultLanguage.text);
                _translations.Add(DEF_LANG_ID, defaultCollection);
                TableConsole.Log("Некоторые файлы локализации не обнаружены, будет загружена локализация по умолчанию.", LogType.Warning);
            }

            _supportedLanguages = _translations.Keys.ToArray();
            _currentLanguage = GetDefaultLanguage();
        }
        public static string GetString(string id, params object[] args)
        {
            if (_translations == null)
                throw new Exception("Translator is not initialized.");
            LanguageCollection langWords = _translations[CurrentLanguage];
            if (langWords.TryGetValue(id, out LanguageString langStr) && !string.IsNullOrWhiteSpace(langStr.value))
                 return langStr.Format(args);
            else
            {
                TableConsole.Log($"Перевод не найден, будет использоваться вариант по умолчанию. ID: {id}", LogType.Warning);
                return _translations[DEF_LANG_ID][id].Format(args);
            }
        }

        private static Dictionary<string, LanguageCollection> LoadLangCollectionsFromFolder()
        {
            const string FOLDER = "Localization";
            if (!Directory.Exists(FOLDER))
                return null;
            Dictionary<string, LanguageCollection> languages = new();
            foreach (string file in Directory.EnumerateFiles(FOLDER))
            {
                if (file.EndsWith("selected.txt")) continue;
                try
                {
                    string languageName = Path.GetFileNameWithoutExtension(file).ToLower();
                    string languageJson = File.ReadAllText(file);
                    LanguageCollection languageCollection = CreateLangCollectionFromJson(languageJson);
                    languages.Add(languageName, languageCollection);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
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

        private static string GetDefaultLanguage()
        {
            if (!File.Exists("Localization/selected.txt"))
                return DEF_LANG_ID;
            string str = File.ReadAllText("Localization/selected.txt");
            if (_supportedLanguages.Contains(str))
                return str;
            else return DEF_LANG_ID;
        }
        private static void SetDefaultLanguage()
        {
            if (!Directory.Exists("Localization"))
                Directory.CreateDirectory("Localization");
            File.WriteAllText("Localization/selected.txt", _currentLanguage);
        }
    }
}
