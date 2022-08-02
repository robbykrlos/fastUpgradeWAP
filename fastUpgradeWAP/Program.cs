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
                        DirectoryInfo apacheRoot = new DirectoryInfo(Settings.Default.APACHE_ROOT);
                        DirectoryInfo versionDir = GetVersionDirectory(apacheRoot);

                        string archiveDest = apacheRoot.Parent.FullName + "\\" + apacheRoot.Name + versionDir.Name;
                        if (!Directory.Exists(archiveDest))
                        {
                            ToggleServices(Settings.Default.APACHE_RELATED_SERVICES_TO_STOP, false);

                            Directory.Move(apacheRoot.FullName, archiveDest);
                            Log("[RENAME] Archive current APACHE to " + archiveDest);

                            ZipFile.ExtractToDirectory(zipFile.FullName, apacheRoot.FullName);
                            Log("[UNZIP] Decompress APACHE zip into current APACHE folder");

                            Directory.CreateDirectory(apacheRoot.FullName + "_" + zipFile.Name.Replace(zipFile.Extension, string.Empty));
                            Log("[VERSION] Register new APACHE Version with a _version_folder.");

                            //Start copying files from old versio to new one:
                            string[] filesToCopy = Settings.Default.APACHE_FILES_TO_COPY.Split(',');
                            foreach (string relativeFile in filesToCopy)
                            {
                                if (File.Exists(archiveDest + "\\" + relativeFile))
                                {
                                    File.Copy(archiveDest + "\\" + relativeFile, apacheRoot + relativeFile);
                                    Log("[COPY] Copy file needed: " + relativeFile);
                                }
                                else
                                {
                                    if (Directory.Exists(archiveDest + "\\" + relativeFile))
                                    {
                                        Directory.CreateDirectory(apacheRoot + relativeFile);
                                        Log("[COPY] Create needed directory: " + relativeFile);
                                    }
                                    else
                                    {
                                        Log("[ERROR] File does not exists for copy: " + relativeFile);
                                    }
                                }
                            }
                            ToggleServices(Settings.Default.APACHE_RELATED_SERVICES_TO_STOP, false);
                        }
                        else
                        {
                            Log("[ERROR] PHP exists already archived: " + archiveDest);
                        }
                        Log("[DONE]");
                    }

                    if (isPhp)
                    {
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
                                    Log("[COPY] Copy file needed: " + relativeFile);
                                }
                                else
                                {
                                    if (Directory.Exists(archiveDest + "\\" + relativeFile))
                                    {
                                        Directory.CreateDirectory(phpRoot + relativeFile);
                                        Log("[COPY] Create needed directory: " + relativeFile);
                                    }
                                    else
                                    {
                                        Log("[ERROR] File does not exists for copy: " + relativeFile);
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
                        if (status == ServiceControllerStatus.Stopped)
                        {
                            serviceController.Start();
                            serviceController.WaitForStatus(ServiceControllerStatus.Running);
                            Thread.Sleep(5000);
                            if(status != ServiceControllerStatus.Running) Thread.Sleep(10000);
                            Log("[SERVICES] Started related services " + relatedServices);
                        }
                        else
                        {
                            Log("[ERROR] Service already RUNNING.");
                        }
                    }
                    else
                    {
                        if (status == ServiceControllerStatus.Running)
                        {
                            serviceController.Stop();
                            serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                            Thread.Sleep(5000);
                            if (status != ServiceControllerStatus.Stopped) Thread.Sleep(10000);
                            Log("[SERVICES] Stopped related services " + relatedServices);
                        }
                        else
                        {
                            Log("[ERROR] Service already STOPPED.");
                        }
                    }
                }
            }
        }

        private static void Log(string text, bool newLine = true)
        {
            ConsoleColor color = ConsoleColor.White;

            if (text.StartsWith("[ERROR]"))
            {
                color = ConsoleColor.Red;
            }
            if (text.StartsWith("[COPY]"))
            {
                color = ConsoleColor.Cyan;
            }
            if (text.StartsWith("[RENAME]"))
            {
                color = ConsoleColor.Yellow;
            }
            if (text.StartsWith("[VERSION]"))
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


            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text + (newLine ? NEWLINE : string.Empty));
            Console.ForegroundColor = defaultColor;
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
    }
}