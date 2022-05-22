using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace OVR_Dash_Manager.Dashes
{
    public static class Dash_Manager
    {
        private static OVR_Dash Oculus_Dash;
        private static OVR_Dash SteamVR_Dash;

        public static void GenerateDashes()
        {
            Oculus_Software.Check_Is_Installed();

            Oculus_Dash = new OVR_Dash("Offical Oculus Dash", "OculusDash_Normal.exe", ProcessToStop: "vrserver.exe");
            SteamVR_Dash = new OVR_Dash("ItsKaitlyn03 - Oculus Killer", "ItsKaitlyn03_Oculus_Killer.exe", "Oculus Killer", "ItsKaitlyn03", "OculusKiller", "OculusDash.exe");

            CheckInstalled();
        }

        private static void CheckInstalled()
        {
            Oculus_Dash.CheckInstalled();

            SteamVR_Dash.CheckInstalled();

            if (!SteamVR_Dash.Installed)
                SteamVR_Dash.Download();
            else if (!SteamVR_Dash.NeedUpdate)
                SteamVR_Dash.Download();

            Oculus_Software.Check_Current_Dash();

            if (!Oculus_Dash.Installed)
            {
                if (Oculus_Software.NormalDash)
                {
                    // Copy Default Oculus Dash if not already done
                    File.Copy(Oculus_Software.OculusDashFile, Path.Combine(Oculus_Software.OculusDashDirectory, Oculus_Dash.DashFileName), true);
                    Oculus_Dash.CheckInstalled();
                }
            }
            else
            {
                // Check if Oculus Updated and check is Oculus Dash has changed by "Length"
                if (Oculus_Software.NormalDash)
                {
                    FileInfo CurrentDash = new FileInfo(Path.Combine(Oculus_Software.OculusDashDirectory, Oculus_Dash.DashFileName));
                    FileInfo OculusDashFile = new FileInfo(Oculus_Software.OculusDashFile);

                    // Update File
                    if (CurrentDash.Length != OculusDashFile.Length)
                        File.Copy(Oculus_Software.OculusDashFile, Path.Combine(Oculus_Software.OculusDashDirectory, Oculus_Dash.DashFileName), true);
                }
            }
        }

        public static Boolean IsInstalled(Dash_Type Dash)
        {
            switch (Dash)
            {
                case Dash_Type.Normal:
                    return Oculus_Dash.Installed;

                case Dash_Type.OculusKiller:
                    return SteamVR_Dash.Installed;

                default:
                    return false;
            }
        }

        private static bool SetActiveDash(Dash_Type Dash)
        {
            Boolean Activated = false;

            switch (Dash)
            {
                case Dash_Type.Normal:
                    Activated = Oculus_Dash.Activate_Dash();
                    break;

                case Dash_Type.OculusKiller:
                    Activated = SteamVR_Dash.Activate_Dash();
                    break;

                default:
                    break;
            }

            return Activated;
        }

        public static Dash_Type CheckWhosDash(String File_ProductName)
        {
            if (SteamVR_Dash.IsThisYourDash(File_ProductName))
                return Dash_Type.OculusKiller;

            if (String.IsNullOrEmpty(File_ProductName))
                return Dash_Type.Normal;

            return Dash_Type.Unknown;
        }

        public static String GetDashName(Dash_Type Dash)
        {
            switch (Dash)
            {
                case Dash_Type.Unknown:
                    return "Unknown";

                case Dash_Type.Normal:
                    return Oculus_Dash.DisplayName;

                case Dash_Type.OculusKiller:
                    return SteamVR_Dash.DisplayName;

                default:
                    return "No Name Found";
            }
        }

        public static Boolean Activate(Dash_Type Dash)
        {
            Console.WriteLine("Starting Activation: " + Dash.ToString());

            Boolean Activated = false;

            ServiceController sc = new ServiceController("OVRService");

            Boolean OVRServiceRunning = Running(sc.Status);
            Boolean OVRService_WasRunning = false;
            Boolean CanAccess = true;

            if (OVRServiceRunning)
            {
                OVRService_WasRunning = true;
                try
                {
                    Console.WriteLine("Stopping OVRService");

                    sc.Stop();
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    CanAccess = false;
                }
            }

            if (CanAccess)
            {
                Console.WriteLine("Checking OVRService");

                sc.Refresh();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(1));

                sc.Refresh();
                OVRServiceRunning = Running(sc.Status);

                if (!OVRServiceRunning)
                {
                    Console.WriteLine("Activating Dash");
                    Activated = SetActiveDash(Dash);
                }
                else
                    Console.WriteLine("!!!!!! OVRService Can Not Be Stopped");

                if (OVRService_WasRunning)
                {
                    Console.WriteLine("Restarting OVRService");
                    sc.Start();
                }
            }
            else
                Console.WriteLine("!!!!!! OVRService Can Not Be Accessed");

            return Activated;
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
    }
}