using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OVR_Dash_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /// Set Button to Dash Type for linkage

            btn_Normal.Tag = Dashes.Dash_Type.Normal;
            btn_SteamVR.Tag = Dashes.Dash_Type.OculusKiller;

            foreach (UIElement item in gd_DashButtons.Children)
            {
                if (item is Button button)
                    item.IsEnabled = false;
            }

            Dashes.Dash_Manager.GenerateDashes();

            if (!Oculus_Software.OculusInstalled)
            {
                lbl_CurrentSetting.Content = "Oculus Directory Not Found";
                return;
            }

            // Only Enabled Whats Installed
            foreach (UIElement item in gd_DashButtons.Children)
            {
                if (item is Button button)
                {
                    if (button.Tag is Dashes.Dash_Type Dash)
                        button.IsEnabled = Dashes.Dash_Manager.IsInstalled(Dash);
                }
            }

            lbl_CurrentSetting.Content = Oculus_Software.CustomDashName;

            Dispatcher_Timer_Functions.CreateTimer("SteamVR Checker", TimeSpan.FromSeconds(10), CheckSteamVR);
            Dispatcher_Timer_Functions.StartTimer("SteamVR Checker");

            Dispatcher_Timer_Functions.CreateTimer("Hover Checker", TimeSpan.FromMilliseconds(500), Check_Hover);
            Dispatcher_Timer_Functions.StartTimer("Hover Checker");

            CheckSteamVR(null, null);
        }

        private void btn_ActivateDash_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is Dashes.Dash_Type Dash)
                {
                    Dashes.Dash_Manager.Activate(Dash);
                    Oculus_Software.Check_Current_Dash();
                    lbl_CurrentSetting.Content = Oculus_Software.CustomDashName;
                }
            }
        }

        private bool SteamVR_Running = false;

        private void CheckSteamVR(object sender, EventArgs args)
        {
            Process[] SteamVR = Process.GetProcessesByName("vrserver.exe");
            if (SteamVR.Length > 0)
                SteamVR_Running = true;
            else
                SteamVR_Running = false;
        }

        private bool Hovering_Normal_Button = false;
        private DateTime Hover_Normal_Time;

        private void btn_Normal_MouseEnter(object sender, MouseEventArgs e)
        {
            Hovering_Normal_Button = true;
            Hover_Normal_Time = DateTime.Now;

            if (SteamVR_Running)
                pb_Normal.Value = 100;
            else
                pb_Normal.Value = 0;
        }

        private void btn_Normal_MouseLeave(object sender, MouseEventArgs e)
        {
            Hovering_Normal_Button = false;
            pb_Normal.Value = 0;
        }

        private void Check_Hover(object sender, EventArgs args)
        {
            if (SteamVR_Running)
            {
                if (Hovering_Normal_Button)
                {
                    TimeSpan Passed = DateTime.Now.Subtract(Hover_Normal_Time);
                    if (Passed.TotalSeconds < 5)
                        pb_Normal.Value = Passed.TotalMilliseconds;

                    if (Passed.TotalSeconds >= 5)
                    {
                        Hovering_Normal_Button = false;
                        pb_Normal.Value = 0;
                        btn_ActivateDash_Click(btn_Normal, null);
                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispatcher_Timer_Functions.StopTimer("SteamVR Checker");
        }
    }
}