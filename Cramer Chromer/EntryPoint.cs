using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Cramer_Chromer
{
    class EntryPoint
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(System.IntPtr hWnd, ShowWindowEnum flags);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(System.IntPtr hWnd);

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };

        private static Process LatchChrome()
        {
            Process[] localByName = Process.GetProcessesByName("chrome");
            if (localByName.Length > 0)
            {
                foreach (Process p in localByName)
                {
                    if (p.MainWindowHandle != System.IntPtr.Zero)
                    {
                        return p;
                    }
                }
            }
            return null;
        }

        private static Process StartAndLatch(Process p)
        {
            p.StartInfo.FileName = "chrome.exe";
            p.StartInfo.Arguments = "https://123news.tv/cnbc-live-stream/";
            p.Start();

            Process temp = LatchChrome();
            if (temp == null)
            {
                return p;
            }

            p = temp;
            return p;
        }

        static void Main(string[] args)
        {
            //Check if an existing chromer is already running, throw a warning in your face, and exit early.
            if (Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
            {
                MessageBox.Show("An instance of Cramer Chromer is already running on this PC", "Duplicate Process",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            System.TimeSpan interval = new System.TimeSpan(0, 0, 50);
            Process process = new Process();

            while (true)
            {
                int remaining = MinutesRemaining();

                if (remaining == 10)
                {
                    process = StartAndLatch(process);
                }
                else if (remaining == 1)
                {
                    try
                    {
                        // The window is hidden so try to restore it before setting focus.
                        ShowWindow(process.MainWindowHandle, ShowWindowEnum.Maximize);
                        SetForegroundWindow(process.MainWindowHandle);
                    }
                    catch (System.InvalidOperationException)
                    {
                        // Process doesn't exist so restart it.
                        process = StartAndLatch(process);
                    }
                }

                System.Threading.Thread.Sleep(interval);
            }
        }

        private static int MinutesRemaining()
        {
            System.DateTime currentTime = System.DateTime.UtcNow;
            //Half Time Report (16:00 UTC/12:00 EST) or Mad Money (22:00 UTC/18:00 EST) 
            if (currentTime.Hour == 15 || currentTime.Hour == 21)
            {
                return (60 - currentTime.Minute);
            }
            return 1000;
        }
    }
}
