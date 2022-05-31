using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;

namespace OVR_Dash_Manager
{
    public static class SteamVR
    {
        private static bool _Steam_Installed;
        private static String _SteamDirectory;
        private static bool _SteamVR_Installed;
        private static bool _SteamVR_Running;

        private static String _SteamVRDirectory;

        public static Boolean ManagerCalledExit = false;

        public static bool Steam_Installed
        {
            get { return _Steam_Installed; }
            private set { _Steam_Installed = value; }
        }

        public static String SteamDirectory
        {
            get { return _SteamDirectory; }
            private set { _SteamDirectory = value; }
        }

        public static bool SteamVR_Installed
        {
            get { return _SteamVR_Installed; }
            private set { _SteamVR_Installed = value; }
        }

        public static bool SteamVR_Running
        {
            get { return _SteamVR_Running; }
            private set { _SteamVR_Running = value; }
        }

        public static String SteamVRDirectory
        {
            get { return _SteamVRDirectory; }
            private set { _SteamVRDirectory = value; }
        }

        public static void CheckInstalled()
        {
            string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string OpenVR = Path.Combine(LocalAppData, "openvr\\openvrpaths.vrpath");
            if (File.Exists(OpenVR))
            {
                String JSON = File.ReadAllText(OpenVR);
                if (JSON.Contains("config") || JSON.Contains("runtime"))
                {
                    try
                    {
                        OpenVR_Stripped Config = JsonConvert.DeserializeObject<OpenVR_Stripped>(JSON);
                        if (Config != null)
                        {
                            _SteamDirectory = Config.config.FirstOrDefault();
                            _SteamVRDirectory = Config.runtime.FirstOrDefault();

                            if (!String.IsNullOrEmpty(_SteamDirectory))
                                _SteamDirectory = Functions.RemoveStringFromEnd(_SteamDirectory, @"\\config");
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            if (!String.IsNullOrEmpty(_SteamDirectory))
            {
                if (!Directory.Exists(_SteamDirectory))
                    _SteamDirectory = String.Empty;
                else
                    _Steam_Installed = true;
            }

            if (!String.IsNullOrEmpty(_SteamVRDirectory))
            {
                if (!Directory.Exists(_SteamVRDirectory))
                    _SteamVRDirectory = String.Empty;
                else
                    _SteamVR_Installed = true;
            }
        }

        public static void Dispose()
        {
            Timer_Functions.DisposeTimer("SteamVR Checker");
        }

        public static void Setup()
        {
            CheckInstalled();
            Timer_Functions.CreateTimer("SteamVR Checker", TimeSpan.FromSeconds(5), CheckSteamVR);
        }

        public static void StartTimer()
        {
            Timer_Functions.StartTimer("SteamVR Checker");
        }

        public static void StopTimer()
        {
            Timer_Functions.StopTimer("SteamVR Checker");
        }

        private static int ID_Hooked;
        private static void CheckSteamVR(object sender, ElapsedEventArgs args)
        {
            Process[] SteamVR = Process.GetProcessesByName("vrserver");
            if (SteamVR.Length > 0)
            {
                SteamVR_Running = true;
                if (ID_Hooked != SteamVR[0].Id)
                {
                    ID_Hooked = SteamVR[0].Id;

                    Thread Thread_WaitForExit = new Thread(WaitForExit);
                    Thread_WaitForExit.IsBackground = true;
                    Thread_WaitForExit.Start(SteamVR[0]);
                }
            }
            else
                SteamVR_Running = false;
        }

        static Process VRServer;
        private static void WaitForExit(object oProcess)
        {
            VRServer = (Process)oProcess;
            VRServer.WaitForExit();
            Debug.WriteLine("Steam VR Closed");

            if (!ManagerCalledExit)
            {
                Debug.WriteLine("Resetting Link");
                Oculus_Software.ResetLink();
            }

            ManagerCalledExit = false;
        }

        public static void FocusSteamVR()
        {
            if (VRServer != null)
            {
                if (!VRServer.HasExited)
                {
                    Process[] vrmonitor = Process.GetProcessesByName("vrmonitor");

                    if (vrmonitor.Length == 1)
                    {
                        if (vrmonitor[0].MainWindowHandle != IntPtr.Zero)
                        {
                            Functions.BringWindowToTop(vrmonitor[0].MainWindowHandle);
                            Functions.SetForegroundWindow(vrmonitor[0].MainWindowHandle);
                            Functions.SetFocus(vrmonitor[0].MainWindowHandle);
                        }
                    }
                }
            }
        }
    }

    internal class OpenVR_Stripped
    {
        public List<string> config { get; set; }
        public List<string> runtime { get; set; }
    }
}