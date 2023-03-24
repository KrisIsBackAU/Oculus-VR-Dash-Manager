using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using AdvancedSharpAdbClient;

namespace OVR_Dash_Manager.Software
{
    public static class Oculus_Link
    {
        public static void StartLinkOnDevice()
        {
            /// ADB Auto Start Created By https://github.com/quagsirus
            /// 
            if (Properties.Settings.Default.QuestPolling)
            {
                ADB.Start(); // KrisIsBack Addin - This allow when the setting is turned on after launch (only trys if needed to)

                // Allow time for quest to register with ADB server
                System.Threading.Thread.Sleep(1000);
                var connectedDevices = USB_Devices_Functions.GetUSBDevices();
                foreach (var device in connectedDevices)
                {
                    if (string.IsNullOrEmpty(device.FullSerial) || device.Type != "Quest") continue;
                    var client = new AdbClient();
                    var adbDevices = client.GetDevices();
                    // Ensure adb only interacts with quest device serial nos
                    foreach (var adbDevice in adbDevices.Where(adbDevice => device.FullSerial == adbDevice.Serial))
                    {
                        // Only start quest link if adb has been authorized
                        if (adbDevice.State == DeviceState.Online) client.StartApp(adbDevice, "com.oculus.xrstreamingclient");
                    }
                }
            }
            /// ADB Auto Start Created By https://github.com/quagsirus
            /// 
        }


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
