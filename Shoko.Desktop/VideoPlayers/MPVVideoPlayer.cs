using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using Shoko.Desktop.VideoPlayers.mpv;

namespace Shoko.Desktop.VideoPlayers
{
    public class MPVVideoPlayer : IVideoPlayer
    {
        private const int MpvFormatString = 1;
        private IntPtr _libMpvDll;
        private IntPtr _mpvHandle;
        private System.Timers.Timer timeTimer = null;
        private VideoInfo info = null;
        private PlayerForm form;

        [Flags]
        internal enum LoadLibraryFlags : uint
        {
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
        }

        [DllImport("kernel32.dll")]
        internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MpvCreate();
        private MpvCreate _mpvCreate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvInitialize(IntPtr mpvHandle);
        private MpvInitialize _mpvInitialize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvCommand(IntPtr mpvHandle, IntPtr strings);
        private MpvCommand _mpvCommand;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvTerminateDestroy(IntPtr mpvHandle);
        private MpvTerminateDestroy _mpvTerminateDestroy;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetOption(IntPtr mpvHandle, byte[] name, int format, ref long data);
        private MpvSetOption _mpvSetOption;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetOptionString(IntPtr mpvHandle, byte[] name, byte[] value);
        private MpvSetOptionString _mpvSetOptionString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvGetPropertystring(IntPtr mpvHandle, byte[] name, int format, ref IntPtr data);
        private MpvGetPropertystring _mpvGetPropertyString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetProperty(IntPtr mpvHandle, byte[] name, int format, ref byte[] data);
        private MpvSetProperty _mpvSetProperty;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MpvFree(IntPtr data);
        private MpvFree _mpvFree;


        internal void StartWatcher()
        {
            StopWatcher();
            timeTimer = new System.Timers.Timer();
            timeTimer.Elapsed += TimeTimer_Elapsed;
            timeTimer.Interval = 1000;
            timeTimer.Enabled = true;
        }

        private void TimeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            info.ChangePosition((long)(Time * 100));
            if (Width == 0 && Height == 0)
            {
                string w = GetProperty("width");
                int wd = 0;
                if (!string.IsNullOrEmpty(w))
                {
                    int.TryParse(w, out wd);
                    if (wd == 0)
                        return;
                }
                else
                    return;
                int hg = 0;
                string h = GetProperty("height");
                if (!string.IsNullOrEmpty(h))
                {
                    int.TryParse(h, out hg);
                    if (hg == 0)
                        return;
                }
                else
                    return;
                Width = wd;
                Height = hg;
                form?.Resize(wd, hg);
            }
        }
        public int Width { get; private set; }
        public int Height { get; private set; }
        internal void StopWatcher()
        {
            if (timeTimer != null)
            {
                timeTimer.Stop();
                timeTimer.Dispose();
                timeTimer = null;
            }
        }

        private object GetDllType(Type type, string name)
        {
            IntPtr address = GetProcAddress(_libMpvDll, name);
            if (address != IntPtr.Zero)
                return Marshal.GetDelegateForFunctionPointer(address, type);
            return null;
        }

        public void Play(VideoInfo video)
        {
            Width = 0;
            Height = 0;
            info = video;
            Initialize();
           form =new PlayerForm(this);
            form.Show();
            form.Play(video);
        }

        public void Init()
        {
            Active = true;
        }

        public void Initialize()
        {
            if (_mpvHandle != IntPtr.Zero)
                _mpvTerminateDestroy(_mpvHandle);

            string fullexepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo fi = new FileInfo(fullexepath);
            fullexepath = Path.Combine(fi.Directory.FullName, Environment.Is64BitProcess ? "x64" : "x86", "mpv-1.dll");
            _libMpvDll = LoadLibraryEx(fullexepath, IntPtr.Zero, 0);
            _mpvCreate = (MpvCreate)GetDllType(typeof(MpvCreate), "mpv_create");
            _mpvInitialize = (MpvInitialize)GetDllType(typeof(MpvInitialize), "mpv_initialize");
            _mpvTerminateDestroy = (MpvTerminateDestroy)GetDllType(typeof(MpvTerminateDestroy), "mpv_terminate_destroy");
            _mpvCommand = (MpvCommand)GetDllType(typeof(MpvCommand), "mpv_command");
            _mpvSetOption = (MpvSetOption)GetDllType(typeof(MpvSetOption), "mpv_set_option");
            _mpvSetOptionString = (MpvSetOptionString)GetDllType(typeof(MpvSetOptionString), "mpv_set_option_string");
            _mpvGetPropertyString = (MpvGetPropertystring)GetDllType(typeof(MpvGetPropertystring), "mpv_get_property");
            _mpvSetProperty = (MpvSetProperty)GetDllType(typeof(MpvSetProperty), "mpv_set_property");
            _mpvFree = (MpvFree)GetDllType(typeof(MpvFree), "mpv_free");

            if (_libMpvDll == IntPtr.Zero)
                return;

            _mpvHandle = _mpvCreate.Invoke();
            if (_mpvHandle == IntPtr.Zero)
                return;

            _mpvInitialize.Invoke(_mpvHandle);
        }
        private static byte[] GetUtf8Bytes(string s)
        {
            return Encoding.UTF8.GetBytes(s + "\0");
        }
        public bool Active { get; set;  }
        public Enums.VideoPlayer Player => Enums.VideoPlayer.MPV;
        public event BaseVideoPlayer.FilesPositionsHandler FilePositionsChange;
        public event BaseVideoPlayer.FilePositionHandler VideoInfoChange;

        public void Stop()
        {
            Pause();
            info.ForceChange((long)(Time * 1000));
            VideoInfoChange?.Invoke(info, (long) (Time*1000));
            Time = 0;
            StopWatcher();
            if (info != null)
                BaseVideoPlayer.PlaybackStopped(info, (long) Time);
        }

        private int prevvolume = 0;

        public void ProcessKey(KeyEventArgs e)
        {
            if ((e.Key == Key.Space) || (e.Key == Key.MediaPlayPause))
            {
                if (IsPaused)
                    Play();
                else
                    Pause();
            }
            else if (e.Key == Key.Play)
            {
                Play();
            }
            else if (e.Key == Key.Pause)
            {
                Pause();
            }
            else if ((e.Key == Key.Left) || (e.Key==Key.BrowserBack))
            {
                double time = Time;
                time -= 15;
                if (time < 0)
                    time = 0;
                Time = time;
            }
            else if ((e.Key == Key.Right) || (e.Key==Key.BrowserForward))
            {
                Time += 15; //Todo Check against duration
            }
            else if (e.Key == Key.D0)
            {
                Volume = 0;
            }
            else if (e.Key == Key.D1)
            {
                Volume = 11;
            }
            else if (e.Key == Key.D2)
            {
                Volume = 22;
            }
            else if (e.Key == Key.D3)
            {
                Volume = 33;
            }
            else if (e.Key == Key.D4)
            {
                Volume =44;
            }
            else if (e.Key == Key.D5)
            {
                Volume = 55;
            }
            else if (e.Key == Key.D6)
            {
                Volume = 66;
            }
            else if (e.Key == Key.D7)
            {
                Volume = 77;
            }
            else if (e.Key == Key.D8)
            {
                Volume = 88;
            }
            else if (e.Key == Key.D9)
            {
                Volume = 100;
            }
            else if ((e.Key == Key.VolumeUp) || (e.Key==Key.OemPlus))
            {
                int val = Volume;
                val += 5;
                if (val > 100)
                    val = 100;
                Volume = val;
            }
            else if ((e.Key == Key.VolumeDown) || (e.Key == Key.OemMinus))
            {
                int val = Volume;
                val -= 5;
                if (val <0)
                    val = 0;
                Volume = val;
            }
            else if ((e.Key == Key.VolumeMute))
            {
                if (Volume > 0)
                {
                    prevvolume = Volume;
                    Volume = 0;
                }
                else
                {
                    Volume = prevvolume;
                }
            }
        }
        public void Quit()
        {
            StopWatcher();
            info.ForceChange((long)(Time * 1000));
            VideoInfoChange?.Invoke(info, (long)(Time * 1000));
            if (_mpvHandle != IntPtr.Zero)
            {
                _mpvTerminateDestroy(_mpvHandle);
                _mpvHandle = IntPtr.Zero;
            }
            form = null;
        }

        public bool IsPaused { get; private set; } = false;
        public void Pause()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            var bytes = GetUtf8Bytes("yes");
            _mpvSetProperty(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
            IsPaused = true;
        }

        public void Play()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            var bytes = GetUtf8Bytes("no");
            _mpvSetProperty(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
            IsPaused = false;
            StartWatcher();
        }
        /*
        public bool IsPaused()
        {
            if (_mpvHandle == IntPtr.Zero)
                return true;

            var lpBuffer = IntPtr.Zero;
            _mpvGetPropertyString(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref lpBuffer);
            var isPaused = Marshal.PtrToStringAnsi(lpBuffer) == "yes";
            _mpvFree(lpBuffer);
            return isPaused;
        }
        */

        public void SetProperty(string property, string value)
        {
            if (_mpvHandle == IntPtr.Zero)
                return;
            var bytes = GetUtf8Bytes(value);
            _mpvSetProperty(_mpvHandle, GetUtf8Bytes(property), MpvFormatString, ref bytes);
        }
        public string GetProperty(string property)
        {
            if (_mpvHandle == IntPtr.Zero)
                return null;

            var lpBuffer = IntPtr.Zero;
            _mpvGetPropertyString(_mpvHandle, GetUtf8Bytes(property), MpvFormatString, ref lpBuffer);
            string ret = Marshal.PtrToStringAnsi(lpBuffer);
            _mpvFree(lpBuffer);
            return ret;
        }

        public int GetIntProperty(string property)
        {
            string r = GetProperty(property);
            int result;
            if (int.TryParse(r, out result))
                return result;
            return 0;
        }
        public double GetDoubleProperty(string property)
        {
            string r = GetProperty(property);
            double result;
            if (double.TryParse(r, out result))
                return result;
            return 0;
        }
        public double Time
        {
            get
            {
                return GetDoubleProperty("time-pos");
            }
            set
            {
                if (_mpvHandle == IntPtr.Zero)
                    return;
                DoMpvCommand("seek", value.ToString(CultureInfo.InvariantCulture), "absolute");
            }
        }

        public int Volume
        {
            get
            {
                return GetIntProperty("volume");
            }
            set
            {
                SetProperty("volume",value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static IntPtr AllocateUtf8IntPtrArrayWithSentinel(string[] arr, out IntPtr[] byteArrayPointers)
        {
            int numberOfStrings = arr.Length + 1; // add extra element for extra null pointer last (sentinel)
            byteArrayPointers = new IntPtr[numberOfStrings];
            IntPtr rootPointer = Marshal.AllocCoTaskMem(IntPtr.Size * numberOfStrings);
            for (int index = 0; index < arr.Length; index++)
            {
                var bytes = GetUtf8Bytes(arr[index]);
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
                byteArrayPointers[index] = unmanagedPointer;
            }
            Marshal.Copy(byteArrayPointers, 0, rootPointer, numberOfStrings);
            return rootPointer;
        }

        public void PlayUrl(string url)
        {
            DoMpvCommand("loadfile", url);
            StartWatcher();
        }

        public void AddSubtitle(string subtitlefile)
        {
            DoMpvCommand("sub-add", subtitlefile );

        }

        public void PlayPlaylist(string playlistfile)
        {
            DoMpvCommand("loadlist", playlistfile);
            StartWatcher();

        }

        public void SetWindowsHandle(IntPtr handle)
        {
            _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("keep-open"), GetUtf8Bytes("always"));
            int mpvFormatInt64 = 4;
            var windowId =handle.ToInt64();
            _mpvSetOption(_mpvHandle, GetUtf8Bytes("wid"), mpvFormatInt64, ref windowId);
        }
        private void DoMpvCommand(params string[] args)
        {
            IntPtr[] byteArrayPointers;
            var mainPtr = AllocateUtf8IntPtrArrayWithSentinel(args, out byteArrayPointers);
            _mpvCommand(_mpvHandle, mainPtr);
            foreach (var ptr in byteArrayPointers)
            {
                Marshal.FreeHGlobal(ptr);
            }
            Marshal.FreeHGlobal(mainPtr);
        }



    }
}
