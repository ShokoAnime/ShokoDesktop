using JMMClient.Forms;
using JMMClient.Utilities;
using Microsoft.Win32;
using NLog;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Path = Pri.LongPath.Path;
using Directory = Pri.LongPath.Directory;
using File = Pri.LongPath.File;

namespace JMMClient
{
    public static class Utils
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        extern static bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern static IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        extern static IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        extern static IntPtr GetProcAddress(IntPtr hModule, string methodName);

        #region PrettyFilesize
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        static extern long StrFormatByteSize(long fileSize,
        [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);

        private static object assemblyLock = new object();


        public static IEnumerable<ScrollViewer> GetScrollViewers(DependencyObject control)
        {
            for (DependencyObject element = control; element != null; element = System.Windows.Media.VisualTreeHelper.GetParent(element))
                if (element is ScrollViewer) yield return element as ScrollViewer;
        }
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Looks for the given list of exes in the sys path.
        /// </summary>
        /// <param name="files">An array of exes</param>
        /// <returns>The full path to the first occurance of an exe in the path.</returns>
        public static string CheckSysPath(string[] files)
        {
            try
            {
                string sysPath = Environment.GetEnvironmentVariable("PATH");
                string playerPath = null;
                foreach (var path in sysPath.Split(';'))
                {
                    foreach (string file in files)
                    {
                        playerPath = Path.Combine(path, file);
                        if (File.Exists(playerPath))
                            return playerPath;
                    }
                }
            } catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Invalid PATH variable.");
            }
            return null;
        }

        public static void PopulateScheduledComboBox(System.Windows.Controls.ComboBox cbo, ScheduledUpdateFrequency curFrequency)
        {
            cbo.Items.Clear();
            cbo.Items.Add(Properties.Resources.UpdateFrequency_6Hours);
            cbo.Items.Add(Properties.Resources.UpdateFrequency_12Hours);
            cbo.Items.Add(Properties.Resources.UpdateFrequency_Daily);
            cbo.Items.Add(Properties.Resources.UpdateFrequency_OneWeek);
            cbo.Items.Add(Properties.Resources.UpdateFrequency_OneMonth);
            cbo.Items.Add(Properties.Resources.UpdateFrequency_Never);

            switch (curFrequency)
            {
                case ScheduledUpdateFrequency.HoursSix: cbo.SelectedIndex = 0; break;
                case ScheduledUpdateFrequency.HoursTwelve: cbo.SelectedIndex = 1; break;
                case ScheduledUpdateFrequency.Daily: cbo.SelectedIndex = 2; break;
                case ScheduledUpdateFrequency.WeekOne: cbo.SelectedIndex = 3; break;
                case ScheduledUpdateFrequency.MonthOne: cbo.SelectedIndex = 4; break;
                case ScheduledUpdateFrequency.Never: cbo.SelectedIndex = 5; break;
            }

        }

        public static string GetTempFilePathWithExtension(string extension)
        {
            var path = Path.GetTempPath();
            var fileName = Guid.NewGuid().ToString() + extension;
            return Path.Combine(path, fileName);
        }

        public static string FormatByteSize(long fileSize)
        {
            StringBuilder sbBuffer = new StringBuilder(20);
            StrFormatByteSize(fileSize, sbBuffer, 20);
            return sbBuffer.ToString();
        }
        #endregion

        public static string GetED2KDump(string result)
        {
            // try and get the ed2k dump from the keyboard
            string[] lines = result.Split('\r');
            foreach (string line in lines)
            {
                string editedLine = line.Replace('\n', ' ');
                editedLine = editedLine.Trim();

                if (editedLine.StartsWith(@"ed2k://"))
                    return editedLine;
            }

            return "";
        }

        /// <summary>
        /// Looks to see if JMM Server is installed on the local machine, and starts it.
        /// </summary>
        /// <returns>Returns true if jmm server is local.</returns>
        public static bool StartJMMServer()
        {
            //TODO Check 32bit registry logic
            //TODO Wait a little bit for jmm server to start (just enough for the window to load)
            //TODO Let the client know that the server is initalising.

            // first check if JMM Server is already started
            Mutex mutex;
            string mutexName = "JmmServer3.0Mutex";

            try
            {
                mutex = Mutex.OpenExisting(mutexName);
                //since it hasn't thrown an exception, then we already have one copy of the app open.
                return true; // already running
            }
            catch { }
            
            string JMMServerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{0BA2D22B-A0B7-48F8-8AA1-BAAEFC2034CB}_is1", "InstallLocation", null);

            if (string.IsNullOrEmpty(JMMServerPath))
                return false;

            logger.Info("Found ShokoServer install path in reg");

            Process proc = new Process();
            proc.StartInfo.ErrorDialog = false;
            JMMServerPath = Path.Combine(JMMServerPath, "ShokoServer.exe");
            proc.StartInfo.FileName = JMMServerPath;
            proc.StartInfo.UseShellExecute = false;
            if (!File.Exists(JMMServerPath))
            {
                logger.Info("No file found at reg path given for ShokoServer");
                return false;
            }

            Process[] pname = Process.GetProcessesByName("ShokoServer");
            if (pname.Length != 0) return true;

            try
            {
                logger.Info("Starting JMM Server");
                return proc.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error starting ShokoServer");
                return false;
            }
        }

        public static DateTime? GetAniDBDateAsDate(int secs)
        {
            if (secs == 0) return null;

            DateTime thisDate = new DateTime(1970, 1, 1, 0, 0, 0);
            thisDate = thisDate.AddSeconds(secs);
            return thisDate;
        }

        public static DateTime? GetUTCDate(long secs)
        {
            if (secs == 0) return null;

            DateTime thisDate = new DateTime(1970, 1, 1, 0, 0, 0);
            thisDate = thisDate.AddSeconds(secs);
            return thisDate;
        }

        public static string DownloadWebPage(string url)
        {
            return DownloadWebPage(url, null, false);
        }

        public static string DownloadWebPage(string url, string cookieHeader, bool setUserAgent)
        {
            try
            {
                logger.Trace("DownloadWebPage: {0}", url);

                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
                webReq.Timeout = 30000; // 30 seconds
                webReq.Proxy = null;
                webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

                if (!string.IsNullOrEmpty(cookieHeader))
                    webReq.Headers.Add("Cookie", cookieHeader);
                if (setUserAgent)
                    webReq.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

                webReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                HttpWebResponse WebResponse = (HttpWebResponse)webReq.GetResponse();

                Stream responseStream = WebResponse.GetResponseStream();
                String enco = WebResponse.CharacterSet;
                Encoding encoding = null;
                if (!String.IsNullOrEmpty(enco))
                    encoding = Encoding.GetEncoding(WebResponse.CharacterSet);
                if (encoding == null)
                    encoding = Encoding.Default;
                StreamReader Reader = new StreamReader(responseStream, encoding);

                string output = Reader.ReadToEnd();

                WebResponse.Close();
                responseStream.Close();

                //logger.Trace("DownloadWebPage Response: {0}", output);

                return output;
            }
            catch (Exception ex)
            {
                string msg = "---------- ERROR IN DOWNLOAD WEB PAGE ---------" + Environment.NewLine +
                    url + Environment.NewLine +
                    ex.ToString() + Environment.NewLine + "------------------------------------";
                logger.Error(msg);

                // if the error is a 404 error it may mean that there is a bad series association
                // so lets log it to the web cache so we can investigate
                if (ex.ToString().Contains("(404) Not Found"))
                {
                }

                return "";
            }
        }

        public static void DownloadFile(string url, string destFile, string cookieHeader, bool setUserAgent)
        {
            try
            {
                logger.Trace("DownloadFile: {0}", url);


                using (WebClient client = new WebClient())
                {
                    if (!string.IsNullOrEmpty(cookieHeader))
                        client.Headers.Add("Cookie", cookieHeader);
                    if (setUserAgent)
                        client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)");

                    client.DownloadFile(url, destFile);
                }

            }
            catch (Exception ex)
            {
                string msg = "---------- ERROR IN DOWNLOAD WEB PAGE ---------" + Environment.NewLine +
                    url + Environment.NewLine +
                    ex.ToString() + Environment.NewLine + "------------------------------------";
                logger.Error(msg);

                // if the error is a 404 error it may mean that there is a bad series association
                // so lets log it to the web cache so we can investigate
                if (ex.ToString().Contains("(404) Not Found"))
                {
                }
            }
        }

        public static void ShowErrorMessage(string msg, Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(msg, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            logger.Error(ex, ex.ToString());
        }

        public static void ShowErrorMessage(Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            logger.Error(ex, ex.ToString());
        }

        public static void ShowErrorMessage(string msg)
        {
            System.Windows.Forms.MessageBox.Show(msg, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            logger.Error(msg);
        }

        public static string FormatAniDBRating(double rat)
        {
            return String.Format("{0:0.00}", rat);

        }

        public static string FormatPercentage(double val)
        {
            return String.Format("{0:0.0}%", val);

        }

        public static string FormatSecondsToDisplayTime(int secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);

            if (t.Hours > 0)
                return string.Format("{0}:{1}:{2}", t.Hours, t.Minutes.ToString().PadLeft(2, '0'), t.Seconds.ToString().PadLeft(2, '0'));
            else
                return string.Format("{0}:{1}", t.Minutes, t.Seconds.ToString().PadLeft(2, '0'));
        }

        public static BackgroundWorker RunBackgroundWork(DoWorkEventHandler doWorkEvent,
            RunWorkerCompletedEventHandler completedEvent,
            ProgressChangedEventHandler progressEvent,
            object parameter)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;

            if (progressEvent != null)
            {
                worker.WorkerReportsProgress = true;
                worker.ProgressChanged += progressEvent;
            }

            if (doWorkEvent != null) worker.DoWork += doWorkEvent;
            if (completedEvent != null) worker.RunWorkerCompleted += completedEvent;

            if (parameter == null)
                worker.RunWorkerAsync();
            else
                worker.RunWorkerAsync(parameter);

            return worker;
        }

        public static string FormatFileSize(long bytes)
        {
            double mb = (bytes / 1024f) / 1024f;

            return mb.ToString("##.# MB");
        }

        public static string FormatFileSize(double bytes)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (bytes <= (Math.Pow(1024, i + 1)))
                {
                    return ThreeNonZeroDigits(bytes /
                        Math.Pow(1024, i)) +
                        " " + suffixes[i];
                }
            }

            return ThreeNonZeroDigits(bytes /
                Math.Pow(1024, suffixes.Length - 1)) +
                " " + suffixes[suffixes.Length - 1];
        }

        public static string FormatBitRate(double bytes)
        {
            string[] suffixes = { "bytes", "kbps" };
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (bytes <= (Math.Pow(1024, i + 1)))
                {
                    return ThreeNonZeroDigits(bytes /
                        Math.Pow(1024, i)) +
                        " " + suffixes[i];
                }
            }

            return ThreeNonZeroDigits(bytes /
                Math.Pow(1024, suffixes.Length - 1)) +
                " " + suffixes[suffixes.Length - 1];
        }

        private static string ThreeNonZeroDigits(double value)
        {
            if (value >= 100)
            {
                // No digits after the decimal.
                return value.ToString("0,0");
            }
            else if (value >= 10)
            {
                // One digit after the decimal.
                return value.ToString("0.0");
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString("0.00");
            }
        }

        public static string GetBaseImagesPath()
        {
            lock (assemblyLock)
            {
                if (Directory.Exists(AppSettings.ImagesPath))
                    return AppSettings.ImagesPath;
                string serverpath = AppSettings.JMMServerImagePath;
                if (Directory.Exists(serverpath))
                    return serverpath;
                serverpath = AppSettings.DefaultImagePath;
                if (!Directory.Exists(serverpath))
                    Directory.CreateDirectory(serverpath);
                return serverpath;
            }

        }
        public static void GrantAccess(string fullPath)
        {
            //C# version do not work, do not inherit permissions to childs.
            string BatchFile = Path.Combine(System.IO.Path.GetTempPath(), "GrantAccess.bat");
            int exitCode = -1;
            Process proc = new Process();

            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = $@"/c {BatchFile}";
            proc.StartInfo.Verb = "runas";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.UseShellExecute = true;

            try
            {
                StreamWriter BatchFileStream = new StreamWriter(BatchFile);

                //Cleanup previous
                try
                {
                    string batchLine = $"{"icacls"} \"{fullPath}\" {"/grant *S-1-1-0:(OI)(CI)F /T"}";
                    logger.Log(LogLevel.Info, "GrantAccess batch line: " + batchLine);
                    BatchFileStream.WriteLine(batchLine);
                }
                finally
                {
                    BatchFileStream.Close();
                }

                proc.Start();

                proc.WaitForExit();

                exitCode = proc.ExitCode;
                proc.Close();

                File.Delete(BatchFile);

                if (exitCode == 0)
                {
                    logger.Info("Successfully granted write permissions to " + fullPath);
                }
                else
                {
                    logger.Error("Temporary batch process for granting folder write access returned error code: " +
                                 exitCode);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
            }
        }

        public static string GetBaseAniDBImagesPath()
        {
            string filePath = Path.Combine(GetBaseImagesPath(), "AniDB");

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetImagesTempFolder()
        {
            string filePath = Path.Combine(GetBaseImagesPath(), "_Temp_");

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetBaseAniDBCharacterImagesPath()
        {
            string filePath = Path.Combine(GetBaseImagesPath(), "AniDB_Char");

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetBaseAniDBCreatorImagesPath()
        {
            string filePath = Path.Combine(GetBaseImagesPath(), "AniDB_Creator");

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetAniDBCharacterImagePath(int charID)
        {
            string subFolder = "";
            string sid = charID.ToString();
            if (sid.Length == 1)
                subFolder = sid;
            else
                subFolder = sid.Substring(0, 2);

            string filePath = Path.Combine(GetBaseAniDBCharacterImagesPath(), subFolder);

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetAniDBCreatorImagePath(int creatorID)
        {
            string subFolder = "";
            string sid = creatorID.ToString();
            if (sid.Length == 1)
                subFolder = sid;
            else
                subFolder = sid.Substring(0, 2);

            string filePath = Path.Combine(GetBaseAniDBCreatorImagesPath(), subFolder);

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetAniDBImagePath(int animeID)
        {
            string subFolder = "";
            string sid = animeID.ToString();
            if (sid.Length == 1)
                subFolder = sid;
            else
                subFolder = sid.Substring(0, 2);

            string filePath = Path.Combine(GetBaseAniDBImagesPath(), subFolder);

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetBaseTraktImagesPath()
        {
            string filePath = Path.Combine(GetBaseImagesPath(), "Trakt");

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetTraktImagePath()
        {
            string filePath = GetBaseTraktImagesPath();

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetTraktImagePath_Avatars()
        {
            string filePath = Path.Combine(GetTraktImagePath(), "Avatars");

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetBaseTvDBImagesPath()
        {
            string filePath = Path.Combine(GetBaseImagesPath(), "TvDB");

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetTvDBImagePath()
        {
            string filePath = GetBaseTvDBImagesPath();

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetBaseMovieDBImagesPath()
        {
            string filePath = Path.Combine(GetBaseImagesPath(), "MovieDB");

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static string GetMovieDBImagePath()
        {
            string filePath = GetBaseMovieDBImagesPath();

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            return filePath;
        }

        public static void OpenFolderAndSelectFile(string fullFilePath)
        {
            try
            {
                /*Process ExplorerWindowProcess = new Process();

				ExplorerWindowProcess.StartInfo.FileName = "explorer.exe";
				ExplorerWindowProcess.StartInfo.Arguments = string.Format("/select,\"{0}\"", fullFilePath);

				ExplorerWindowProcess.Start();*/

                ShowSelectedInExplorer.FilesOrFolders(fullFilePath);

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public static void OpenFile(string fullePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo(fullePath));
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public static void OpenFolder(string fullPath)
        {
            try
            {
                if (Directory.Exists(fullPath))
                {
                    Process.Start(new ProcessStartInfo(fullPath));
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Compute Levenshtein distance --- http://www.merriampark.com/ldcsharp.htm
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length; //length of s
            int m = t.Length; //length of t

            int[,] d = new int[n + 1, m + 1]; // matrix

            int cost; // cost

            // Step 1
            if (n == 0) return m;
            if (m == 0) return n;

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    cost = (t.Substring(j - 1, 1) == s.Substring(i - 1, 1) ? 0 : 1);

                    // Step 6
                    d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                              d[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return d[n, m];
        }
        public static int InverseLevenshteinDistance(string s, string t)
        {
            int n = s.Length; //length of s
            int m = t.Length; //length of t

            int[,] d = new int[n + 1, m + 1]; // matrix

            int cost; // cost

            // Step 1
            if (n == 0) return m;
            if (m == 0) return n;

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    cost = (t.Substring((m - j), 1) == s.Substring((n - i), 1) ? 0 : 1);

                    // Step 6
                    d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                              d[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return d[n, m];
        }

        public static string GetApplicationVersion(Assembly a)
        {
            AssemblyName an = a.GetName();
            return an.Version.ToString();
        }

        public static string GetOSInfo()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = "2000";
                        else
                            operatingSystem = "XP";
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = "Vista";
                        else
                            operatingSystem = "7";
                        break;
                    default:
                        break;
                }
            }
            //Make sure we actually got something in our OS check
            //We don't want to just return " Service Pack 2" or " 32-bit"
            //That information is useless without the OS version.
            if (operatingSystem != "")
            {
                //Got something.  Let's prepend "Windows" and get more info.
                operatingSystem = "Windows " + operatingSystem;
                //See if there's a service pack installed.
                if (os.ServicePack != "")
                {
                    //Append it to the OS name.  i.e. "Windows XP Service Pack 3"
                    operatingSystem += " " + os.ServicePack;
                }
                //Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
                operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
            }
            //Return the information we've gathered.
            return operatingSystem;
        }

        public static int getOSArchitecture()
        {
            if (Is64BitOperatingSystem)
                return 64;
            else
                return 32;
        }

        public static bool Is64BitProcess
        {
            get { return IntPtr.Size == 8; }
        }

        public static bool Is64BitOperatingSystem
        {
            get
            {
                // Clearly if this is a 64-bit process we must be on a 64-bit OS.
                if (Is64BitProcess)
                    return true;
                // Ok, so we are a 32-bit process, but is the OS 64-bit?
                // If we are running under Wow64 than the OS is 64-bit.
                bool isWow64;
                return ModuleContainsFunction("kernel32.dll", "IsWow64Process") && IsWow64Process(GetCurrentProcess(), out isWow64) && isWow64;
            }
        }

        static bool ModuleContainsFunction(string moduleName, string methodName)
        {
            IntPtr hModule = GetModuleHandle(moduleName);
            if (hModule != IntPtr.Zero)
                return GetProcAddress(hModule, methodName) != IntPtr.Zero;
            return false;
        }

        private static string[] escapes = { "SOURCE", "TAKEN", "FROM", "HTTP", "ANN", "ANIMENFO", "ANIDB", "ANIMESUKI" };

        public static string ReparseDescription(string description)
        {
            if (description == null || description.Length == 0) return "";

            string val = description;
            val = val.Replace("<br />", Environment.NewLine).Replace("<br/>", Environment.NewLine).Replace("<i>", "").
                    Replace("</i>", "").Replace("<b>", "").Replace("</b>", "").Replace("[i]", "").Replace("[/i]", "").
                    Replace("[b]", "").Replace("[/b]", "");
            val = val.Replace("<BR />", Environment.NewLine).Replace("<BR/>", Environment.NewLine).Replace("<I>", "").Replace("</I>", "").Replace("<B>", "").Replace("</B>", "").Replace("[I]", "").Replace("[/I]", "").
                    Replace("[B]", "").Replace("[/B]", "");

            string vup = val.ToUpper();
            while ((vup.Contains("[URL")) || (vup.Contains("[/URL]")))
            {
                int a = vup.IndexOf("[URL");
                if (a >= 0)
                {
                    int b = vup.IndexOf("]", a + 1);
                    if (b >= 0)
                    {
                        val = val.Substring(0, a) + val.Substring(b + 1);
                        vup = val.ToUpper();
                    }
                }
                a = vup.IndexOf("[/URL]");
                if (a >= 0)
                {
                    val = val.Substring(0, a) + val.Substring(a + 6);
                    vup = val.ToUpper();
                }
            }
            while (vup.Contains("HTTP:"))
            {
                int a = vup.IndexOf("HTTP:");
                if (a >= 0)
                {
                    int b = vup.IndexOf(" ", a + 1);
                    if (b >= 0)
                    {
                        if (vup[b + 1] == '[')
                        {
                            int c = vup.IndexOf("]", b + 1);
                            val = val.Substring(0, a) + " " + val.Substring(b + 2, c - b - 2) + val.Substring(c + 1);
                        }
                        else
                        {
                            val = val.Substring(0, a) + val.Substring(b);
                        }
                        vup = val.ToUpper();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            int d = -1;
            do
            {
                if (d + 1 >= vup.Length)
                    break;
                d = vup.IndexOf("[", d + 1);
                if (d != -1)
                {
                    int b = vup.IndexOf("]", d + 1);
                    if (b != -1)
                    {
                        string cont = vup.Substring(d, b - d);
                        bool dome = false;
                        foreach (string s in escapes)
                        {
                            if (cont.Contains(s))
                            {
                                dome = true;
                                break;
                            }
                        }
                        if (dome)
                        {
                            val = val.Substring(0, d) + val.Substring(b + 1);
                            vup = val.ToUpper();
                        }
                    }
                }
            } while (d != -1);
            d = -1;
            do
            {
                if (d + 1 >= vup.Length)
                    break;

                d = vup.IndexOf("(", d + 1);
                if (d != -1)
                {
                    int b = vup.IndexOf(")", d + 1);
                    if (b != -1)
                    {
                        string cont = vup.Substring(d, b - d);
                        bool dome = false;
                        foreach (string s in escapes)
                        {
                            if (cont.Contains(s))
                            {
                                dome = true;
                                break;
                            }
                        }
                        if (dome)
                        {
                            val = val.Substring(0, d) + val.Substring(b + 1);
                            vup = val.ToUpper();
                        }
                    }
                }
            } while (d != -1);
            d = vup.IndexOf("SOURCE:");
            if (d == -1)
                d = vup.IndexOf("SOURCE :");
            if (d > 0)
            {
                val = val.Substring(0, d);
            }
            return val.Trim();
        }

        public static void PromptToRateSeries(AnimeSeriesVM ser, Window parentWindow)
        {
            try
            {
                if (!AppSettings.DisplayRatingDialogOnCompletion) return;

                // if the user doesn't have all the episodes return
                if (ser.MissingEpisodeCount > 0) return;

                // only prompt the user if the series has finished airing
                // and the user has watched all the episodes
                if (!ser.AniDB_Anime.FinishedAiring || !ser.AllFilesWatched) return;

                if (ser.AniDB_Anime.Detail.UserHasVoted) return;

                RateSeriesForm frm = new RateSeriesForm();
                frm.Owner = parentWindow;
                frm.Init(ser);
                bool? result = frm.ShowDialog();

            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void ExecuteCommandSync(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                Console.WriteLine(result);
            }
            catch (Exception)
            {
                // Log the exception
            }
        }

        public static void ClearAutoUpdateCache()
        {
            // rmdir /s /q "%userprofile%\wc"
            ExecuteCommandSync("rmdir /s /q \"%userprofile%\\wc\"");
        }

        public static void AniDBVoteRecommendation(int animeID, int similarAnimeID, bool isVoteUp)
        {
            string url = string.Format(@"http://anidb.net/perl-bin/animedb.pl?show=addsimilaranime&do.vote={0}&aid={1}&sid={2}&redirect=anime",
                isVoteUp ? "up" : "down", animeID, similarAnimeID);

            Uri uri = new Uri(url);
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
        }

        public static void RestartAsAdmin()
        {
            string BatchFile = Path.Combine(System.IO.Path.GetTempPath(), "RestartAsAdmin.bat");
            var exeName = Process.GetCurrentProcess().MainModule.FileName;

            int exitCode = -1;
            Process proc = new Process();

            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = $@"/c {BatchFile}";
            proc.StartInfo.Verb = "runas";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.UseShellExecute = true;

            try
            {
                StreamWriter BatchFileStream = new StreamWriter(BatchFile);

                //Cleanup previous
                try
                {
                    // Wait a few seconds to allow shutdown later on, use task kill just in case still running
                    string batchline = $"timeout 5 && taskkill /F /IM {System.AppDomain.CurrentDomain.FriendlyName} /fi \"memusage gt 2\" && \"{exeName}\"";
                    Debug.WriteLine(LogLevel.Info, "RestartAsAdmin batch line: " + batchline);
                    BatchFileStream.WriteLine(batchline);
                }
                finally
                {
                    BatchFileStream.Close();
                }

                proc.Start();
                System.Windows.Application.Current.Shutdown();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Info, "Error occured during RestartAsAdmin(): " + ex.Message);
            }
        }
    }
}
