using Microsoft.Win32;
using System;
using System.IO;

namespace OVR_Dash_Manager.Software
{
    public static class Oculus_Link
    {
        public static void ResetLink()
        {
            if (Service_Manager.GetState("OVRService") == "Running")
            {
                Steam.ManagerCalledExit = true;

                Service_Manager.StopService("OVRService");
                Service_Manager.StartService("OVRService");

                Steam.ManagerCalledExit = true;
            }
        }

        public static void StopLink()
        {
            if (Service_Manager.GetState("OVRService") == "Running")
            {
                Steam.ManagerCalledExit = true;

                Service_Manager.StopService("OVRService");

                Steam.ManagerCalledExit = true;
            }
        }

        public static void StartLink()
        {
            if (Service_Manager.GetState("OVRService") != "Running")
                Service_Manager.StartService("OVRService");
        }

        public static void SetToOculusRunTime()
        {
            if (Oculus.Oculus_Is_Installed)
            {
                RegistryKey RunTime = Functions.Registry_Functions.GetRegistryKey(RegistryKey_Type.LocalMachine, @"SOFTWARE\Khronos\OpenXR\1");
                if (RunTime != null)
                {
                    String OculusRunTimePath = Path.Combine(Oculus.Oculus_Main_Directory, "Support\\oculus-runtime\\oculus_openxr_64.json");

                    if (File.Exists(OculusRunTimePath))
                        Functions.Registry_Functions.SetKeyValue(RunTime, "ActiveRuntime", OculusRunTimePath);

                    Functions.Registry_Functions.CloseKey(RunTime);

                    Dashes.Dash_Manager.MainForm_CheckRunTime();
                }
            }
        }
    }
}
