using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows;

namespace OVR_Dash_Manager.Software
{
    public static class Oculus
    {
        private static string _Oculus_Main_Directory;
        public static string Oculus_Main_Directory
        {
            get { return _Oculus_Main_Directory; }
            private set { _Oculus_Main_Directory = value; }
        }

        private static string _Oculus_Dash_Directory;
        public static string Oculus_Dash_Directory
        {
            get { return _Oculus_Dash_Directory; }
            private set { _Oculus_Dash_Directory = value; }
        }

        private static string _Oculus_Dash_File;
        public static string Oculus_Dash_File
        {
            get { return _Oculus_Dash_File; }
            private set { _Oculus_Dash_File = value; }
        }

        private static string _Oculus_Client_EXE;
        public static string Oculus_Client_EXE
        {
            get { return _Oculus_Client_EXE; }
            private set { _Oculus_Client_EXE = value; }
        }

        private static string _Oculus_DebugTool_EXE;
        public static string Oculus_DebugTool_EXE
        {
            get { return _Oculus_DebugTool_EXE; }
            private set { _Oculus_DebugTool_EXE = value; }
        }

        private static bool _Oculus_Is_Installed;
        public static bool Oculus_Is_Installed
        {
            get { return _Oculus_Is_Installed; }
            private set { _Oculus_Is_Installed = value; }
        }

        private static bool _Normal_Dash;
        public static bool Normal_Dash
        {
            get { return _Normal_Dash; }
            private set { _Normal_Dash = value; }
        }

        private static bool _Custom_Dash;
        public static bool Custom_Dash
        {
            get { return _Custom_Dash; }
            private set { _Custom_Dash = value; }
        }

        private static String _Current_Dash_Name;
        public static String Current_Dash_Name
        {
            get { return _Current_Dash_Name; }
            private set { _Current_Dash_Name = value; }
        }

        private static bool _ClientJustExited = false;
        private static bool _Report_ClientJustExited = false;

        private static bool _IsSetup = false;
        public static void Setup()
        {
            if (!_IsSetup)
            {
                _IsSetup = true;

                Functions.Process_Watcher.ProcessStarted += Process_Watcher_ProcessStarted;
                Functions.Process_Watcher.ProcessExited += Process_Watcher_ProcessExited;
            }
        }

        private static void Process_Watcher_ProcessStarted(string pProcessName, int pProcessID)
        {
            Debug.WriteLine($"Started: {pProcessName} - {DateTime.Now}");
            switch (pProcessName)
            {
                case "OculusClient.exe":
                    break;
                default:
                    break;
            }
        }

        private static void Process_Watcher_ProcessExited(string pProcessName, int pProcessID)
        {
            Debug.WriteLine($"Stopped: {pProcessName} - {DateTime.Now}");
            switch (pProcessName)
            {
                case "OculusClient.exe":
                    if (_Report_ClientJustExited)
                    {
                        Debug.WriteLine("Set Client Minimize Exit Trigger");
                        _ClientJustExited = true;
                        _Report_ClientJustExited = false;
                    }
                    break;
                default:
                    break;
            }
        }

        public static void Check_Oculus_Is_Installed()
        {
            String OculusPath = Environment.GetEnvironmentVariable("OculusBase");

            if (Directory.Exists(OculusPath))
            {
                _Oculus_Main_Directory = OculusPath;
                _Oculus_Dash_Directory = Path.Combine(OculusPath, @"Support\oculus-dash\dash\bin");
                _Oculus_Client_EXE = Path.Combine(OculusPath, @"Support\oculus-client\OculusClient.exe");
                _Oculus_DebugTool_EXE = Path.Combine(OculusPath, @"Support\oculus-diagnostics\OculusDebugTool.exe");
                _Oculus_Dash_File = Path.Combine(_Oculus_Dash_Directory, @"OculusDash.exe");

                if (File.Exists(_Oculus_Client_EXE))
                    _Oculus_Is_Installed = true;
            }
        }

        public static void Check_Current_Dash()
        {
            if (_Oculus_Is_Installed)
            {
                if (File.Exists(_Oculus_Dash_File))
                    WhichDash(_Oculus_Dash_File);
            }
        }

        private static void WhichDash(String FilePath)
        {
            if (File.Exists(FilePath))
            {
                _Normal_Dash = false;
                _Custom_Dash = false;
                _Current_Dash_Name = "Checking";

                FileVersionInfo Info = FileVersionInfo.GetVersionInfo(FilePath);
                Dashes.Dash_Type Current = Dashes.Dash_Manager.CheckWhosDash(Info.ProductName);
                _Current_Dash_Name = Dashes.Dash_Manager.GetDashName(Current);

                if (_Current_Dash_Name != Dashes.Dash_Manager.GetDashName(Dashes.Dash_Type.Normal))
                    _Custom_Dash = true;
                else
                {
                    if (Check_Is_OfficalDash(FilePath))
                        _Normal_Dash = true;
                    else
                    {
                        _Custom_Dash = true;
                        _Current_Dash_Name = "Unknown";
                    }
                }
            }
        }

        private static bool Check_Is_OfficalDash(String FilePath)
        {
            Boolean Legit = false;

            X509Certificate cert = X509Certificate.CreateFromSignedFile(FilePath);

            if (cert.Issuer == "CN=DigiCert SHA2 Assured ID Code Signing CA, OU=www.digicert.com, O=DigiCert Inc, C=US")
                Legit = true;

            return Legit;
        }

        public static void StartOculusClient()
        {
            if (Debugger.IsAttached)
            {
                if (!Dashes.Dash_Manager.EmulateReleaseMode())
                    return;
            }

            if (File.Exists(_Oculus_Client_EXE))
            {
                Process[] Dashes = Process.GetProcessesByName("OculusClient");

                if (Dashes.Length == 0)
                {
                    if (Service_Manager.GetState("OVRService") != "Running")
                    {
                        if (File.Exists(Path.Combine(Oculus_Main_Directory, "Support\\oculus-runtime\\OVRServiceLauncher.exe")))
                        {
                            Process ServiceLauncher = Process.Start(Path.Combine(Oculus_Main_Directory, "Support\\oculus-runtime\\OVRServiceLauncher.exe"), "-start");
                            ServiceLauncher.WaitForExit();

                            for (int i = 0; i < 100; i++)
                            {
                                Thread.Sleep(1000);
                                Process[] Redir = Process.GetProcessesByName("OVRRedir");
                                if (Redir.Length > 0)
                                {
                                    Debug.WriteLine("OVRRedir Started");
                                    Thread.Sleep(2000);
                                    break;
                                }
                            }
                        }
                    }

                    ProcessStartInfo Info = new ProcessStartInfo { WorkingDirectory = Path.GetDirectoryName(_Oculus_Client_EXE), FileName = _Oculus_Client_EXE };
                    Process Client = Process.Start(Info);

                    if (Properties.Settings.Default.Minimize_Oculus_Client_OnClientStart)
                    {
                        _Report_ClientJustExited = true;

                        for (int i = 0; i < 50; i++)
                        {
                            Thread.Sleep(250);

                            if (_ClientJustExited)
                                break;
                        }

                        _Report_ClientJustExited = false;

                        Rect Location = new Rect();
                        for (int i = 0; i < 20; i++)
                        {
                            // try more then once

                            Functions.Native_Functions.MinimizeExternalWindow(Client.MainWindowHandle);

                            Thread.Sleep(250);

                            Functions.Native_Functions.GetWindowRect(Client.MainWindowHandle, ref Location);

                            if (double.IsNaN(Location.Left))
                                break;
                        }

                        Debug.WriteLine("Client Window Minimized");
                    }
                }
            }
        }

        public static void StopOculusServices()
        {
            if (Debugger.IsAttached)
            {
                if (!Dashes.Dash_Manager.EmulateReleaseMode())
                    return;
            }

            if (Properties.Settings.Default.CloseOculusClientOnExit)
            {
                Process[] Clients = Process.GetProcessesByName("OculusClient");

                foreach (Process item in Clients)
                    item.CloseMainWindow();
            }

            if (Properties.Settings.Default.CloseOculusServicesOnExit)
            {
                if (Service_Manager.GetStartup("OVRLibraryService") == "Manual")
                    Service_Manager.StopService("OVRLibraryService");

                if (Service_Manager.GetStartup("OVRService") == "Manual")
                    Service_Manager.StopService("OVRService");
            }
        }
    }
}