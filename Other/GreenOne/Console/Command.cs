using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GreenOne.Console
{
    /// <summary>
    /// Абстрактный класс, представляющий команду терминала.
    /// </summary>
    public abstract class Command : IEquatable<Command>
    {
        // use lower strings
        public readonly string id;
        public readonly string desc;

        // do NOT add duplicate arguments (with the same id or type)
        public readonly CommandArg[] arguments;
        public readonly string[] pseudonims;
        public readonly int requiredArgsCount;

        public Command(string id, string desc)
        {
            this.id = id;
            this.desc = desc;

            arguments = ArgumentsCreator();
            pseudonims = PseudonimsCreator();

            bool prevIsRequired = true;
            foreach (CommandArg arg in arguments)
            {
                bool isRequired = arg.isRequired;
                if (isRequired)
                {
                    if (!prevIsRequired)
                        throw new ArgumentException("Command arguments with non-required values must be in the end of arguments list. Do not mix required and not required arguments.");
                    requiredArgsCount++;
                }
                prevIsRequired = isRequired;
            }
        }

        public static bool TryParse(string str, out Command? cmd)
        {
            foreach (var command in CommandList.Set.Values)
            {
                if (command.IsPseudonim(str))
                {
                    cmd = command;
                    return true;
                }
            }

            cmd = null;
            return false;
        }
        /// <exception cref="ComplexArgException"></exception>
        public static void SplitAsArgs(string line, out string[] args)
        {
            // this method does not support recursive commands/arguments
            int argStartIndex = -1;
            int lineLength = line.Length;
            List<string> argsSet = new();
            bool isComplexValue = false;
            int prefixStartIndex = -1;

            for (int i = 0; i < lineLength; i++)
            {
                if (line[i] == '\"')
                {
                    isComplexValue = !isComplexValue;
                    if (isComplexValue)
                    {
                        prefixStartIndex = (i == 0 || line[i - 1] == ' ') ? -1 : line.LastIndexOf(' ', i - 1);
                        if (prefixStartIndex == -1) // used for named arguments
                            argStartIndex = i;
                        else argStartIndex = prefixStartIndex;
                    }
                    else
                    {
                        int endIndex = prefixStartIndex == -1 ? i : i + 1;  // include apostrophe if has prefix
                        argsSet.Add(line[(argStartIndex + 1)..endIndex]);
                    }
                }
                else if (!isComplexValue && line[i] == ' ')
                {
                    if (i == 0 || line[i - 1] != '\"')
                        argsSet.Add(line[(argStartIndex + 1)..i]); // do not include space

                    argStartIndex = i;
                }
            }

            if (isComplexValue)
                throw new ComplexArgException("An odd number of \" characters were specified.");
            else if (!line.EndsWith('\"'))
                argsSet.Add(line[(argStartIndex + 1)..]);

            args = argsSet.ToArray();
        }

        /// <exception cref="ArgDuplicateException"></exception>
        /// <exception cref="ArgValueException"></exception>
        /// <exception cref="ArgCountException"></exception>
        /// <exception cref="ComplexArgException"></exception>
        /// <exception cref="NamedArgException"></exception>
        public static void ExecuteLine(string line)
        {
            line = line.Trim(' ');
            bool hasArgs = line.Contains(' ');
            string commandId = hasArgs ? line[..line.IndexOf(' ')] : line;

            if (!TryParse(commandId, out var command))
                throw new ArgValueException("command", "Command was not found.");

            SplitAsArgs(line, out var argsInput);
            Execute(command!, argsInput[1..]); // first arg is command
        }
        /// <exception cref="ArgDuplicateException"></exception>
        /// <exception cref="ArgValueException"></exception>
        /// <exception cref="ArgCountException"></exception>
        /// <exception cref="NamedArgException"></exception>
        public static void Execute(Command command, string[] args)
        {
            var inputDict = new Dictionary<string, CommandArgInput>();
            var readOnlyInputDict = new CommandArgInputDict(inputDict);

            var argsByInput = GetArgsByInput(command, args);
            var argsByDefault = GetArgsByDefault(command, readOnlyInputDict);

            foreach (KeyValuePair<string, CommandArgInput> pair in argsByInput)
            {
                if (!inputDict.ContainsKey(pair.Key))
                    inputDict.Add(pair.Key, pair.Value);
                else throw new ArgDuplicateException(pair.Key, "Argument duplicate value was passed.");
            }

            foreach (KeyValuePair<string, CommandArgInput> pair in argsByDefault)
                inputDict.Add(pair.Key, pair.Value);

            foreach (CommandArgInput argInput in inputDict.Values)
            {
                if (!argInput.isValid)
                    throw new ArgValueException(argInput.argRef.id, "Argument value is invalid.");
            }

            command.Execute(readOnlyInputDict);
        }

        protected abstract void Execute(CommandArgInputDict args);
        protected virtual CommandArg[] ArgumentsCreator() => Array.Empty<CommandArg>();
        protected virtual string[] PseudonimsCreator() => Array.Empty<string>();

        public bool IsPseudonim(string str)
        {
            return id == str || pseudonims.Any(ps => ps == str);
        }

        public override string ToString()
        {
            StringBuilder sb = new(id);
            foreach (CommandArg arg in arguments)
                sb.Append($" {arg}");
            return sb.Append($": {desc}").ToString();
        }
        public string ToFullString()
        {
            string str = ToString();
            string smallSpace = new(' ', id.Length + (arguments.Length == 0 ? 2 : 1)); // empty space (+ colon)

            StringBuilder sb = new($"{str}\n{smallSpace}Псевдонимы: ");
            sb.Append(pseudonims[0]);

            int pseudoLength = pseudonims.Length;
            for (int i = 1; i < pseudoLength; i++)
                sb.Append($", {pseudonims[i]}");

            if (arguments.Length != 0)
                sb.AppendLine();

            foreach (CommandArg arg in arguments)
                sb.Append($"\n{arg.ToFullString()}");

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as Command);
        }
        public bool Equals(Command? other)
        {
            if (other is null)
                 return this is null;
            else return id == other.id;
        }

        /// <exception cref="NamedArgException"></exception>
        /// <exception cref="ArgCountException"></exception>
        static KeyValuePair<string, CommandArgInput>[] GetArgsByInput(Command cmd, string[] argsInput)
        {
            var array = new KeyValuePair<string, CommandArgInput>[argsInput.Length];
            int argsInputLength = argsInput.Length;

            if (argsInputLength < cmd.requiredArgsCount)
                throw new ArgCountException(true, "Not enough arguments were passed.");

            static void ProcessArgAsNamed(string argInput, CommandArg arg, out CommandArgInput processedInput)
            {
                int equalCharIndex = argInput.Length; // right after '='
                bool namedArgHasValue = argInput.Length > equalCharIndex + 1 && argInput[equalCharIndex] == '=';
                if (!namedArgHasValue)
                {
                    processedInput = new CommandArgInput(arg, string.Empty);
                    return;
                }

                int argValueIndex = equalCharIndex + 1;
                string namedValue;

                if (argInput[argValueIndex] == '\"')
                    namedValue = argInput[argValueIndex..].Trim('\"');
                else namedValue = argInput[argValueIndex..];

                processedInput = new CommandArgInput(arg, namedValue);
            }

            for (int i = 0; i < argsInputLength; i++)
            {
                string argInput = argsInput[i];
                bool hasPrefix = argInput.Contains('=');
                string[] split = hasPrefix ? argInput.Split('=') : Array.Empty<string>();
                string prefix = hasPrefix ? split[0].ToLower() : string.Empty;

                foreach (CommandArg namedArg in cmd.arguments)
                {
                    if (namedArg.id != prefix)
                        continue;

                    ProcessArgAsNamed(argInput, namedArg, out var namedInput);
                    array[i] = new KeyValuePair<string, CommandArgInput>(namedArg.id, namedInput);
                    goto Continue;
                }

                if (hasPrefix)
                    throw new NamedArgException("Named argument id was not found.");
                if (i >= cmd.arguments.Length)
                    throw new ArgCountException(false, "Too many arguments were passed.");

                CommandArg arg = cmd.arguments[i];
                CommandArgInput orderInput = new(arg, argInput);
                array[i] = new KeyValuePair<string, CommandArgInput>(arg.id, orderInput);
                Continue: continue;
            }
            return array;
        }
        static KeyValuePair<string, CommandArgInput>[] GetArgsByDefault(Command cmd, CommandArgInputDict argsInputDict)
        {
            int currentIndex = 0;
            int startIndex = cmd.requiredArgsCount;
            int count = cmd.arguments.Length;
            int maxDefaultArgs = count - startIndex; // also counts flags
            if (maxDefaultArgs == 0)
                return Array.Empty<KeyValuePair<string, CommandArgInput>>();

            var set = new HashSet<KeyValuePair<string, CommandArgInput>>(maxDefaultArgs);
            foreach (CommandArg arg in cmd.arguments)
            {
                if (currentIndex < startIndex) continue;
                if (!arg.isDefault || argsInputDict.ContainsKey(arg.id))
                    continue;

                CommandArgInput defaultInput = new(arg, arg.Default.value);
                set.Add(new KeyValuePair<string, CommandArgInput>(arg.id, defaultInput));
            }

            return set.ToArray();
        }
    }
}