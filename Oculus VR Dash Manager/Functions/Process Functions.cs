using System;
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
    }
}