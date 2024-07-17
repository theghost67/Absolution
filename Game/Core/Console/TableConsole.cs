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
        public static string FilePath => _filePath;
        public static event Func<string, bool> OnLogToFile; // bool = handled

        static readonly List<string> _latestCommands = new();
        static readonly Queue<string> _fileQueuedLogs = new();
        static string _filePath;
        static StreamWriter _fileStream;

        static GameObject _consoleObject;
        static TMP_InputField _inputTextMesh;
        static TextMeshPro _outputTextMesh;
        static int _commandIndex;
        static bool _isVisible;

        public static void Initialize()
        {
            Commands.Add(new cmdCardStatAdd());
            Commands.Add(new cmdCardTraitAdd());
            Commands.Add(new cmdData());
            Commands.Add(new cmdDeckCardAdd());
            Commands.Add(new cmdHelp());
            Commands.Add(new cmdSideCardAdd());
            Commands.Add(new cmdSideCardPlace());
            Commands.Add(new cmdSideStatAdd());
            Commands.Add(new cmdSkip());

            _filePath = Application.persistentDataPath + "/console.log";
            _fileStream = new(_filePath);
            _fileStream.WriteLine($"PATH SHORTCUT: {_filePath}\n");
            _fileStream.Flush();

            _consoleObject = Global.Root.Find("CORE/Console").gameObject;
            _inputTextMesh = _consoleObject.Find<TMP_InputField>("Input text");
            _outputTextMesh = _consoleObject.Find<TextMeshPro>("Output text");

            Global.OnUpdate += OnUpdate;
            Application.quitting += _fileStream.Close;
            Task.Run(LogToFileQueue);
        }

        public static void LogToFile(string module, string text)
        {
            text = $"[{module}] {text}";
            if (OnLogToFile?.Invoke(text) ?? !Global.writeConsoleLogs) return;
            _fileQueuedLogs.Enqueue(text);
        }
        public static void LogToFile(string module, IReadOnlyList<string> texts)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                string text = $"[{module}] {texts[i]}";
                if (OnLogToFile?.Invoke(text) ?? !Global.writeConsoleLogs) continue;
                _fileQueuedLogs.Enqueue(text);
            }
        }
        public static void Log(string text, LogType type)
        {
            string str;
            if (type == LogType.Error || type == LogType.Exception)
                 str = $"<color=red>{text}</color>\n";
            else str = $"{text}\n";
            _outputTextMesh.text += str;
        }

        static void ExecuteLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            try { Command.ExecuteLine(line); }
            catch (ArgDuplicateException e) { Log($"Указан дубликат аргумента {e.argId}.", LogType.Error); }
            catch (ArgValueException e) { Log($"Указано неверное значение аргумента {e.argId}.", LogType.Error); }
            catch (ArgCountException) { Log($"Указано неверное количество аргументов.", LogType.Error); }
            catch (ComplexArgException) { Log($"Ошибка обработки аргумента как комплексного (с указанием \"\").", LogType.Error); }
            catch (NamedArgException) { Log($"Ошибка обработки аргумента как именнованного (с указанием =).", LogType.Error); }

            _latestCommands.Add(line);
            _commandIndex = _latestCommands.Count;

            if (!_isVisible) return;
            _inputTextMesh.text = null;
            _inputTextMesh.ActivateInputField();
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

        static void LogToFileQueue()
        {
            Start:
            if (_fileQueuedLogs.Count == 0) goto End;
            if (!_fileStream.BaseStream.CanWrite) goto End;
            try
            {
                while (_fileQueuedLogs.Count != 0)
                {
                    string text;
                    lock (_fileQueuedLogs) { text = _fileQueuedLogs.Dequeue(); }
                    _fileStream.WriteLine(text);
                }
                _fileStream.Flush();
            }
            catch (Exception ex) 
            { 
                Debug.LogException(ex);
                Debug.LogError("Log stream has been disposed.");
                _fileStream.Dispose();
                _fileStream = null;
                return;
            }

            End:
            Thread.Sleep(1000);
            goto Start;
        }
    }
}
