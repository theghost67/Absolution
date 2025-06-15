using Game.Console;
using GreenOne;
using GreenOne.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Статический класс, представляющий игровую консоль с логами и командами, отобразить которую можно нажатием клавиши <see cref="SWITCH_KEY"/>.
    /// </summary>
    public static class TableConsole
    {
        public const KeyCode SWITCH_KEY = KeyCode.BackQuote;
        public static string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                _filePath = Path.Combine(_persistentPath, _fileName);
                _fileStream?.Dispose();
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                _fileStream = new(_filePath);
            }
        }
        public static string FilePath => _filePath;
        public static event Func<string, bool> OnLogToFile; // bool = handled
        public static bool IsVisible => _isVisible;

        static readonly List<string> _latestCommands = new();
        static readonly string _persistentPath = Application.persistentDataPath;
        static string _filePath;
        static string _fileName;
        static StreamWriter _fileStream;

        static GameObject _consoleObject;
        static TMP_InputField _inputTextMesh;
        static TextMeshPro _outputTextMesh;
        static int _commandIndex;
        static bool _isVisible;

        public static void Initialize()
        {
            Commands.Add(new cmdAiMove());
            Commands.Add(new cmdAiTest());
            Commands.Add(new cmdCardGod());
            Commands.Add(new cmdCardStat());
            Commands.Add(new cmdCardTrait());
            Commands.Add(new cmdData());
            Commands.Add(new cmdDeckCard());
            Commands.Add(new cmdDeckGen());
            Commands.Add(new cmdDeckTest());
            Commands.Add(new cmdHelp());
            Commands.Add(new cmdMove());
            Commands.Add(new cmdPointsTest());
            Commands.Add(new cmdRestart());
            Commands.Add(new cmdSideCard());
            Commands.Add(new cmdSideCardPlace());
            Commands.Add(new cmdSideStat());
            Commands.Add(new cmdSkip());
            Commands.Add(new cmdSkipMusic());

            FileName = "Console.log";

            _consoleObject = Global.Root.Find("CORE/Console").gameObject;
            _inputTextMesh = _consoleObject.Find<TMP_InputField>("Input text");
            _outputTextMesh = _consoleObject.Find<TextMeshPro>("Output text");

            Global.OnUpdate += OnUpdate;
            Application.quitting += _fileStream.Close;
        }
        public static void ExecuteLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            try { Command.ExecuteLine(line); }
            catch (ArgDuplicateException e) { Log($"Указан дубликат аргумента {e.argId}.", LogType.Error); }
            catch (ArgValueException e) { Log($"Указано неверное значение аргумента {e.argId}.", LogType.Error); }
            catch (ArgCountException) { Log($"Указано неверное количество аргументов.", LogType.Error); }
            catch (ComplexArgException) { Log($"Ошибка обработки аргумента как комплексного (с указанием \"\").", LogType.Error); }
            catch (NamedArgException) { Log($"Ошибка обработки аргумента как именнованного (с указанием =).", LogType.Error); }

            LogToFile("console", line);
            _latestCommands.Add(line);
            _commandIndex = _latestCommands.Count;

            if (!_isVisible) return;
            _inputTextMesh.text = null;
            _inputTextMesh.ActivateInputField();
        }

        public static void LogToFile(string module, string text)
        {
            text = $"[{module}] {text}";
            if (OnLogToFile?.Invoke(text) ?? false) return;
            _fileStream?.WriteLine(text);
        }
        public static void LogToFile(string module, IReadOnlyList<string> texts)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                string text = $"[{module}] {texts[i]}";
                if (OnLogToFile?.Invoke(text) ?? false) continue;
                _fileStream?.WriteLine(text);
            }
        }
        public static void Log(string text, LogType type)
        {
            string str;
            if (type == LogType.Error || type == LogType.Exception)
                 str = $"<color=red>{text}</color>\n";
            else str = $"{text}\n";
            UnityMainThreadDispatcher.Enqueue(() => _outputTextMesh.text += str);
        }

        static void OnUpdate() 
        {
            if (_isVisible)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                    ExecuteLine(_inputTextMesh.text);
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    OnUpArrowPressed();
                if (Input.GetKeyDown(KeyCode.DownArrow))
                    OnDownArrowPressed();
            }

            if (!Input.GetKeyDown(SWITCH_KEY)) return;
            _isVisible = !_isVisible;
            _consoleObject.SetActive(_isVisible);
            if (_isVisible)
            {
                _inputTextMesh.text = "";
                _inputTextMesh.ActivateInputField();
                _commandIndex = _latestCommands.Count;
            }
        }
        static void OnUpArrowPressed()
        {
            if (_commandIndex > 0) // selects previous command
            {
                _commandIndex--;
                _inputTextMesh.text = _latestCommands[_commandIndex];
            }
            _inputTextMesh.MoveToEndOfLine(false, false);
        }
        static void OnDownArrowPressed()
        {
            if (_commandIndex < _latestCommands.Count) // selects next command
            {
                _commandIndex++;
                _inputTextMesh.text = _commandIndex == _latestCommands.Count ? "" : _latestCommands[_commandIndex];
            }
            _inputTextMesh.MoveToEndOfLine(false, false);
        }
    }
}
