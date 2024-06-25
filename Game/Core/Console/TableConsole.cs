using UnityEngine;
using GreenOne.Console;
using TMPro;
using GreenOne;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Статический класс, представляющий игровую консоль с логами и командами, отобразить которую можно нажатием клавиши <see cref="SWITCH_KEY"/>.
    /// </summary>
    public static class TableConsole
    {
        const KeyCode SWITCH_KEY = KeyCode.BackQuote;

        static GameObject _consoleObject;
        static TMP_InputField _inputTextMesh;
        static TextMeshPro _outputTextMesh;
        static List<string> _latestCommands = new();
        static int _commandIndex;
        static bool _isVisible;

        public static void Initialize()
        {
            Global.OnUpdate += OnUpdate;

            Commands.Add(new cmdCardStatAdd());
            Commands.Add(new cmdCardTraitAdd());
            Commands.Add(new cmdHelp());
            Commands.Add(new cmdLogTerritory());
            Commands.Add(new cmdSideCardAdd());
            Commands.Add(new cmdSideCardPlace());
            Commands.Add(new cmdSideStatAdd());

            _consoleObject = Global.Root.Find("CORE/Console").gameObject;
            _inputTextMesh = _consoleObject.Find<TMP_InputField>("Input text");
            _outputTextMesh = _consoleObject.Find<TextMeshPro>("Output text");
        }
        public static void WriteLine(string text, LogType type)
        {
            string str;
            if (type == LogType.Error || type == LogType.Exception)
                 str = $"<color=red>{text}</color>\n";
            else str = $"{text}\n";
            _outputTextMesh.text += str;
        }

        static void ExecuteLine(string line)
        {
            try { Command.ExecuteLine(line); }
            catch (ArgDuplicateException e) { WriteLine($"Указан дубликат аргумента {e.argId}.", LogType.Error); }
            catch (ArgValueException e) { WriteLine($"Указано неверное значение аргумента {e.argId}.", LogType.Error); }
            catch (ArgCountException) { WriteLine($"Указано неверное количество аргументов.", LogType.Error); }
            catch (ComplexArgException) { WriteLine($"Ошибка обработки аргумента как комплексного (с указанием \"\").", LogType.Error); }
            catch (NamedArgException) { WriteLine($"Ошибка обработки аргумента как именнованного (с указанием =).", LogType.Error); }

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
    }
}
