using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using System.Threading.Tasks;

namespace Cleaner {

    public struct SearchJob {

        private static List<string> s_Output = new List<string>();

        /// <summary>
        /// Runs multiple parallel search threads to identify Unity project folders contained within the root directory.
        /// </summary>
        /// <param name="_result">List of discovered Unity projects.</param>
        /// <param name="_paths">The folders to be searched.</param>
        public static void Search(out List<string> _result, params string[] _paths) {

            CLI.DisplayText("\nSCANNING...\n\nOutput:\n");

            PerformRecursive(_paths);

            _result = s_Output;
        }

        /// <summary>
        /// Recursively scans the directories located in the input array and adds found Unity projects to s_Output.
        /// </summary>
        /// <param name="_paths">Array of directories to be scanned recursively.</param>
        private static void PerformRecursive(params string[] _paths) {

            Parallel.For(0, _paths.Length, index => {

                string path = _paths[index];

                if (path.EndsWith(Path.DirectorySeparatorChar) == false) {
                    path += Path.DirectorySeparatorChar;
                }

                try {
                    if (IsUnityProject(path)) {
                        lock (s_Output) {
                            s_Output.Add(path);
                            CLI.DisplayText("    " + new DirectoryInfo(path).Name + "\n");
                        }
                    }
                    else {
                        PerformRecursive(Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly));
                    }
                }
                catch { }
            });
        }

        /// <summary>
        /// Determines if a folder is the root directory for a unity project by detecting if it contains an Assets folder with one or more meta files.
        /// </summary>
        /// <param name="_root">The root directory of the supposed Unity project.</param>
        /// <returns>True if the folder is a Unity project.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsUnityProject(string _root) {

            /*
             * Any Asset folders not containing at least one meta file will be ignored. Any Unity projects not containing an assets folder will be ignored.
             * While these projects are indeed Unity projects, they are not actually functional, so skipping them saves time and massively reduces the chance of false positives.
             */

            string assetsFolder = _root + "Assets";

            return Directory.Exists(assetsFolder) && Directory.GetFiles(assetsFolder, "*.meta", SearchOption.TopDirectoryOnly).Length != 0;
        }

        /// <summary>
        /// Releases stored memory associated with the scanning process.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clean() {
            s_Output.Clear();
            s_Output.TrimExcess();
        }
    }
}