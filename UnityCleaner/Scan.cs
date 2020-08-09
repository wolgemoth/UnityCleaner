using System.IO;
using System.Linq;
using System.Collections.Generic;

using System.Runtime.CompilerServices;

using System.Threading.Tasks;

namespace Cleaner {

    public struct ScanJob {

        private static List<string> s_Output = new List<string>();

        /// <summary>
        /// Runs multiple parallel scan threads to identify disposable files and directories contained within the root directory.
        /// </summary>
        /// <param name="_result">List of disposable files and directories.</param>
        /// <param name="_root">The root directory to be scanned.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScanProject(out List<string> _result, string _root) {

            CLI.DisplayText("    Scanning " + new DirectoryInfo(_root).Name + "... ");

            List<string> badDirectories = new List<string>();

            /* GET DISPOSABLE DIRECTORIES: */
            Parallel.For(0, DisposableItems.s_DisposableDirectories.Length, index => {
                string supposedDirectory = _root + DisposableItems.s_DisposableDirectories[index];

                if (Directory.Exists(supposedDirectory)) {
                    badDirectories.Add(supposedDirectory);

                    lock (s_Output) {
                        s_Output.Add(supposedDirectory);
                    }
                }
            });

            /* GET DISPOSABLE FILES: */

            // In _root folder...
            Parallel.For(0, DisposableItems.s_DisposableFiles.Length, index => {
                lock (s_Output) {
                    s_Output.AddRange(Directory.GetFiles(_root, DisposableItems.s_DisposableFiles[index], SearchOption.TopDirectoryOnly));
                }
            });

            // In subdirectories...
            var dirs = Directory.GetDirectories(_root).Where(dir => badDirectories.Contains(dir) != false);
            Parallel.ForEach(dirs, dir =>
                Parallel.For(0, DisposableItems.s_DisposableFiles.Length, index => {
                    lock (s_Output) {
                        s_Output.AddRange(Directory.GetFiles(dir, DisposableItems.s_DisposableFiles[index]));
                    }
                })
            );

            _result = s_Output;

            CLI.DisplayText("[DONE]\n");
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