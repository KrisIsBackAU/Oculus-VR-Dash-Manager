using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;

namespace OVR_Dash_Manager.Forms
{
    /// <summary>
    /// Interaction logic for frm_Diagnostics.xaml
    /// </summary>
    public partial class frm_Diagnostics : Window
    {
        public frm_Diagnostics()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DiagnosticsChecker();

            Timer_Functions.CreateTimer("Diagnostics Checker", TimeSpan.FromSeconds(10), CallDiagnosticsChecker);
            Timer_Functions.StartTimer("Diagnostics Checker");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Timer_Functions.StopTimer("Diagnostics Checker");
            Timer_Functions.DisposeTimer("Diagnostics Checker");
        }

        private void CallDiagnosticsChecker(object sender, ElapsedEventArgs args)
        {
            Functions_Old.DoAction(this, new Action(delegate () { DiagnosticsChecker(); }));
        }

        private void DiagnosticsChecker()
        {
            lbl_OculusSoftware.Content = Software.Oculus.Oculus_Is_Installed ? "Installed" : "Not Found";

            if (!String.IsNullOrEmpty(Software.Oculus.Oculus_Client_EXE))
            {
                if (File.Exists(Software.Oculus.Oculus_Client_EXE))
                    lbl_OculussClient.Content = "Installed";
                else
                    lbl_OculussClient.Content = "Not Found";
            }
            else
                lbl_OculussClient.Content = "Not Found";

            lbl_OfficialDash.Content = Dashes.Dash_Manager.IsInstalled(Dashes.Dash_Type.Normal) ? "Installed" : "Not Found";
            lbl_OculusKiller.Content = Dashes.Dash_Manager.IsInstalled(Dashes.Dash_Type.Normal) ? "Installed" : "Not Found";

            FileVersionInfo Info = FileVersionInfo.GetVersionInfo(Software.Oculus.Oculus_Dash_File);
            Dashes.Dash_Type Current = Dashes.Dash_Manager.CheckWhosDash(Info.ProductName);
            lbl_CurrentDash.Content = Dashes.Dash_Manager.GetDashName(Current);

            lbl_OculusLibaryService.Content = $"State: {Service_Manager.GetState("OVRService")} - Startup: {Service_Manager.GetStartup("OVRService")}";
            lbl_OculusRuntimeService.Content = $"State: {Service_Manager.GetState("OVRService")} - Startup: {Service_Manager.GetStartup("OVRService")}";

            lbl_Steam.Content = $"{(Software.Steam.Steam_Installed ? "Installed" : "Not Found")}";
            lbl_SteamVR.Content = $"{(Software.Steam.Steam_VR_Installed ? "Installed" : "Not Found")} - {(Software.Steam.Steam_VR_Server_Running ? "Running" : "Not Started")}";

            lbl_DiagnosticsCheckTime.Content = DateTime.Now.ToString();

            lv_OculusDevices.ItemsSource = USB_Devices_Functions.GetUSBDevices();

            Software.Steam_VR_Settings.OpenXR_Runtime CurrentRuntime = Software.Steam_VR_Settings.Read_Runtime();

            if (CurrentRuntime == Software.Steam_VR_Settings.OpenXR_Runtime.Oculus)
                lbl_OpenXR_RunTime.Content = "Oculus Runtime";

            if (CurrentRuntime == Software.Steam_VR_Settings.OpenXR_Runtime.SteamVR)
                lbl_OpenXR_RunTime.Content = "SteamVR Runtime";
        }

        private void btn_OculusDebugTool_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Software.Oculus.Oculus_DebugTool_EXE))
                Process.Start(Software.Oculus.Oculus_DebugTool_EXE);
        }
    }
}