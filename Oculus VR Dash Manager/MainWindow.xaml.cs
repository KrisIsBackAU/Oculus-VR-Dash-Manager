using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Timers;

namespace OVR_Dash_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Boolean FireUIEvents = false;
        private Boolean Elevated = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chkbx_CheckForUpdates.IsChecked = Properties.Settings.Default.CheckUpdate;
            chkbx_AlwaysOnTop.IsChecked = Properties.Settings.Default.AlwaysOnTop;

            Topmost = Properties.Settings.Default.AlwaysOnTop;

            Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Starting Up"; }));

            LinkDashesToButtons();

            Disable_Dash_Buttons();

            if (Functions.IsCurrentProcess_Elevated())
                Elevated = true;

            Thread Start = new Thread(Startup);
            Start.IsBackground = true;
            Start.Start();
        }

        private void LinkDashesToButtons()
        {
            btn_ExitOculusLink.Tag = Dashes.Dash_Type.Exit;
            btn_Normal.Tag = Dashes.Dash_Type.Normal;
            btn_SteamVR.Tag = Dashes.Dash_Type.OculusKiller;
        }

        private void Startup()
        {
            if (Elevated)
            {
                Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Checking Installed Dashes & Updates"; }));
                Dashes.Dash_Manager.GenerateDashes();

                if (!Oculus_Software.OculusInstalled)
                {
                    Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Oculus Directory Not Found"; }));
                    return;
                }

                if (!Dashes.Dash_Manager.Oculus_Offical_Dash_Installed())
                {
                    Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Offical Oculus Dash Not Found, Please Replace Original Oculus Dash"; }));
                    return;
                }

                Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Starting SteamVR Watcher"; }));


                CheckSteamVR(null, null);

                Functions.DoAction(this, new Action(delegate () 
                { 
                    lbl_CurrentSetting.Content = "Updating UI"; 
                    Update_Dash_Buttons();

                    Timer_Functions.CreateTimer("SteamVR Checker", TimeSpan.FromSeconds(10), CheckSteamVR);
                    Timer_Functions.CreateTimer("Hover Checker", TimeSpan.FromMilliseconds(250), Check_Hover);
                    
                    Timer_Functions.StartTimer("SteamVR Checker");
                    Timer_Functions.StartTimer("Hover Checker");

                    //Dispatcher_Timer_Functions.CreateTimer("SteamVR Checker", TimeSpan.FromSeconds(10), CheckSteamVR);
                    //Dispatcher_Timer_Functions.StartTimer("SteamVR Checker");

                    //Dispatcher_Timer_Functions.CreateTimer("Hover Checker", TimeSpan.FromSeconds(1), Check_Hover);
                    //Dispatcher_Timer_Functions.StartTimer("Hover Checker");

                }));
                FireUIEvents = true;
            }
            else
            {
                Functions.DoAction(this, new Action(delegate ()
                {
                    lbl_CurrentSetting.Content = "Run as Admin Required";
                    MessageBox.Show(this, "This proram must be run with Admin Permissions", "This proram must be run with Admin Permissions", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }
        }

        private void Disable_Dash_Buttons()
        {
            foreach (UIElement item in gd_DashButtons.Children)
            {
                if (item is Button button)
                    button.IsEnabled = false;
            }
        }

        private void Update_Dash_Buttons()
        {
            // Only Enabled Whats Installed
            foreach (UIElement item in gd_DashButtons.Children)
            {
                if (item is Button button)
                {
                    if (button.Tag is Dashes.Dash_Type Dash)
                        button.IsEnabled = Dashes.Dash_Manager.IsInstalled(Dash);
                }
            }

            btn_ExitOculusLink.IsEnabled = true;

            lbl_CurrentSetting.Content = Oculus_Software.CustomDashName;
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
                    pb_Normal.Value = 0;
                    pb_Exit.Value = 0;
                }
            }
        }

        private bool SteamVR_Running = false;

        private void CheckSteamVR(object sender, ElapsedEventArgs args)
        {
            Process[] SteamVR = Process.GetProcessesByName("vrserver");
            if (SteamVR.Length > 0)
                SteamVR_Running = true;
            else
                SteamVR_Running = false;
        }




        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Timer_Functions.StopTimer("SteamVR Checker");
        }

        private void lbl_ItsKaitlyn03_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Functions.OpenURL("https://github.com/ItsKaitlyn03/OculusKiller");
        }

        private void lbl_Title_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Functions.OpenURL("https://github.com/KrisIsBackAU/Oculus-VR-Dash-Manager");
        }

        private void chkbx_CheckForUpdates_Checked(object sender, RoutedEventArgs e)
        {
            if (!FireUIEvents)
                return;

            Properties.Settings.Default.CheckUpdate = (bool)chkbx_CheckForUpdates.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void chkbx_CheckForUpdates_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!FireUIEvents)
                return;

            Properties.Settings.Default.CheckUpdate = (bool)chkbx_CheckForUpdates.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void chkbx_AlwaysOnTop_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CheckUpdate = (bool)chkbx_AlwaysOnTop.IsChecked;
            Properties.Settings.Default.Save();

            Topmost = Properties.Settings.Default.AlwaysOnTop;

        }

        private void chkbx_AlwaysOnTop_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CheckUpdate = (bool)chkbx_AlwaysOnTop.IsChecked;
            Properties.Settings.Default.Save();

            Topmost = Properties.Settings.Default.AlwaysOnTop;
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


        private bool Hovering_Exit_Button = false;
        private DateTime Hover_Exit_Time;


        private void btn_ExitOculusLink_MouseEnter(object sender, MouseEventArgs e)
        {
            Hovering_Exit_Button = true;
            Hover_Exit_Time = DateTime.Now;

            if (SteamVR_Running)
                pb_Exit.Value = 100;
            else
                pb_Exit.Value = 0;
        }

        private void btn_ExitOculusLink_MouseLeave(object sender, MouseEventArgs e)
        {
            Hovering_Exit_Button = false;
            pb_Exit.Value = 0;
        }


        private void Check_Hover(object sender, ElapsedEventArgs args)
        {
            if (SteamVR_Running)
            {
                bool MouseMouse = false;
                bool ActivateNormal = false;
                bool ActivateExit = false;

                if (Hovering_Normal_Button)
                {
                    TimeSpan Passed = DateTime.Now.Subtract(Hover_Normal_Time);
                    if (Passed.TotalSeconds < 5)
                        Functions.DoAction(this, new Action(delegate () { pb_Normal.Value = Passed.TotalMilliseconds; }));

                    if (Passed.TotalSeconds >= 5)
                    {
                        Functions.DoAction(this, new Action(delegate () { pb_Normal.Value = 5000; }));
                        Hovering_Normal_Button = false;
                        ActivateNormal = true;
                        MouseMouse = true;
                    }
                }

                if (Hovering_Exit_Button)
                {
                    TimeSpan Passed = DateTime.Now.Subtract(Hover_Exit_Time);
                    if (Passed.TotalSeconds < 5)
                        Functions.DoAction(this, new Action(delegate () { pb_Exit.Value = Passed.TotalMilliseconds; }));

                    if (Passed.TotalSeconds >= 5)
                    {
                        Functions.DoAction(this, new Action(delegate () { pb_Exit.Value = 5000; }));
                        Hovering_Exit_Button = false;
                        ActivateExit = true;
                        MouseMouse = true;
                    }
                }

                if (MouseMouse)
                {
                    Functions.DoAction(this, new Action(delegate () {
                        MoveMouseToElement(lbl_CurrentSetting);
                    }));
                }

                if (ActivateNormal)
                {
                    Functions.DoAction(this, new Action(delegate () {
                        pb_Normal.Value = 5000;
                        pb_Normal.UpdateLayout();
                        btn_ActivateDash_Click(btn_Normal, null);
                    }));
                }

                if (ActivateExit)
                {
                    Functions.DoAction(this, new Action(delegate () {
                        pb_Exit.Value = 5000;
                        pb_Exit.UpdateLayout();
                        btn_ActivateDash_Click(btn_ExitOculusLink, null);
                    }));
                }
            }
        }

        private void MoveMouseToElement(FrameworkElement Element)
        {
            Point relativePoint = Element.TransformToAncestor(this).Transform(new Point(0, 0));
            Point pt = new Point(relativePoint.X + Element.ActualWidth / 2, relativePoint.Y + Element.ActualHeight / 2);
            Point windowCenterPoint = pt;//new Point(125, 80);
            Point centerPointRelativeToSCreen = this.PointToScreen(windowCenterPoint);
            Functions.MoveCursor((int)centerPointRelativeToSCreen.X, (int)centerPointRelativeToSCreen.Y);
        }

        private void btn_OpenDashLocation_Click(object sender, RoutedEventArgs e)
        {
            if (Oculus_Software.OculusInstalled)
                Functions.ShowFileInDirectory(Oculus_Software.OculusDashFile);
        }
    }
}