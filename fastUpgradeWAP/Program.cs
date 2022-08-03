using fastUpgradeWAP.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.ServiceProcess;
using System.Threading;

namespace fastUpgradeWAP
{
    class Program
    {
        private const int INDEX_PARAM_TARGET_PATH = 0;
        private const int INDEX_PARAM_SILENT_RUN = 1;

        private const string NEWLINE = "\r\n";

        static void Main(string[] args)
        {
            string VersionNumber = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            bool boolVerboseRun = true;
            string pathToZip = string.Empty;

            Log("##############################################################");
            Log($"##      fastUpgradeWAP    (v{VersionNumber})         Made by CRK    ##");
            Log("##############################################################");

            if (args.Length < 1)
            {
                Log($" p0:PATH [p4:/s|/q]{NEWLINE}");
                Log(" p0 REQ: path to new zip installation kit (apache/php).");
                Log($" p1 OPT: silent run (/q, /s silent/quiet. Default verbose){NEWLINE}");

                Log("HINT: works well with TotalCommander with key-bind and arg %P");
                Log("##############################################################");
            }

            if (args.Length > INDEX_PARAM_TARGET_PATH && args[INDEX_PARAM_TARGET_PATH] != string.Empty)
            {
                if (File.Exists(args[INDEX_PARAM_TARGET_PATH]))
                {
                    FileInfo fi = new FileInfo(args[INDEX_PARAM_TARGET_PATH]);
                    if (fi.Extension == ".zip")
                    {
                        pathToZip = args[INDEX_PARAM_TARGET_PATH];
                    }
                    else
                    {
                        Log("[ERROR] Invalid instalation kit (not a zip).");
                    }
                }
                else
                {
                    Log("[ERROR] Inexistent instalation kit (file missing).");
                }
                //[OPTIONAL] SILENT RUN
                if (args.Length > INDEX_PARAM_SILENT_RUN && args[INDEX_PARAM_SILENT_RUN] != string.Empty)
                {
                    boolVerboseRun = args[INDEX_PARAM_SILENT_RUN] != "/s" && args[INDEX_PARAM_SILENT_RUN] != "/q";
                }
            }
            else
            {
                Log("[ERROR] Invalid instalation kit (missing parameter 0).");
            }

            if (pathToZip != string.Empty)
            {
                FileInfo zipFile = new FileInfo(pathToZip);

                bool isApache = zipFile.Name.StartsWith("httpd");
                bool isPhp = zipFile.Name.StartsWith("php");

                if (isApache || isPhp)
                {
                    if (isApache)
                    {
                        Log("[INFO] APACHE installation package detected.");
                        DirectoryInfo apacheRoot = new DirectoryInfo(Settings.Default.APACHE_ROOT);
                        DirectoryInfo versionDir = GetVersionDirectory(apacheRoot);

                        string archiveDest = apacheRoot.Parent.FullName + "\\" + apacheRoot.Name + versionDir.Name;

                        string tempDir = apacheRoot.Parent.FullName + "\\" + apacheRoot.Name + "_TMP";
                        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);

                        if (!Directory.Exists(archiveDest))
                        {
                            ToggleServices(Settings.Default.APACHE_RELATED_SERVICES_TO_STOP, false);

                            Directory.Move(apacheRoot.FullName, archiveDest);
                            Log("[RENAME] Archive current APACHE to " + archiveDest);

                            Log("[UNZIP] Unpacking APACHE zip ...");
                            ZipFile.ExtractToDirectory(zipFile.FullName, tempDir);
                            Log("[UNZIP] Unpacked APACHE zip into temporary folder");

                            string innerApacheKit = "Apache24";
                            if (!Directory.Exists(tempDir + "\\" + innerApacheKit))
                            {
                                if (Directory.Exists(tempDir + "\\Apache22"))
                                {
                                    innerApacheKit = "Apache22";
                                }
                                else
                                {
                                    Log("[ERROR] Apache installation kit does not contain expected inner folder Apache22 or Apache24.");
                                    return;
                                }
                            }
                            Directory.Move(tempDir + "\\" + innerApacheKit, apacheRoot.FullName);
                            Log("[RENAME] Move and rename APACHE from temporary folder to " + apacheRoot.FullName);

                            Directory.Delete(tempDir, true);
                            Log("[DELETE] Delete APACHE temporary folder");

                            Directory.CreateDirectory(apacheRoot.FullName + "_" + zipFile.Name.Replace(zipFile.Extension, string.Empty));
                            Log("[VERSION] Register new APACHE Version with a _version_folder.");

                            //Start copying files from old versio to new one:
                            string[] filesToCopy = Settings.Default.APACHE_FILES_TO_COPY.Split(',');
                            foreach (string relativeFile in filesToCopy)
                            {
                                if (File.Exists(archiveDest + "\\" + relativeFile))
                                {
                                    File.Copy(archiveDest + "\\" + relativeFile, apacheRoot + relativeFile);
                                    Log("[COPY] Copy file: " + relativeFile);
                                }
                                else
                                {
                                    //If it's a directory and not a file
                                    if (Directory.Exists(archiveDest + "\\" + relativeFile))
                                    {
                                        //if directory already exists in the new apache installation folder
                                        if (Directory.Exists(apacheRoot + relativeFile))
                                        {
                                            Directory.Move(apacheRoot + relativeFile, apacheRoot + relativeFile + "_orig");
                                            Log("[RENAME] Apache has already folder " + relativeFile + ". Creating backup " + relativeFile + "_orig");
                                        }
                                        DirectoryCopy(archiveDest + "\\" + relativeFile, apacheRoot + relativeFile, true);
                                        Log("[COPY] Copy directory: " + relativeFile);
                                    }
                                    else
                                    {
                                        Log("[WARNING] File does not exists for copy: " + relativeFile);
                                    }
                                }
                            }
                            ToggleServices(Settings.Default.APACHE_RELATED_SERVICES_TO_STOP, true);
                        }
                        else
                        {
                            Log("[ERROR] APACHE exists already archived: " + archiveDest);
                        }
                        Log("[DONE]");
                    }

                    if (isPhp)
                    {
                        Log("[INFO] PHP installation package detected.");
                        DirectoryInfo phpRoot = new DirectoryInfo(Settings.Default.PHP_ROOT);
                        DirectoryInfo versionDir = GetVersionDirectory(phpRoot);

                        string archiveDest = phpRoot.Parent.FullName + "\\" + phpRoot.Name + versionDir.Name;
                        if (!Directory.Exists(archiveDest))
                        {
                            ToggleServices(Settings.Default.PHP_RELATED_SERVICES_TO_STOP, false);

                            Directory.Move(phpRoot.FullName, archiveDest);
                            Log("[RENAME] Archive current PHP to " + archiveDest);

                            ZipFile.ExtractToDirectory(zipFile.FullName, phpRoot.FullName);
                            Log("[UNZIP] Decompress PHP zip into current PHP folder");

                            Directory.CreateDirectory(phpRoot.FullName + "_" + zipFile.Name.Replace(zipFile.Extension, string.Empty));
                            Log("[VERSION] Register new PHP Version with a _version_folder.");

                            //Start copying files from old versio to new one:
                            string[] filesToCopy = Settings.Default.PHP_FILES_TO_COPY.Split(',');
                            foreach (string relativeFile in filesToCopy)
                            {
                                if (File.Exists(archiveDest + "\\" + relativeFile))
                                {
                                    File.Copy(archiveDest + "\\" + relativeFile, phpRoot + relativeFile);
                                    Log("[COPY] Copy file: " + relativeFile);
                                }
                                else
                                {
                                    //If it's a directory and not a file
                                    if (Directory.Exists(archiveDest + "\\" + relativeFile))
                                    {
                                        //if directory already exists in the new apache installation folder
                                        if (Directory.Exists(phpRoot + relativeFile))
                                        {
                                            Directory.Move(phpRoot + relativeFile, phpRoot + relativeFile + "_orig");
                                            Log("[RENAME] PHP has already folder " + relativeFile + ". Creating backup " + relativeFile + "_orig");
                                        }
                                        DirectoryCopy(archiveDest + "\\" + relativeFile, phpRoot + relativeFile, true);
                                        Log("[COPY] Copy directory: " + relativeFile);
                                    }
                                    else
                                    {
                                        Log("[WARNING] File does not exists for copy: " + relativeFile);
                                    }
                                }
                            }
                            ToggleServices(Settings.Default.PHP_RELATED_SERVICES_TO_STOP, true);
                        }
                        else
                        {
                            Log("[ERROR] PHP exists already archived: " + archiveDest);
                        }

                        Log("[DONE]");
                    }
                }
                else
                {
                    Log("[ERROR] Invalid instalation kit (not apache or php).");
                }
            }

            Log("Press any key to exit...");
            Console.ReadKey();
        }

        private static void ToggleServices(string relatedServices, bool start)
        {
            string[] services = relatedServices.Split(',');

            foreach (string service in services)
            {
                if (service != string.Empty)
                {
                    ServiceController serviceController = new ServiceController(service);
                    ServiceControllerStatus status = serviceController.Status;
                    if (start)
                    {
                        if (!status.Equals(ServiceControllerStatus.Running))
                        {
                            Log($"[SERVICES] Starting service {service}...");
                            serviceController.Start();
                            serviceController.WaitForStatus(ServiceControllerStatus.Running);
                            Log($"[SERVICES] Waiting for service({status}) {service} to start...");
                            Thread.Sleep(5000);
                            status = serviceController.Status;
                            if (status != ServiceControllerStatus.Running)
                            {
                                Log($"[SERVICES] Waiting even more for service({status}) {service} to start...");
                                Thread.Sleep(5000);
                                status = serviceController.Status;
                                if (status != ServiceControllerStatus.Running)
                                {
                                    Log($"[SERVICES] Waiting even more for service({status}) {service} to start...");
                                    Thread.Sleep(10000);
                                    status = serviceController.Status;
                                    if (status != ServiceControllerStatus.Running)
                                    {
                                        Log($"[SERVICES] {service} doesn't seem to want to start...({status})");
                                    }
                                }
                            }

                            Log("[SERVICES] Started related services " + relatedServices);
                        }
                        else
                        {
                            Log($"[WARNING] Service({status}) was already RUNNING.");
                        }
                    }
                    else
                    {
                        if (!status.Equals(ServiceControllerStatus.Stopped))
                        {
                            Log($"[SERVICES] Stopping service {service}...");
                            serviceController.Stop();
                            serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                            Log($"[SERVICES] Waiting for service({status}) {service} to stop...");
                            Thread.Sleep(5000);
                            status = serviceController.Status;
                            if (status != ServiceControllerStatus.Stopped)
                            {
                                Log($"[SERVICES] Waiting even more for service({status}) {service} to stop...");
                                Thread.Sleep(5000);
                                status = serviceController.Status;
                                if (status != ServiceControllerStatus.Stopped)
                                {
                                    Log($"[SERVICES] Waiting even more for service({status}) {service} to stop...");
                                    Thread.Sleep(10000);

                                    status = serviceController.Status;
                                    if (status != ServiceControllerStatus.Stopped)
                                    {
                                        Log($"[SERVICES] {service} doesn't seem to want to stop...({status})");
                                    }
                                }
                            }

                            Log("[SERVICES] Stopped related services " + relatedServices);
                        }
                        else
                        {
                            Log($"[WARNING] Service({status}) was already STOPPED.");
                        }
                    }
                }
            }
        }

        private static void Log(string text, bool newLine = true)
        {
            ConsoleColor color = ConsoleColor.White;
            ConsoleColor bgColor = ConsoleColor.Black;

            if (text.StartsWith("[ERROR]"))
            {
                color = ConsoleColor.White;
                bgColor = ConsoleColor.DarkRed;
            }
            if (text.StartsWith("[WARNING]"))
            {
                color = ConsoleColor.Black;
                bgColor = ConsoleColor.DarkYellow;
            }
            if (text.StartsWith("[INFO]"))
            {
                color = ConsoleColor.White;
                bgColor = ConsoleColor.DarkBlue;
            }
            if (text.StartsWith("[DELETE]"))
            {
                color = ConsoleColor.DarkRed;
            }
            if (text.StartsWith("[COPY]"))
            {
                color = ConsoleColor.Cyan;
            }
            if (text.StartsWith("[VERSION]"))
            {
                color = ConsoleColor.Yellow;
            }
            if (text.StartsWith("[RENAME]"))
            {
                color = ConsoleColor.DarkCyan;
            }
            if (text.StartsWith("[UNZIP]"))
            {
                color = ConsoleColor.Magenta;
            }
            if (text.StartsWith("[SERVICES]"))
            {
                color = ConsoleColor.DarkYellow;
            }
            if (text.StartsWith("[DONE]"))
            {
                color = ConsoleColor.Green;
            }

            //Backup default colors to revert later.
            ConsoleColor defaultColor = Console.ForegroundColor;
            ConsoleColor defaultBgColor = Console.BackgroundColor;

            //set colors
            Console.ForegroundColor = color;
            Console.BackgroundColor = bgColor;

            //print
            Console.Write(text + (newLine ? NEWLINE : string.Empty));

            //revert colors
            Console.ForegroundColor = defaultColor;
            Console.BackgroundColor = defaultBgColor;
        }

        private static DirectoryInfo GetVersionDirectory(DirectoryInfo parentDir)
        {
            string[] phpDirs = Directory.GetDirectories(parentDir.FullName);
            foreach (string dir in phpDirs)
            {
                if (new DirectoryInfo(dir).Name.StartsWith("_"))
                {
                    return new DirectoryInfo(dir);
                }
            }

            return null;
        }

        private static void DirectoryCopy(
        string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}