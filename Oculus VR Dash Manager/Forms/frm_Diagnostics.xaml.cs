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
            Functions.DoAction(this, new Action(delegate () { DiagnosticsChecker(); }));
        }

        private void DiagnosticsChecker()
        {
            lbl_OculusSoftware.Content = Oculus_Software.OculusInstalled ? "Installed" : "Not Found";

            if (!String.IsNullOrEmpty(Oculus_Software.OculusClientFile))
            {
                if (File.Exists(Oculus_Software.OculusClientFile))
                    lbl_OculussClient.Content = "Installed";
                else
                    lbl_OculussClient.Content = "Not Found";
            }
            else
                lbl_OculussClient.Content = "Not Found";

            lbl_OfficalDash.Content = Dashes.Dash_Manager.IsInstalled(Dashes.Dash_Type.Normal) ? "Installed" : "Not Found";
            lbl_OculusKiller.Content = Dashes.Dash_Manager.IsInstalled(Dashes.Dash_Type.Normal) ? "Installed" : "Not Found";

            FileVersionInfo Info = FileVersionInfo.GetVersionInfo(Oculus_Software.OculusDashFile);
            Dashes.Dash_Type Current = Dashes.Dash_Manager.CheckWhosDash(Info.ProductName);
            lbl_CurrentDash.Content = Dashes.Dash_Manager.GetDashName(Current);

            lbl_OculusLibaryService.Content = $"State: {Service_Manager.GetState("OVRService")} - Startup: {Service_Manager.GetStartup("OVRService")}";
            lbl_OculusRuntimeService.Content = $"State: {Service_Manager.GetState("OVRService")} - Startup: {Service_Manager.GetStartup("OVRService")}";

            lbl_SteamVR.Content = $"{(SteamVR.SteamVR_Installed ? "Installed" : "Not Found")} - {(SteamVR.SteamVR_Running ? "Running" : "Not Started")}";
            lbl_Steam.Content = $"{(SteamVR.Steam_Installed ? "Installed" : "Not Found")}";

            lbl_DiagnosticsCheckTime.Content = DateTime.Now.ToString();

            lv_OculusDevices.ItemsSource = USB_Devices_Functions.GetUSBDevices();
        }

        private void btn_OculusDebugTool_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Oculus_Software.OculusDebugTool))
                Process.Start(Oculus_Software.OculusDebugTool);
        }
    }
}