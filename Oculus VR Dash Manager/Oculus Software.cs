using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace OVR_Dash_Manager
{
    public static class Oculus_Software
    {
        private static string _OculusDashDirectory;

        public static string OculusDashDirectory
        {
            get { return _OculusDashDirectory; }
            private set { _OculusDashDirectory = value; }
        }

        private static string _OculusDashFile;

        public static string OculusDashFile
        {
            get { return _OculusDashFile; }
            private set { _OculusDashFile = value; }
        }

        private static string _OculusClientFile;

        public static string OculusClientFile
        {
            get { return _OculusClientFile; }
            private set { _OculusClientFile = value; }
        }

        private static string _OculusDebugTool;

        public static string OculusDebugTool
        {
            get { return _OculusDebugTool; }
            private set { _OculusDebugTool = value; }
        }

        private static bool _OculusInstalled;

        public static bool OculusInstalled
        {
            get { return _OculusInstalled; }
            private set { _OculusInstalled = value; }
        }

        private static bool _NormalDash;

        public static bool NormalDash
        {
            get { return _NormalDash; }
            private set { _NormalDash = value; }
        }

        private static bool _CustomDash;

        public static bool CustomDash
        {
            get { return _CustomDash; }
            private set { _CustomDash = value; }
        }

        private static String _CustomDashName;

        public static String CustomDashName
        {
            get { return _CustomDashName; }
            private set { _CustomDashName = value; }
        }

        public static void Check_Is_Installed()
        {
            // @"C:\Program Files\Oculus\Support\oculus-dash\dash\bin";
            // @"C:\Program Files\Oculus\Support\oculus-dash\dash\bin\OculusDash.exe";
            String OculusPath = Environment.GetEnvironmentVariable("OculusBase");

            if (Directory.Exists(OculusPath))
            {
                _OculusDashDirectory = Path.Combine(OculusPath, @"Support\oculus-dash\dash\bin");
                _OculusClientFile = Path.Combine(OculusPath, @"Support\oculus-client\OculusClient.exe");
                _OculusDebugTool = Path.Combine(OculusPath, @"Support\oculus-diagnostics\OculusDebugTool.exe");
                _OculusDashFile = Path.Combine(_OculusDashDirectory, @"OculusDash.exe");
            }
        }

        private static void ClearDashSetting()
        {
            _NormalDash = false;
            _CustomDash = false;
        }

        public static void Check_Current_Dash()
        {
            if (Directory.Exists(OculusDashDirectory))
            {
                _OculusInstalled = true;

                if (File.Exists(Path.Combine(OculusDashDirectory, OculusDashFile)))
                    WhichDash(Path.Combine(OculusDashDirectory, OculusDashFile));
            }
        }

        private static void WhichDash(String FilePath)
        {
            if (File.Exists(FilePath))
            {
                ClearDashSetting();

                FileVersionInfo Info = FileVersionInfo.GetVersionInfo(FilePath);
                Dashes.Dash_Type Current = Dashes.Dash_Manager.CheckWhosDash(Info.ProductName);
                CustomDashName = Dashes.Dash_Manager.GetDashName(Current);

                if (CustomDashName != Dashes.Dash_Manager.GetDashName(Dashes.Dash_Type.Normal))
                    _CustomDash = true;
                else
                {
                    if (CheckIsLegit(FilePath))
                        _NormalDash = true;
                    else
                    {
                        _CustomDash = true;
                        CustomDashName = "Unknown";
                    }
                }
            }
        }

        private static bool CheckIsLegit(String FilePath)
        {
            Boolean Legit = false;

            X509Certificate cert = X509Certificate.CreateFromSignedFile(FilePath);

            if (cert.Issuer == "CN=DigiCert SHA2 Assured ID Code Signing CA, OU=www.digicert.com, O=DigiCert Inc, C=US")
                Legit = true;

            return Legit;
        }

        public static void StartOculusClient()
        {
            Process[] Dashes = Process.GetProcessesByName("OculusClient");

            if (Dashes.Length == 0)
                Process.Start(Oculus_Software._OculusClientFile);
        }

        public static void ResetLink()
        {
            if (Service_Manager.GetState("OVRService") == "Running")
            {
                SteamVR.ManagerCalledExit = true;

                Service_Manager.StopService("OVRService");
                Service_Manager.StartService("OVRService");

                SteamVR.ManagerCalledExit = true;

            }
        }
    }
}