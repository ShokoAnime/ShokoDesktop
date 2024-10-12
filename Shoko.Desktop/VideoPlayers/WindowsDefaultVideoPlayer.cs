using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Shoko.Desktop.Enums;

namespace Shoko.Desktop.VideoPlayers
{
    public class WindowsDefaultVideoPlayer : BaseVideoPlayer, IVideoPlayer
    {
        [Flags]
        enum AssocF : uint
        {
            None = 0,
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200,
            Init_IgnoreUnknown = 0x400,
            Init_FixedProgId = 0x800,
            IsProtocol = 0x1000,
            InitForFile = 0x2000,
        }

        enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic,
            InfoTip,
            QuickTip,
            TileInfo,
            ContentType,
            DefaultIcon,
            ShellExtension,
            DropTarget,
            DelegateExecute,
            SupportedUriProtocols,
            // The values below ('Max' excluded) have been introduced in W10 1511
            ProgID,
            AppID,
            AppPublisher,
            AppIconReference,
            Max
        }

        [DllImport("Shlwapi.dll", SetLastError=true, CharSet = CharSet.Auto)]
        static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);

        static string AssocQueryString(string extension)
        {
            uint length = 0;
            var ret = AssocQueryString(AssocF.None, AssocStr.Command, extension, null, null, ref length);
            if (ret != 1) // expected S_FALSE
                throw new InvalidOperationException(
                    "Could not determine associated string, unable to get the required buffer length. Error code: " +
                    ret);

            var sb = new StringBuilder((int)length); // (length-1) will probably work too as null termination is added
            ret = AssocQueryString(AssocF.None, AssocStr.Command, extension, null, sb, ref length);
            if (ret != 0) // expected S_OK
                throw new InvalidOperationException("Could not determine associated string. Error code: " + ret);

            return sb.ToString();
        }

        public override void Play(VideoInfo video)
        {
            if (IsPlaying)
                return;
            Task.Factory.StartNew(() =>
            {
                // format of "C:\Program Files\..." --option-thing
                var exe = AssocQueryString(Path.GetExtension(video.Uri));
                // pull the executable out
                string[] args;
                string path;
                if (exe.StartsWith("\""))
                {
                    args = exe.Split("\"").Skip(1).ToArray();
                    path = args.FirstOrDefault();
                    // recombine to preserve the quotes in arguments
                    args = string.Join("\"", args.Skip(1)).Split(" ",
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    args = exe.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    path = args.FirstOrDefault();
                }

                var startInfo = new ProcessStartInfo(path) { UseShellExecute = true };
                // ArgumentList is apparently a pita to pass a list to
                foreach(var arg in args) startInfo.ArgumentList.Add(arg);
                startInfo.ArgumentList.Add(video.Uri);
                var process = Process.Start(startInfo);
                process?.WaitForExit();
                StopWatcher();
                IsPlaying = false;
            });
        }

        internal override void FileChangeEvent(string filePath)
        {
            // Not used
        }

        public override void Init()
        {
            Active = true;

        }

        public VideoPlayer Player => VideoPlayer.WindowsDefault;
    }
}
