using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;

namespace OVR_Dash_Manager.Software
{
    public static class Steam
    {
        public static bool Steam_Installed { get; private set; }
        public static bool Steam_VR_Installed { get; private set; }
        public static string Steam_Directory { get; private set; }
        public static string Steam_VR_Directory { get; private set; }
        public static bool Steam_Running { get; private set; }
        public static bool Steam_VR_Server_Running { get; private set; }

        private delegate void Steam_Running_State_Changed();

        private static event Steam_Running_State_Changed Steam_Running_State_Changed_Event;

        public static bool Steam_VR_Monitor_Running { get; private set; }

        public delegate void Steam_VR_Running_State_Changed();

        public static event Steam_VR_Running_State_Changed Steam_VR_Running_State_Changed_Event;

        public static Boolean ManagerCalledExit = false;

        private static bool _IsSetup = false;
        public static void Setup()
        {
            if (_IsSetup)
                return;

            _IsSetup = true;

            CheckInstalled();

            Steam_VR_Running_State_Changed_Event += Steam_Steam_VR_Running_State_Changed_Event;
            Functions.Process_Watcher.ProcessStarted += Process_Watcher_ProcessStarted;
            Functions.Process_Watcher.ProcessExited += Process_Watcher_ProcessExited;

            Timer_Functions.CreateTimer("SteamVR Focus Fix", TimeSpan.FromSeconds(1), Check_SteamVR_FocusProblem);

            List<String> ProcessToCheck = new List<string> { "steam", "vrserver", "vrmonitor" };

            foreach (String Check in ProcessToCheck)
            {
                Process[] CheckThese = Process.GetProcessesByName(Check);
                if (CheckThese.Length > 0)
                    Set_Running_State($"{Check}.exe", true);
            }
        }

        private static void Steam_Steam_VR_Running_State_Changed_Event()
        {
            if (!Steam_VR_Server_Running)
            {
                if (!ManagerCalledExit)
                {
                    if (Properties.Settings.Default.ExitLinkOn_UserExit_SteamVR)
                    {
                        Close_SteamVR_ResetLink();
                    }
                }
            }

            ManagerCalledExit = false;
        }

        public static void Close_SteamVR_ResetLink()
        {
            Close_SteamVR_Server();

            Software.Oculus_Link.StopLink();
            Close_SteamVR_Server();

            Thread pInAMoment = new Thread(StartLinkInAMoment);
            pInAMoment.Start();
        }

        private static void StartLinkInAMoment()
        {
            Thread.Sleep(2000);
            ManagerCalledExit = true;
            Close_SteamVR_Server();
            Software.Oculus_Link.StartLink();
            Thread.Sleep(2000);
            ManagerCalledExit = true;
            Close_SteamVR_Server();
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
                            Steam_Directory = Config.config.FirstOrDefault();
                            Steam_VR_Directory = Config.runtime.FirstOrDefault();

                            if (!String.IsNullOrEmpty(Steam_Directory))
                                Steam_Directory = Functions_Old.RemoveStringFromEnd(Steam_Directory, @"\\config");
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            if (!String.IsNullOrEmpty(Steam_Directory))
            {
                if (!Directory.Exists(Steam_Directory))
                    Steam_Directory = String.Empty;
                else
                    Steam_Installed = true;
            }

            if (!String.IsNullOrEmpty(Steam_VR_Directory))
            {
                if (!Directory.Exists(Steam_VR_Directory))
                    Steam_VR_Directory = String.Empty;
                else
                    Steam_VR_Installed = true;
            }
        }

        private static void Process_Watcher_ProcessStarted(string pProcessName, int pProcessID)
        {
            Set_Running_State(pProcessName, true);
        }

        private static void Process_Watcher_ProcessExited(string pProcessName, int pProcessID)
        {
            Set_Running_State(pProcessName, false);
        }

        private static void Set_Running_State(String ProcessName, Boolean State)
        {
            switch (ProcessName)
            {
                case "steam.exe":
                    Steam_Running = State;
                    Steam_Running_State_Changed_Event?.Invoke();
                    break;
                case "vrserver.exe":
                    Steam_VR_Server_Running = State;
                    Steam_VR_Running_State_Changed_Event?.Invoke();
                    break;
                case "vrmonitor.exe":
                    Steam_VR_Monitor_Running = State;
                    break;
                default:
                    break;
            }
        }

        #region SteamVR Focus Fix
        // Janky
        private static void Check_SteamVR_FocusProblem(object sender, ElapsedEventArgs args)
        {
            if (Steam_VR_Server_Running)
            {
                if (Properties.Settings.Default.SteamVRFocusFix)
                {
                    switch (Functions_Old.GetActiveWindowTitle())
                    {
                        case "Task View":
                            Dashes.Dash_Manager.MainForm_FixTaskViewIssue();
                            Focus_Steam_VR_Monitor_Window();
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public static void Focus_Steam_VR_Monitor_Window()
        {
            if (Steam_VR_Server_Running)
            {
                Process[] vrmonitor = Process.GetProcessesByName("vrmonitor");
                if (vrmonitor.Length == 1)
                {
                    if (vrmonitor[0].MainWindowHandle != IntPtr.Zero)
                    {
                        Functions_Old.BringWindowToTop(vrmonitor[0].MainWindowHandle);
                        Functions_Old.SetForegroundWindow(vrmonitor[0].MainWindowHandle);
                        Functions_Old.SetFocus(vrmonitor[0].MainWindowHandle);
                    }
                }
            }
        }
        #endregion

        public static void Close_SteamVR_Server()
        {
            if (Steam_VR_Server_Running)
            {
                Process[] vrServer = Process.GetProcessesByName("vrserver");

                if (vrServer.Length == 1)
                    vrServer[0].Kill();
            }

            CloseSteamVRMonitor();
        }

        private static void CloseSteamVRMonitor()
        {
            Process[] vrmonitor = Process.GetProcessesByName("vrmonitor");
            if (vrmonitor.Length == 1)
            {
                if (vrmonitor[0].MainWindowHandle != IntPtr.Zero)
                {
                    try
                    {
                        vrmonitor[0].CloseMainWindow();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            vrmonitor[0].Kill();
                        }
                        catch (Exception)
                        {
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

    public static class Steam_VR_Settings
    {
        public static OpenXR_Runtime Current_Open_XR_Runtime { get; private set; }

        public static void Set_SteamVR_Runtime()
        {
            Run_RemoveUsbHelper_Action("setopenxrruntime");
        }

        public static void Set_Disable_USB_PowerManagement()
        {
            Run_RemoveUsbHelper_Action("disableenhancepowermanagement");
        }

        private static void Run_RemoveUsbHelper_Action(String Action)
        {
            if (Steam.Steam_VR_Installed)
            {
                if (Directory.Exists(Steam.Steam_VR_Directory))
                {
                    String Helper = Path.Combine(Steam.Steam_VR_Directory, "bin\\win32\\removeusbhelper.exe");
                    if (File.Exists(Helper))
                        Process.Start(Helper, Action);
                }
            }
        }

        public static OpenXR_Runtime Read_Runtime()
        {
            String OculusRunTimePath = Functions.Registry_Functions.GetKeyValue_String(RegistryKey_Type.LocalMachine, @"SOFTWARE\Khronos\OpenXR\1", "ActiveRuntime");

            if (OculusRunTimePath.Contains("oculus-runtime\\oculus_openxr_64.json"))
                Current_Open_XR_Runtime = OpenXR_Runtime.Oculus;
            else if (OculusRunTimePath.Contains("SteamVR\\steamxr_win64.json"))
                Current_Open_XR_Runtime = OpenXR_Runtime.SteamVR;
            else if (OculusRunTimePath.Contains("oculus-runtime\\oculus_openxr_32.json"))
                Current_Open_XR_Runtime = OpenXR_Runtime.Oculus;
            else if (OculusRunTimePath.Contains("SteamVR\\steamxr_win32.json"))
                Current_Open_XR_Runtime = OpenXR_Runtime.SteamVR;
            else
                Current_Open_XR_Runtime = OpenXR_Runtime.Unknown;

            return Current_Open_XR_Runtime;
        }

        public enum OpenXR_Runtime
        {
            Unknown = -1,
            Oculus = 0,
            SteamVR = 1
        }
    }
}
