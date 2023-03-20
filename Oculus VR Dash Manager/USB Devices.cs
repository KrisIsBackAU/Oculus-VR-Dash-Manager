using System;
using System.Collections.Generic;
using System.Management;

namespace OVR_Dash_Manager
{
    public static class USB_Devices_Functions
    {
        public static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> PluggedInDevices = new List<USBDeviceInfo>();

            using (ManagementObjectSearcher Searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity WHERE DeviceID LIKE '%VID_2833%'"))
                PluggedInDevices.AddRange(ReadSearcher(Searcher.Get()));

            return PluggedInDevices;
        }

        private static List<USBDeviceInfo> ReadSearcher(ManagementObjectCollection Devices)
        {
            Dictionary<String, String> DeviceIDs = new Dictionary<string, string>
            {
                { "VID_2833&PID_2031", "Rift CV1" },
                { "VID_2833&PID_3031", "Rift CV1" },
                { "VID_2833&PID_0137", "Quest Headset" },
                { "VID_2833&PID_0201", "Camera DK2" },
                { "VID_2833&PID_0211", "Rift CV1 Sensor" },
                { "VID_2833&PID_0330", "Rift CV1 Audio" },
                { "VID_2833&PID_1031", "Rift CV1" },
                { "VID_2833&PID_2021", "Rift DK2" },
                { "VID_2833&PID_0001", "Rift Developer Kit 1" },
                { "VID_2833&PID_0021", "Rift DK2" },
                { "VID_2833&PID_0031", "Rift CV1" },
                { "VID_2833&PID_0101", "Latency Tester" },
                { "VID_2833&PID_0183", "Quest" },
                { "VID_2833&PID_0182", "Quest" },
                { "VID_2833&PID_0186", "Quest" },
                { "VID_2833&PID_0083", "Quest" },
                { "VID_2833&PID_0186&MI_00", "Quest XRSP" },
                { "VID_2833&PID_0186&MI_01", "Quest ADB" },
                { "VID_2833&PID_0183&MI_00", "Quest XRSP" },
                { "VID_2833&PID_0183&MI_01", "Quest ADB" },
            };

            List<USBDeviceInfo> PluggedInDevices = new List<USBDeviceInfo>();

            foreach (ManagementObject oDevice in Devices)
            {
                try
                {
                    String DeviceID = TryGetProperty(oDevice, "DeviceID").ToString();
                    String DeviceCaption = TryGetProperty(oDevice, "Caption").ToString();

                    string[] Data = DeviceID.Split('\\');
                    string Type = "";
                    string Serial = "";
                    string MaskedSerial = "";

                    if (Data.Length == 3)
                    {
                        Serial = Data[2];

                        if (Serial.Contains("&"))
                            Serial = "";

                        if (!DeviceIDs.TryGetValue(Data[1], out Type))
                            Type = "Unknown - " + Data[1];

                        if (DeviceCaption.StartsWith("USB Comp"))
                            DeviceCaption = Type;

                        if (DeviceCaption.Length > 0)
                            Type = DeviceCaption;

                        Int32 SerialLength = Serial.Length - 5;

                        if (SerialLength > 0)
                        {
                            MaskedSerial = Serial.Substring(Serial.Length - 5, 5);
                            MaskedSerial = MaskedSerial.PadLeft(SerialLength, '*');
                        }

                        PluggedInDevices.Add(new USBDeviceInfo(DeviceID, Type, MaskedSerial, Serial));
                    }
                }
                catch (Exception)
                {
                }
            }

            return PluggedInDevices;
        }

        private static object TryGetProperty(ManagementObject wmiObj, string propertyName)
        {
            object retval;
            try
            {
                retval = wmiObj.GetPropertyValue(propertyName);
            }
            catch (System.Management.ManagementException)
            {
                retval = null;
            }
            return retval;
        }
    }

    /// <summary>USB Device info Class</summary>
    public class USBDeviceInfo
    {
        public USBDeviceInfo(string DeviceID, string Type, string MaskedSerial, string FullSerial)
        {
            this.DeviceID = DeviceID;
            this.Type = Type;
            this.MaskedSerial = MaskedSerial;
            this.FullSerial = FullSerial;
        }

        public string DeviceID { get; private set; }
        public string Type { get; private set; }
        public string MaskedSerial { get; private set; }
        public string FullSerial { get; private set;}
    }
}