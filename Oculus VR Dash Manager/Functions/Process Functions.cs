using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace OVR_Dash_Manager.Functions
{
    public static class Process_Functions
    {
        public static Boolean IsCurrentProcess_Elevated()
        {
            WindowsIdentity vIdentity = GetWindowsIdentity();
            WindowsPrincipal vPrincipal = GetWindowsPrincipal(vIdentity);

            bool pReturn = vPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            vIdentity.Dispose();
            return pReturn;
        }

        private static WindowsIdentity GetWindowsIdentity()
        {
            return WindowsIdentity.GetCurrent();
        }

        private static WindowsPrincipal GetWindowsPrincipal(WindowsIdentity pIdentity)
        {
            return new WindowsPrincipal(pIdentity);
        }

        public static string GetCurrentProcessDirectory()
        {
            Process Current = Process.GetCurrentProcess();
            return Path.GetDirectoryName(Current.MainModule.FileName);
        }

        public static Process StartProcess(string Path, string Arguments = "")
        {
            if (File.Exists(Path))
            {
                return Process.Start(Path, Arguments);
                // File
            }
            else
            {
                // try and build full url - else returns same as input
                string URL = String_Functions.GetFullURL(Path);
                return Process.Start(URL, Arguments);
                // Web Site
            }
        }
    }
}