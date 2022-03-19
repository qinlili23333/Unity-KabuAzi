using System;
using System.IO;
using System.Text;
using System.Threading;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    /// <summary>
    /// IO related utility methods.
    /// </summary>
    internal static class IOUtilities {
        /// <summary>
        /// Moves all contents of <paramref name="sourceDirectoryPath"/> directory into <paramref name="destinationDirectoryPath"/> directory.
        /// </summary>
        /// <param name="sourceDirectoryPath">
        /// The source directory.
        /// </param>
        /// <param name="destinationDirectoryPath">
        /// The destination directory.
        /// </param>
        /// <param name="overwrite">
        /// Whether to overwrite existing files;
        /// </param>
        public static void MoveDirectoryContents(string sourceDirectoryPath, string destinationDirectoryPath, bool overwrite = false) {
            string[] entries = Directory.GetFileSystemEntries(sourceDirectoryPath);
            foreach (string entryPath in entries) {
                string entryName = Path.GetFileName(entryPath);
                string destinationPath = Path.Combine(destinationDirectoryPath, entryName);
                if (IsDirectory(entryPath)) {
                    AttemptPotentiallyFailingOperation(() => MoveDirectory(entryPath, destinationPath, true, overwrite));
                } else {
                    AttemptPotentiallyFailingOperation(() => MoveFile(entryPath, destinationPath, overwrite));
                }
            }
        }

        /// <summary>
        /// Moves <paramref name="sourceDirectoryPath"/> directory into <paramref name="destinationDirectoryPath"/> directory.
        /// </summary>
        /// <param name="sourceDirectoryPath">
        /// The source directory.
        /// </param>
        /// <param name="destinationDirectoryPath">
        /// The destination directory.
        /// </param>
        /// <param name="recursive">
        /// Whether to move sub-directories as well.
        /// </param>
        /// <param name="overwrite">
        /// Whether to overwrite existing files;
        /// </param>
        public static void MoveDirectory(string sourceDirectoryPath, string destinationDirectoryPath, bool recursive = true, bool overwrite = false) {
            DirectoryInfo dir = new DirectoryInfo(sourceDirectoryPath);
            DirectoryInfo[] directories = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists) {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirectoryPath);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destinationDirectoryPath)) {
                Directory.CreateDirectory(destinationDirectoryPath);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files) {
                // Create the path to the new copy of the file.
                string filePath = Path.Combine(destinationDirectoryPath, file.Name);

                // Move the file.
                try {
                    AttemptPotentiallyFailingOperation(() => MoveFile(file.FullName, filePath, overwrite));
                } catch (SystemException e) {
                    throw new SystemException(string.Format("Unable to move directory '{0}'", sourceDirectoryPath), e);
                }
            }

            if (recursive) {
                foreach (DirectoryInfo directory in directories) {
                    // Create the subdirectory.
                    string dirPath = Path.Combine(destinationDirectoryPath, directory.Name);

                    // Move the subdirectories.
                    MoveDirectory(directory.FullName, dirPath, true, overwrite);
                }
            }

            try {
                AttemptPotentiallyFailingOperation(() => Directory.Delete(sourceDirectoryPath));
            } catch (SystemException e) {
                throw new SystemException(string.Format("Unable to delete directory '{0}'", sourceDirectoryPath), e);
            }
        }

        /// <summary>
        /// Copies <paramref name="sourceDirectoryPath"/> directory into <paramref name="destinationDirectoryPath"/> directory.
        /// </summary>
        /// <param name="sourceDirectoryPath">
        /// The source directory.
        /// </param>
        /// <param name="destinationDirectoryPath">
        /// The destination directory.
        /// </param>
        /// <param name="recursive">
        /// Whether to copy sub-directories as well.
        /// </param>
        public static void CopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath, bool recursive = true) {
            DirectoryInfo dir = new DirectoryInfo(sourceDirectoryPath);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists) {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirectoryPath);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destinationDirectoryPath)) {
                Directory.CreateDirectory(destinationDirectoryPath);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files) {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destinationDirectoryPath, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (recursive) {
                foreach (DirectoryInfo subdir in dirs) {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destinationDirectoryPath, subdir.Name);

                    // Copy the subdirectories.
                    CopyDirectory(subdir.FullName, temppath, true);
                }
            }
        }

        /// <summary>
        /// Moves <paramref name="sourcePath"/> file to <paramref name="destinationPath"/>.
        /// </summary>
        /// <param name="sourcePath">
        /// The source file path.
        /// </param>
        /// <param name="destinationPath">
        /// The destination file path.
        /// </param>
        /// <param name="overwrite">
        /// Whether to overwrite existing files.
        /// </param>
        public static void MoveFile(string sourcePath, string destinationPath, bool overwrite = false) {
            sourcePath = Path.GetFullPath(sourcePath);
            destinationPath = Path.GetFullPath(destinationPath);
            if (overwrite) {
                DeleteFileIfExists(destinationPath);
            }

            File.Move(sourcePath, destinationPath);
        }

        /// <summary>
        /// Checks whether the directory is empty.
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path.
        /// </param>
        /// <returns>
        /// Whether the directory is empty.
        /// </returns>
        public static bool IsDirectoryEmpty(string directoryPath) {
            if (!IsDirectory(directoryPath))
                throw new InvalidOperationException(string.Format("'{0}' is not a directory.", directoryPath));

            return Directory.GetFileSystemEntries(Path.GetFullPath(directoryPath)).Length == 0;
        }

        /// <summary>
        /// Checks whether the provided path correspond to a directory.
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path.
        /// </param>
        /// <returns>
        /// Whether the provided path correspond to a directory.
        /// </returns>
        public static bool IsDirectory(string directoryPath) {
            if (string.IsNullOrEmpty(directoryPath))
                return false;

            FileAttributes attr = File.GetAttributes(directoryPath);
            return (attr & FileAttributes.Directory) != 0;
        }

        /// <summary>
        /// Checks whether the file exists, and deletes it if it does.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// Whether the file existed.
        /// </returns>
        public static bool DeleteFileIfExists(string path) {
            bool exists = File.Exists(path);
            if (exists) {
                AttemptPotentiallyFailingOperation(() => File.Delete(path));
            }

            return exists;
        }

        /// <summary>
        /// Checks whether the directory exists, and deletes it if it does.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="recursive">
        /// Whether to delete the folder recursively.
        /// </param>
        /// <returns>
        /// Whether the directory existed.
        /// </returns>
        public static bool DeleteDirectoryIfExists(string path, bool recursive = false) {
            bool exists = Directory.Exists(path);
            if (exists) {
                AttemptPotentiallyFailingOperation(() => Directory.Delete(path, recursive));
            }

            return exists;
        }

        /// <summary>
        /// Writes a string to file using UTF-8 encoding with no BOM.
        /// </summary>
        /// <param name="path">
        /// The path to save to.
        /// </param>
        /// <param name="contents">
        /// The contents.
        /// </param>
        public static void WriteAllTextUtf8NoBom(string path, string contents) {
            UTF8Encoding utf8WithoutBom = new UTF8Encoding(false);
            using (StreamWriter sw = new StreamWriter(path, false, utf8WithoutBom)) {
                sw.Write(contents);
            }
        }

        public static string MakeRelativePath(string filePath, string referencePath) {
            Uri fileUri = new Uri(filePath);
            Uri referenceUri = new Uri(referencePath);
            return FixSlashes(referenceUri.MakeRelativeUri(fileUri).ToString());
        }

        public static string FixSlashes(string path) {
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// Attempts to execute <paramref name="action"/> <paramref name="maxAttempts"/> number of times with pauses between attempts.
        /// Rethrows the original exception if attemps are depleted.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="maxAttempts">Maximum amount of attempts before throwing an exception.</param>
        /// <param name="delayBetweenAttempts">Delay between attempts.</param>
        public static void AttemptPotentiallyFailingOperation(Action action, int maxAttempts = 3, int delayBetweenAttempts = 400) {
            int failCounter = 0;
            Exception originalException = null;
            while (true) {
                try {
                    action();
                    return;
                } catch (Exception e) {
                    if (originalException == null) {
                        originalException = e;
                    }

                    if (failCounter < maxAttempts) {
                        failCounter++;
                        Thread.Sleep(delayBetweenAttempts);
                        continue;
                    }

                    throw originalException;
                }
            }
        }
    }
}
