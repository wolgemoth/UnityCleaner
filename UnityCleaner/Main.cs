using System;
using System.IO;
using System.Collections.Generic;

using System.Threading.Tasks;

using System.Runtime.CompilerServices;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;

namespace Cleaner {

    struct DisposableItems { 
    
        public static readonly string[] s_DisposableDirectories = {
                ".consulo\\",
                ".gradle\\",
                ".vs\\",
                "Assets\\Plugins\\Editor\\JetBrains\\",
                "Library\\",
                "Temp\\",
                "Obj\\",
                "Build\\",
                "Builds\\",
                "Logs\\",
                "UserSettings\\",
                "ExportedObj\\",
            };

        public static readonly string[] s_DisposableFiles = {
                "*.csproj",
                "*.unityproj",
                "*.sln",
                "*.suo",
                "*.tmp",
                "*.user",
                "*.userprefs",
                "*.pidb",
                "*.booproj",
                "*.svd",
                "*.pdb",
                "*.mdb",
                "*.opendb",
                "*.VC.db",
                "*.pidb.meta",
                "*.pdb.meta",
                "*.mdb.meta",
                "sysinfo.txt",
                "*.apk",
                "*.unitypackage",
                "crashlytics-build.properties",
            };

    }

    class Application {

        static void Main(string[] args) {

            bool exitFlag = false;

            string[] commands = {
                "scan\n",
                "credits\n",
                "help\n",
                "exit\n"
            };

            while (!exitFlag) {
                switch (CLI.GetInput(commands)) {
                    case 0:  { Scan();                                           break; }
                    case 1:  { CLI.DisplayText("Copyright © Loui Eriksson.\n");  break; }
                    case 2:  { CLI.DisplayText(commands);                        break; }
                    case 3:  { exitFlag = true;                                  break; }

                    default: { CLI.DisplayText("Command not recognised.\n");     break; }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Scan() {

            CLI.DisplayText("\nEnter the path to scan:\n");

            string root = string.Empty;

            if (CLI.TryGetDirectory(ref root)) {

                SearchJob.Search(out List<string> unityProjects, root);

                if (unityProjects.Count != 0) {

                    List<string> disposableItems = new List<string>();

                    /* SCAN ALL PROJECTS */
                    CLI.DisplayText("\nScanning Projects...\n");
                    for (int i = 0; i < unityProjects.Count; i++) {
                        ScanJob.ScanProject(out disposableItems, unityProjects[i]);
                    }

                    /* DISPLAY ALL DISPOSABLE ITEMS FOUND */
                    CLI.DisplayText("\nItems Found:\n");
                    for (int i = 0; i < disposableItems.Count; i++) {
                        CLI.DisplayText("    " + disposableItems[i] + "\n");
                    }
                    CLI.DisplayText("\n    " + disposableItems.Count.ToString() + " items.\n");

                    /* DELETE ALL DISPOSABLE ITEMS */
                    if (disposableItems.Count != 0 && CLI.Prompt("\nWould you like to delete these items?")) {
                        Delete(disposableItems.ToArray());

                        if (CLI.Prompt("\nWould you like to archive the cleaned projects?")) {
                            Archive(unityProjects.ToArray());
                        }
                    }
                }
                else {
                    CLI.DisplayText("\nNo projects were found in the directory. Cancelling.\n");
                }
            }
            else {
                CLI.DisplayText("\nDirectory could not be found. Cancelling.\n");
            }

            CLI.Break();

            SearchJob.Clean();
            ScanJob.Clean();

            GC.Collect();
        }

        /// <summary>
        /// Creates zip archives of all directories in the input array and copies them to the user-selected destination.
        /// </summary>
        /// <param name="_paths">Array containing paths to directories to be archived.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Archive(params string[] _paths) {

            string archivePath = string.Empty;

            CLI.DisplayText("\nEnter the directory to save the archived files:\n");

            if (CLI.TryGetDirectory(ref archivePath)) {

                Process.Start("explorer.exe", "\"" + archivePath +  "\"");

                CLI.DisplayText("\nZIPPING...\n\nOutput:\n");

                List<string> errors = new List<string>();

                Parallel.For(0, _paths.Length, index => {

                    try {

                        string currDir = System.AppDomain.CurrentDomain.BaseDirectory;
                        string zipName = new DirectoryInfo(_paths[index]).Name + ".zip";

                        if (File.Exists(currDir + zipName)) {
                            File.Delete(currDir + zipName);
                        }

                        ZipFile.CreateFromDirectory(
                            _paths[index],
                            zipName,
                            CompressionLevel.Optimal,
                            false
                        );

                        File.Move(currDir + zipName, archivePath + zipName);

                        CLI.DisplayText("    " + new DirectoryInfo(_paths[index]).Name + "    [OK]\n");
                    }
                    catch (Exception e) {
                        lock (errors) {
                            CLI.DisplayText("    " + new DirectoryInfo(_paths[index]).Name + "    [FAILED]\n");
                            errors.Add(e.Message + "\n");
                        }
                    }
                });

                CLI.DisplayText("\nDONE.\n");

                if (errors.Count != 0) {
                    if (CLI.Prompt("\nWould you like to see the error log?")) {
                        CLI.Break();
                        CLI.DisplayText(errors.ToArray());
                        CLI.Break();
                    }
                }
            }
        }

        /// <summary>
        /// Deletes any files or folders listed in the input array.
        /// </summary>
        /// <param name="_paths"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Delete(string[] _paths) {

            CLI.Break();

            int deleteCount = 0;

            List<string> errors = new List<string>();

            for (int i = 0; i < _paths.Length; i++) {

                CLI.DisplayText("Deleting \"" + _paths[i] + "\"...    ");

                try {
                    // Is directory:
                    if (_paths[i].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).EndsWith(Path.DirectorySeparatorChar)) {
                        //Directory.Delete(_paths[i], true);
                        DeleteFilesAndFoldersRecursively(_paths[i]);
                    }
                    // Is file:
                    else {
                        File.Delete(_paths[i]);
                    }

                    CLI.DisplayText("[DONE]\n");
                    deleteCount++;
                }
                // Log any errors:
                catch (Exception e){
                    CLI.DisplayText("[FAILED]\n");
                    errors.Add(e.Message + "\n");
                }
            }

            CLI.DisplayText("Finished deleting files.\n\n    Results:\n\n        " + deleteCount + " deleted.\n        " + errors.Count + " failed.\n\n");

            if (errors.Count != 0) {
                if (CLI.Prompt("Would you like to see the error log?")) {
                    CLI.Break();
                    CLI.DisplayText(errors.ToArray());
                    CLI.Break();
                }
            }
        }

        /// <summary>
        /// Recursively deletes the contents of a subdirectory.
        /// Courtesy of Mika: https://stackoverflow.com/questions/12415105/directory-is-not-empty-error-when-trying-to-programmatically-delete-a-folder (Converted to use parallel threading for increased performance)
        /// </summary>
        /// <param name="target_dir">Path to the directory to be deleted.</param>
        private static void DeleteFilesAndFoldersRecursively(string target_dir) {

            Parallel.ForEach(Directory.GetFiles(target_dir), file => {
                File.Delete(file);
            });

            Parallel.ForEach(Directory.GetDirectories(target_dir), subDir => {
                DeleteFilesAndFoldersRecursively(subDir);
            });

            Thread.Sleep(1);
            Directory.Delete(target_dir);
        }
    }
}
