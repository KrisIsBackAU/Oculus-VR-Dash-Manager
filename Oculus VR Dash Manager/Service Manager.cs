using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace OVR_Dash_Manager
{
    public static class Service_Manager
    {
        private static Dictionary<String, ServiceController> Services = null;

        public static void RegisterService(String ServiceName)
        {
            if (Services == null)
                Services = new Dictionary<string, ServiceController>();

            if (!Services.ContainsKey(ServiceName))
            {
                ServiceController Service = null;
                try { Service = new ServiceController(ServiceName); } catch (Exception ex) { Debug.WriteLine($"Unable to load/find service {ServiceName} - {ex.Message}"); }

                if (Service != null)
                    Services.Add(Service.ServiceName, Service);
            }
        }

        public static void StopService(String ServiceName)
        {
            if (Services == null)
                return;

            if (Services.TryGetValue(ServiceName, out ServiceController Service))
            {
                Service.Refresh();
                if (Running(Service.Status))
                {
                    try
                    {
                        Service.Stop();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Unable to stop service {ServiceName} - {ex.Message}");
                    }
                }
            }
        }

        public static void StartService(String ServiceName)
        {
            if (Services == null)
                return;

            if (Services.TryGetValue(ServiceName, out ServiceController Service))
            {
                Service.Refresh();
                if (!Running(Service.Status))
                {
                    try
                    {
                        Service.Start();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Unable to start service {ServiceName} - {ex.Message}");
                    }
                }
            }
        }

        public static void Set_Automatic_Startup(String ServiceName)
        {
            if (Services == null)
                return;

            if (Services.TryGetValue(ServiceName, out ServiceController Service))
            {
                try
                {
                    Service.Refresh();
                    ServiceHelper.ChangeStartMode(Service, ServiceStartMode.Automatic);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to set automatic startup service {ServiceName} - {ex.Message}");
                }
            }
        }

        public static void Set_Manual_Startup(String ServiceName)
        {
            if (Services == null)
                return;

            if (Services.TryGetValue(ServiceName, out ServiceController Service))
            {
                try
                {
                    Service.Refresh();
                    ServiceHelper.ChangeStartMode(Service, ServiceStartMode.Manual);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to set manual startup service {ServiceName} - {ex.Message}");
                }
            }
        }

        private static Boolean Running(ServiceControllerStatus Status)
        {
            switch (Status)
            {
                case ServiceControllerStatus.Running:
                    return true;

                case ServiceControllerStatus.Stopped:
                    return false;

                case ServiceControllerStatus.Paused:
                    return true;

                case ServiceControllerStatus.StopPending:
                    return true;

                case ServiceControllerStatus.StartPending:
                    return true;

                default:
                    return false;
            }
        }

        public static String GetState(String ServiceName)
        {
            if (Services == null)
                return "--";

            String State = "Not Found";

            if (Services.TryGetValue(ServiceName, out ServiceController Service))
            {
                Service.Refresh();
                State = Service.Status.ToString();
            }

            return State;
        }

        public static String GetStartup(String ServiceName)
        {
            if (Services == null)
                return "--";

            String State = "Not Found";

            if (Services.TryGetValue(ServiceName, out ServiceController Service))
            {
                Service.Refresh();
                State = Service.StartType.ToString();
            }

            return State;
        }
    }

    // Created & Shared Publicly By: http://peterkellyonline.blogspot.com/2011/04/configuring-windows-service.html  https://stackoverflow.com/users/215600/peter-kelly
    public static class ServiceHelper
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean ChangeServiceConfig(
            IntPtr hService,
            UInt32 nServiceType,
            UInt32 nStartType,
            UInt32 nErrorControl,
            String lpBinaryPathName,
            String lpLoadOrderGroup,
            IntPtr lpdwTagId,
            [In] char[] lpDependencies,
            String lpServiceStartName,
            String lpPassword,
            String lpDisplayName);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr OpenService(
            IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(
            string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
        public static extern int CloseServiceHandle(IntPtr hSCObject);

        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
        private const uint SERVICE_QUERY_CONFIG = 0x00000001;
        private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
        private const uint SC_MANAGER_ALL_ACCESS = 0x000F003F;

        public static void ChangeStartMode(ServiceController svc, ServiceStartMode mode)
        {
            var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Manager Error");
            }

            var serviceHandle = OpenService(
                scManagerHandle,
                svc.ServiceName,
                SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

            if (serviceHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Error");
            }

            var result = ChangeServiceConfig(
                serviceHandle,
                SERVICE_NO_CHANGE,
                (uint)mode,
                SERVICE_NO_CHANGE,
                null,
                null,
                IntPtr.Zero,
                null,
                null,
                null,
                null);

            if (result == false)
            {
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                throw new ExternalException("Could not change service start type: "
                    + win32Exception.Message);
            }

            CloseServiceHandle(serviceHandle);
            CloseServiceHandle(scManagerHandle);
        }
    }
}