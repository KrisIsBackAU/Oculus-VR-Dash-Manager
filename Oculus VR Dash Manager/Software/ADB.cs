using AdvancedSharpAdbClient;
using System;
using System.Diagnostics;

namespace OVR_Dash_Manager.Software
{
    public static class ADB
    {
        public static void Start()
        {
            /// Created By https://github.com/quagsirus
            /// 

            if (Properties.Settings.Default.QuestPolling)
            {
                // Start an adb server if we don't already have one
                if (!AdbServer.Instance.GetStatus().IsRunning)
                {
                    var server = new AdbServer();
                    try
                    {
                        var result = server.StartServer(@".\ADB\adb.exe", false);
                        if (result != StartServerResult.Started)
                        {
                            Debug.WriteLine("Can't start adb server");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            /// 
        }
    }
}
