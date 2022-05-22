using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace OVR_Dash_Manager
{
    public static class Oculus_Software
    {
        public static readonly String OculusDashDirectory = @"C:\Program Files\Oculus\Support\oculus-dash\dash\bin";
        public static readonly String OculusDashFile = @"C:\Program Files\Oculus\Support\oculus-dash\dash\bin\OculusDash.exe";

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
    }
}