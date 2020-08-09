using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Cleaner {

    public static class CLI {

        /// <summary>
        /// Prompts the user for input and returns the index of the command associated with it. -1 if no associated command exists.
        /// </summary>
        /// <param name="_commands">The commands to compare the input against.</param>
        /// <returns>Index of command associated with input. -1 if no associated command exists.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetInput(params string[] _commands) {

            Console.Write("> ");

            int result;
            string input = Console.ReadLine();

            if (input != null && input != string.Empty) {

                input = input.ToLower().Trim();

                for (result = 0; result < _commands.Length; result++) {

                    string syntax = _commands[result].ToLower().Trim();

                    if (input == syntax) {
                        break;
                    }
                    else if (result == _commands.Length - 1) {
                        result = -1;

                        break;
                    }
                }
            }
            else {
                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Prompts the user for a single line of input.
        /// </summary>
        /// <returns>The user input as a string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetLine() {
            Console.Write("> ");

            return Console.ReadLine(); ;
        }

        /// <summary>
        /// Prompts the user for input and attempts to return it as a formatted directory path.
        /// </summary>
        /// <param name="_output">Formatted directory taken from user input.</param>
        /// <returns>True if the user input is a valid directory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetDirectory(ref string _output) {

            bool result;

            string root = GetLine().Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            string[] split = root.Split(Path.VolumeSeparatorChar);

            if (split.Length != 0) {
                split[0] = split[0].ToLower();

                string joined = string.Join(Path.VolumeSeparatorChar, split);

                if (result = Directory.Exists(joined)) {
                    _output = joined.TrimEnd(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                }
            }
            else {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Prompts the user for a yes / no answer.
        /// </summary>
        /// <param name="_message">Prompt message to be displayed.</param>
        /// <returns>True if the user responds with a word beginning with y.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Prompt(string _message) {
            DisplayText(_message + "\n> ");

            int result = -1;
            string input = Console.ReadLine().Trim()[0].ToString();

            while (result == -1) {
                switch (input) {
                    case "y": { result = 1; break; }
                    default : { result = 0; break; }
                }
            }

            return result == 1;
        }

        /// <summary>
        /// Inserts a blank line in the output window.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Break() {
            Console.Write("\n");
        }

        /// <summary>
        /// Prints all input strings to the output window.
        /// </summary>
        /// <param name="_input">Strings to be displayed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisplayText(params string[] _input) {
            for (int i = 0; i < _input.Length; i++) {
                Console.Write(_input[i]);
            }
        }

    }
}