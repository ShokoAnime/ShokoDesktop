using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Shoko.Desktop.ViewModel;
using Shoko.Models.Enums;

namespace Shoko.Desktop.Downloads
{
    public class UTorrentHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string token = "";

        public bool Initialised { get; set; }

        CookieContainer cookieJar = null;

        private const string urlTorrentList = "http://{0}:{1}/gui/?token={2}&list=1";
        private const string urlTorrentFileList = "http://{0}:{1}/gui/?token={2}&action=getfiles&hash={3}";

        private const string urlTorrentTokenPage = "http://{0}:{1}/gui/token.html";
        private const string urlTorrentStart = "http://{0}:{1}/gui/?token={2}&action=start&hash={3}";
        private const string urlTorrentStop = "http://{0}:{1}/gui/?token={2}&action=stop&hash={3}";
        private const string urlTorrentPause = "http://{0}:{1}/gui/?token={2}&action=pause&hash={3}";
        private const string urlTorrentAddURL = "http://{0}:{1}/gui/?token={2}&action=add-url&s={3}";
        private const string urlTorrentRemove = "http://{0}:{1}/gui/?token={2}&action=remove&hash={3}";
        private const string urlTorrentRemoveData = "http://{0}:{1}/gui/?token={2}&action=removedata&hash={3}";
        private const string urlTorrentFilePriority = "http://{0}:{1}/gui/?token={2}&action=setprio&hash={3}&p={4}&f={5}";

        private System.Timers.Timer torrentsTimer = null;

        public delegate void ListRefreshedEventHandler(ListRefreshedEventArgs ev);
        public event ListRefreshedEventHandler ListRefreshedEvent;
        protected void OnListRefreshedEvent(ListRefreshedEventArgs ev)
        {
            if (ListRefreshedEvent != null)
            {
                ListRefreshedEvent(ev);
            }
        }

        public UTorrentHelper()
        {
            Initialised = false;
        }

        public void Init()
        {

            VM_UTorrentHelper.Instance.ConnectionStatus = "Populating security token...";
            PopulateToken();

            // timer for automatic updates
            torrentsTimer = new System.Timers.Timer();
            torrentsTimer.AutoReset = false;
            torrentsTimer.Interval = VM_UserSettings.Instance.UTorrentRefreshInterval * 1000; // 5 seconds
            torrentsTimer.Elapsed += new System.Timers.ElapsedEventHandler(torrentsTimer_Elapsed);

            if (ValidCredentials())
            {
                // get the intial list of completed torrents
                List<Torrent> torrents = new List<Torrent>();
                bool success = GetTorrentList(ref torrents);

                torrentsTimer.Start();
                Initialised = true;
            }
        }

        void torrentsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            try
            {
                if (!VM_UserSettings.Instance.UTorrentAutoRefresh) return;

                torrentsTimer.Stop();

                List<Torrent> torrents = new List<Torrent>();

                bool success = GetTorrentList(ref torrents);

                if (success)
                {
                    //OnListRefreshedEvent(new ListRefreshedEventArgs(torrents));
                    torrentsTimer.Interval = VM_UserSettings.Instance.UTorrentRefreshInterval * 1000;
                }
                else
                    torrentsTimer.Interval = 60 * 1000;

                torrentsTimer.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
                torrentsTimer.Start();
            }

        }

        public bool ValidCredentials()
        {
            if (AppSettings.UTorrentAddress.Trim().Length == 0) return false;
            if (AppSettings.UTorrentPort.Trim().Length == 0) return false;
            if (AppSettings.UTorrentUsername.Trim().Length == 0) return false;
            if (AppSettings.UTorrentPassword.Trim().Length == 0) return false;

            return true;
        }

        private void PopulateToken()
        {
            cookieJar = new CookieContainer();
            token = "";

            if (!ValidCredentials())
            {
                logger.Warn("Credentials are not valid for uTorrent");
                return;
            }

            string url = "";
            try
            {

                url = string.Format(urlTorrentTokenPage, AppSettings.UTorrentAddress, AppSettings.UTorrentPort);
                logger.Trace("token url: {0}", url);
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
                webReq.Timeout = 10000; // 10 seconds
                webReq.Credentials = new NetworkCredential(AppSettings.UTorrentUsername, AppSettings.UTorrentPassword);
                webReq.CookieContainer = cookieJar;

                HttpWebResponse WebResponse = (HttpWebResponse)webReq.GetResponse();


                Stream responseStream = WebResponse.GetResponseStream();
                StreamReader Reader = new StreamReader(responseStream, Encoding.UTF8);

                string output = Reader.ReadToEnd();
                logger.Trace("token reponse: {0}", output);

                WebResponse.Close();
                responseStream.Close();

                // parse and get the token
                // <html><div id='token' style='display:none;'>u3iiuDG4dwYDMzurIFif7FS-ldLPcvHk6QlB4y8LSKK5mX9GSPUZ_PpxD0s=</div></html>

                char q = (char)34;
                string quote = q.ToString();

                string torStart = "display:none;'>";
                string torEnd = "</div>";

                int posTorStart = output.IndexOf(torStart, 0);
                if (posTorStart <= 0) return;

                int posTorEnd = output.IndexOf(torEnd, posTorStart + torStart.Length + 1);

                token = output.Substring(posTorStart + torStart.Length, posTorEnd - posTorStart - torStart.Length);
                //BaseConfig.MyAnimeLog.Write("token: {0}", token);
            }
            catch (Exception ex)
            {
                logger.Error("Error in StartTorrent: {0} - {1}", url, ex.ToString());
                return;
            }
        }

        public void RemoveTorrent(string hash)
        {
            if (!ValidCredentials())
            {
                logger.Warn("Credentials are not valid for uTorrent");
                return;
            }

            try
            {
                string url = string.Format(urlTorrentRemove, AppSettings.UTorrentAddress, AppSettings.UTorrentPort, token, hash);
                string output = GetWebResponse(url);

                return;
            }
            catch (Exception ex)
            {
                logger.Error("Error in StartTorrent: {0}", ex.ToString());
                return;
            }
        }

        public void RemoveTorrentAndData(string hash)
        {
            if (!ValidCredentials())
            {
                logger.Warn("Credentials are not valid for uTorrent");
                return;
            }

            try
            {
                string url = string.Format(urlTorrentRemoveData, AppSettings.UTorrentAddress, AppSettings.UTorrentPort, token, hash);
                string output = GetWebResponse(url);

                return;
            }
            catch (Exception ex)
            {
                logger.Error("Error in StartTorrent: {0}", ex.ToString());
                return;
            }
        }

        public void AddTorrentFromURL(string downloadURL)
        {
            if (!ValidCredentials())
            {
                logger.Warn("Credentials are not valid for uTorrent");
                return;
            }

            try
            {
                string encodedURL = HttpUtility.UrlEncode(downloadURL);
                string url = string.Format(urlTorrentAddURL, AppSettings.UTorrentAddress, AppSettings.UTorrentPort, token, encodedURL);

                logger.Trace("Downloading: {0}", encodedURL);

                string output = GetWebResponse(url);

                return;
            }
            catch (Exception ex)
            {
                logger.Error("Error in AddTorrentFromURL: {0}", ex.ToString());
                return;
            }
        }

        public void StopTorrent(string hash)
        {
            if (!ValidCredentials())
            {
                logger.Warn("Credentials are not valid for uTorrent");
                return;
            }

            try
            {
                string url = string.Format(urlTorrentStop, AppSettings.UTorrentAddress, AppSettings.UTorrentPort, token, hash);
                string output = GetWebResponse(url);

                return;
            }
            catch (Exception ex)
            {
                logger.Error("Error in StartTorrent: {0}", ex.ToString());
                return;
            }
        }

        public void StartTorrent(string hash)
        {
            if (!ValidCredentials())
            {
                logger.Warn("Credentials are not valid for uTorrent");
                return;
            }

            try
            {
                string url = string.Format(urlTorrentStart, AppSettings.UTorrentAddress, AppSettings.UTorrentPort, token, hash);
                string output = GetWebResponse(url);

                return;
            }
            catch (Exception ex)
            {
                logger.Error("Error in StartTorrent: {0}", ex.ToString());
                return;
            }
        }

        public void PauseTorrent(string hash)
        {
            if (!ValidCredentials())
            {
                logger.Warn("Credentials are not valid for uTorrent");
                return;
            }

            try
            {
                string url = string.Format(urlTorrentPause, AppSettings.UTorrentAddress, AppSettings.UTorrentPort, token, hash);
                string output = GetWebResponse(url);

                return;
            }
            catch (Exception ex)
            {
                logger.Error("Error in StartTorrent: {0}", ex.ToString());
                return;
            }
        }

        private string GetWebResponse(string url)
        {
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
            webReq.Timeout = 15000; // 15 seconds
            webReq.Credentials = new NetworkCredential(AppSettings.UTorrentUsername, AppSettings.UTorrentPassword);
            webReq.CookieContainer = cookieJar;

            bool tryAgain = false;
            HttpWebResponse webResponse = null;
            try
            {
                webResponse = (HttpWebResponse)webReq.GetResponse();
            }
            catch (Exception ex)
            {
                logger.Trace("UTorrent:: GetWebResponse: {0}", ex.Message);
                if (ex.ToString().Contains("(400) Bad Request"))
                {
                    logger.Warn("UTorrent:: GetWebResponse 400 bad request, will try again...");
                    tryAgain = true;
                }
            }

            if (tryAgain)
            {
                PopulateToken();

                // fin the token in the url and replace it with the new one
                //http://{0}:{1}/gui/?token={2}&list=1
                int iStart = url.IndexOf(@"?token=", 0);
                int iFinish = url.IndexOf(@"&", 0);

                string prefix = url.Substring(0, iStart);
                string tokenStr = @"?token=" + token;
                string suffix = url.Substring(iFinish, url.Length - iFinish);

                logger.Trace("prefix: {0} --- tokenStr: {1} --- suffix: {2}", prefix, tokenStr, suffix);

                url = prefix + tokenStr + suffix;


                webReq = (HttpWebRequest)WebRequest.Create(url);
                webReq.Timeout = 15000; // 15 seconds
                webReq.Credentials = new NetworkCredential(AppSettings.UTorrentUsername, AppSettings.UTorrentPassword);
                webReq.CookieContainer = cookieJar;
                webResponse = (HttpWebResponse)webReq.GetResponse();
            }

            if (webResponse == null) return "";

            Stream responseStream = webResponse.GetResponseStream();
            StreamReader Reader = new StreamReader(responseStream, Encoding.UTF8);

            string output = Reader.ReadToEnd();

            webResponse.Close();
            responseStream.Close();

            return output;
        }

        public bool GetTorrentList(ref List<Torrent> torrents)
        {
            torrents = new List<Torrent>();

            if (!ValidCredentials())
            {
                logger.Warn("Credentials are not valid for uTorrent");
                return false;
            }

            string url = "";
            try
            {
                VM_UTorrentHelper.Instance.ConnectionStatus = "Getting torrent list...";
                //http://[IP]:[PORT]/gui/?list=1
                url = string.Format(urlTorrentList, AppSettings.UTorrentAddress, AppSettings.UTorrentPort, token);
                string output = GetWebResponse(url);
                if (output.Length == 0)
                {
                    VM_UTorrentHelper.Instance.ConnectionStatus = "Error!";
                    return false;
                }


                //BaseConfig.MyAnimeLog.Write("Torrent List JSON: {0}", output);
                TorrentList torList = JSONHelper.Deserialize<TorrentList>(output);

                foreach (object[] obj in torList.torrents)
                {
                    Torrent tor = new Torrent(obj);
                    torrents.Add(tor);
                }

                OnListRefreshedEvent(new ListRefreshedEventArgs(torrents));
                VM_UTorrentHelper.Instance.ConnectionStatus = "Connected.";
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetTorrentList: {0} - {1}", url, ex.ToString());
                return false;
            }
        }

        public bool GetFileList(string hash, ref List<TorrentFile> torFiles)
        {
            torFiles = new List<TorrentFile>();

            if (!ValidCredentials())
            {
                logger.Warn("Credentials are not valid for uTorrent");
                return false;
            }

            try
            {
                string url = string.Format(urlTorrentFileList, AppSettings.UTorrentAddress, AppSettings.UTorrentPort, token, hash);
                string output = GetWebResponse(url);
                if (output.Length == 0) return false;

                TorrentFileList fileList = JSONHelper.Deserialize<TorrentFileList>(output);

                if (fileList != null && fileList.files != null && fileList.files.Length > 1)
                {
                    object[] actualFiles = fileList.files[1] as object[];
                    if (actualFiles == null) return false;

                    foreach (object obj in actualFiles)
                    {
                        object[] actualFile = obj as object[];
                        if (actualFile == null) continue;

                        TorrentFile tf = new TorrentFile();
                        tf.FileName = actualFile[0].ToString();
                        tf.FileSize = long.Parse(actualFile[1].ToString());
                        tf.Downloaded = long.Parse(actualFile[2].ToString());
                        tf.Priority = long.Parse(actualFile[3].ToString());

                        torFiles.Add(tf);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetTorrentList: {0}", ex.ToString());
                return false;
            }
        }


        public void FileSetPriority(string hash, int idx, TorrentFilePriority priority)
        {
            if (!ValidCredentials())
            {
                logger.Warn("Credentials are not valid for uTorrent");
                return;
            }

            try
            {
                string url = string.Format(urlTorrentFilePriority, AppSettings.UTorrentAddress, AppSettings.UTorrentPort, token, hash, (int)priority, idx);
                string output = GetWebResponse(url);

                return;
            }
            catch (Exception ex)
            {
                logger.Error("Error in StartTorrent: {0}", ex.ToString());
                return;
            }
        }
    }
}
