﻿using OVR_Dash_Manager.Software;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WindowsInput;
using WindowsInput.Native;

namespace OVR_Dash_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Boolean Elevated = false;
        private Boolean FireUIEvents = false;
        private Hover_Button Oculus_Dash;
        private Hover_Button Exit_Link;
        private InputSimulator Keyboard_Simuator;
        public bool Debug_EmulateReleaseMode = false;

        public MainWindow()
        {
            InitializeComponent();

            Application _This = Application.Current;
            _This.DispatcherUnhandledException += AppDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;

            Title += " v" + typeof(MainWindow).Assembly.GetName().Version;
            Topmost = Properties.Settings.Default.AlwaysOnTop;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            btn_Diagnostics.IsEnabled = false;
            btn_OpenSettings.IsEnabled = false;
            lbl_CurrentSetting.Content = "Starting Up";
            Elevated = Functions.Process_Functions.IsCurrentProcess_Elevated();

            Disable_Dash_Buttons();
            LinkDashesToButtons();
            Generate_Hover_Buttons();

            Dashes.Dash_Manager.PassMainForm(this);
            Software.Steam.Steam_VR_Running_State_Changed_Event += Steam_Steam_VR_Running_State_Changed_Event;

            Software.Auto_Launch_Programs.Generate_List();

            Thread Start = new Thread(Startup);
            Start.IsBackground = true;
            Start.Start();
        }

        private void Steam_Steam_VR_Running_State_Changed_Event()
        {
            Functions_Old.DoAction(this, new Action(delegate ()
            {
                lbl_SteamVR_Status.Content = Software.Steam.Steam_VR_Server_Running ? "Running" : "N/A";
            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Software.Windows_Audio_v2.Set_To_Normal_Speaker_Auto();
            Software.Windows_Audio_v2.Set_To_Normal_Microphone_Auto();
            Software.ADB.Stop();
            Functions.Process_Watcher.Stop();

            Timer_Functions.StopTimer("Hover Checker");
            Timer_Functions.DisposeTimer("Hover Checker");

            Hide();
            Software.Oculus.StopOculusServices();

            Software.Auto_Launch_Programs.Run_Closing_Programs();
        }

        private void RefreshUI()
        {
            CheckRunTime();
            Exit_Link.Hovered_Seconds_To_Activate = Properties.Settings.Default.Hover_Activation_Time;
            Oculus_Dash.Hovered_Seconds_To_Activate = Properties.Settings.Default.Hover_Activation_Time;
        }

        private void Startup()
        {
            Functions.Process_Watcher.Start();

            /// ADB Auto Start Created By https://github.com/quagsirus
            // KrisIsBack Addin - Sorted code into their own places & added warning message when setting turned on

            // Start listening for new device connections
            Functions.Device_Watcher.DeviceConnected += Oculus_Link.StartLinkOnDevice;
            Functions.Device_Watcher.Start();
            ADB.Start();
            ///

            Functions.Process_Watcher.IngoreEXEName("cmd.exe");
            Functions.Process_Watcher.IngoreEXEName("conhost.exe");
            Functions.Process_Watcher.IngoreEXEName("reg.exe");
            Functions.Process_Watcher.IngoreEXEName("SearchFilterHost.exe");

            if (Elevated)
            {
                Functions_Old.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Checking Installed Dashes & Updates"; }));
                Dashes.Dash_Manager.GenerateDashes();

                if (!Software.Oculus.Oculus_Is_Installed)
                {
                    Functions_Old.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Oculus Directory Not Found"; }));
                    return;
                }

                if (!Dashes.Dash_Manager.Oculus_Official_Dash_Installed())
                {
                    Functions_Old.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Official Oculus Dash Not Found, Replace Original 'OculusDash.exe'"; btn_OpenDashLocation_Click(null, null); }));
                    return;
                }

                Functions_Old.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Starting Steam Watcher"; }));
                Software.Steam.Setup();

                Functions_Old.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Starting Hover Buttons"; }));

                Timer_Functions.CreateTimer("Hover Checker", TimeSpan.FromMilliseconds(250), Check_Hover);
                Timer_Functions.StartTimer("Hover Checker");

                Functions_Old.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Starting Service Manager"; }));

                Service_Manager.RegisterService("OVRLibraryService");
                Service_Manager.RegisterService("OVRService");

                if (Properties.Settings.Default.RunOculusClientOnStartup)
                {
                    Functions_Old.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Starting Oculus Client"; }));
                    Software.Oculus.StartOculusClient();
                }

                CheckRunTime();

                Software.Windows_Audio_v2.Setup();
                Software.Windows_Audio_v2.Set_To_Quest_Speaker_Auto();
                Software.Windows_Audio_v2.Set_To_Quest_Microphone_Auto();

                Software.Auto_Launch_Programs.Run_Startup_Programs();

                Functions_Old.DoAction(this, new Action(delegate ()
                {
                    btn_Diagnostics.IsEnabled = true;
                    btn_OpenSettings.IsEnabled = true;
                    lbl_SteamVR_Status.Content = "Installed: " + Software.Steam.Steam_VR_Installed;
                    lbl_CurrentSetting.Content = Software.Oculus.Current_Dash_Name;
                    Update_Dash_Buttons();
                }));

                FireUIEvents = true;


                for (int i = 0; i < 5; i++)
                {
                    if (Keyboard_Simuator == null)
                        Keyboard_Simuator = new InputSimulator();

                    Keyboard_Simuator.Keyboard.KeyPress(VirtualKeyCode.F20);
                }

            }
            else
                NotElevated();
        }

        private void NotElevated()
        {
            Functions_Old.DoAction(this, new Action(delegate ()
            {
                lbl_CurrentSetting.Content = "Run as Admin Required";
                MessageBox.Show(this, "This proram must be run with Admin Permissions" + Environment.NewLine + Environment.NewLine + "Right click Program File then click - Run as administrator" + Environment.NewLine + Environment.NewLine + " or Right Click Program - Properties - Compatibility then Check - Run this program as an administrator", "This proram must be run with Admin Permissions", MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        #region Dash Buttons

        private void LinkDashesToButtons()
        {
            btn_ExitOculusLink.Tag = Dashes.Dash_Type.Exit;
            btn_Normal.Tag = Dashes.Dash_Type.Normal;
            btn_SteamVR.Tag = Dashes.Dash_Type.OculusKiller;
        }

        private void Generate_Hover_Buttons()
        {
            Oculus_Dash = new Hover_Button { Hover_Complete_Action = Oculus_Dash_Hover_Activate, Bar = pb_Normal, Check_SteamVR = true, Hovered_Seconds_To_Activate = Properties.Settings.Default.Hover_Activation_Time };
            Exit_Link = new Hover_Button { Hover_Complete_Action = Exit_Link_Hover_Activate, Bar = pb_Exit, Check_SteamVR = true, Hovered_Seconds_To_Activate = Properties.Settings.Default.Hover_Activation_Time };
            pb_Normal.Maximum = Properties.Settings.Default.Hover_Activation_Time * 1000;
            pb_Exit.Maximum = Properties.Settings.Default.Hover_Activation_Time * 1000;
        }

        private void Check_Hover(object sender, ElapsedEventArgs args)
        {
            CheckHovering(Oculus_Dash);
            CheckHovering(Exit_Link);
        }

        private void Enable_Hover_Button(Dashes.Dash_Type Dash)
        {
            switch (Dash)
            {
                case Dashes.Dash_Type.Exit:
                    Exit_Link.Enabled = true;
                    break;

                case Dashes.Dash_Type.Unknown:
                    break;

                case Dashes.Dash_Type.Normal:
                    Oculus_Dash.Enabled = true;
                    break;

                case Dashes.Dash_Type.OculusKiller:
                    break;

                default:
                    break;
            }
        }

        private void Reset_Hover_Buttons()
        {
            Oculus_Dash.Reset();
            Exit_Link.Reset();
        }

        private void Oculus_Dash_Hover_Activate()
        {
            Oculus_Dash.Bar.Value = 0;
            btn_ActivateDash_Click(btn_Normal, null);
        }

        private void Exit_Link_Hover_Activate()
        {
            Exit_Link.Bar.Value = 0;
            Software.Steam.Close_SteamVR_ResetLink();
        }

        private void Update_Dash_Buttons()
        {
            foreach (UIElement item in gd_DashButtons.Children)
            {
                if (item is Button button)
                {
                    if (button.Tag is Dashes.Dash_Type Dash)
                    {
                        bool Enabled = Dashes.Dash_Manager.IsInstalled(Dash);
                        button.IsEnabled = Enabled;
                        if (Enabled)
                            Enable_Hover_Button(Dash);
                    }
                }
            }

            btn_ExitOculusLink.IsEnabled = true;
        }

        #region Hover Buttons Enter/Leave

        private void btn_Normal_MouseEnter(object sender, MouseEventArgs e)
        {
            Oculus_Dash.SetHovering();
        }

        private void btn_Normal_MouseLeave(object sender, MouseEventArgs e)
        {
            Oculus_Dash.StopHovering();
        }

        private void btn_ExitOculusLink_MouseEnter(object sender, MouseEventArgs e)
        {
            Exit_Link.SetHovering();
        }

        private void btn_ExitOculusLink_MouseLeave(object sender, MouseEventArgs e)
        {
            Exit_Link.StopHovering();
        }

        #endregion Hover Buttons Enter/Leave

        private void btn_ActivateDash_Click(object sender, RoutedEventArgs e)
        {
            Disable_Dash_Buttons();

            if (sender is Button button)
                ActivateDash(button);

            Thread ReactivateButtons = new Thread(Thread_ReactivateButtons);
            ReactivateButtons.IsBackground = true;
            ReactivateButtons.Start();

            CheckRunTime();
        }

        private void Thread_ReactivateButtons()
        {
            Thread.Sleep(5000);
            Functions_Old.DoAction(this, new Action(delegate () { Update_Dash_Buttons(); }));
        }

        private void btn_OpenDashLocation_Click(object sender, RoutedEventArgs e)
        {
            if (Software.Oculus.Oculus_Is_Installed)
            {
                if (Directory.Exists(Software.Oculus.Oculus_Dash_Directory))
                    Functions_Old.ShowFileInDirectory(Software.Oculus.Oculus_Dash_Directory);
            }
        }

        #endregion Dash Buttons

        #region URL Links

        private void lbl_ItsKaitlyn03_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Functions_Old.OpenURL("https://github.com/ItsKaitlyn03/OculusKiller");
        }

        private void lbl_Title_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Functions_Old.OpenURL("https://github.com/KrisIsBackAU/Oculus-VR-Dash-Manager");
        }

        #endregion URL Links

        #region Dynamic Functions

        private void ActivateDash(Button Clicked)
        {
            MoveMouseToElement(lbl_CurrentSetting);
            Reset_Hover_Buttons();

            if (Clicked.Tag is Dashes.Dash_Type Dash)
            {
                if (Dash == Dashes.Dash_Type.Exit || Dashes.Dash_Manager.IsInstalled(Dash))
                {
                    if (Properties.Settings.Default.FastSwitch)
                        Dashes.Dash_Manager.Activate_FastTransition(Dash);
                    else
                        Dashes.Dash_Manager.Activate(Dash);

                    Software.Oculus.Check_Current_Dash();

                    lbl_CurrentSetting.Content = Software.Oculus.Current_Dash_Name;
                }
                else
                    lbl_CurrentSetting.Content = Dashes.Dash_Manager.GetDashName(Dash) + " Not Installed";
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

        private void CheckHovering(Hover_Button Button)
        {
            if (Button.Hovering)
            {
                if (Button.Check_SteamVR)
                {
                    if (!Properties.Settings.Default.Ignore_SteamVR_Status_HoverButtonAction)
                    {
                        if (!Software.Steam.Steam_VR_Server_Running)
                            return;
                    }
                }

                TimeSpan Passed = DateTime.Now.Subtract(Button.Hover_Started);
                Functions_Old.DoAction(this, new Action(delegate () { Button.Bar.Value = Passed.TotalMilliseconds; }));

                if (Passed.TotalSeconds >= Button.Hovered_Seconds_To_Activate)
                    Functions_Old.DoAction(this, new Action(delegate () { Button.Reset(); Button.Bar.Value = Button.Bar.Maximum; MoveMouseToElement(lbl_CurrentSetting); Button.Hover_Complete_Action.Invoke(); }));
            }
        }

        private void MoveMouseToElement(FrameworkElement Element)
        {
            Point relativePoint = Element.TransformToAncestor(this).Transform(new Point(0, 0));
            Point pt = new Point(relativePoint.X + Element.ActualWidth / 2, relativePoint.Y + Element.ActualHeight / 2);
            Point windowCenterPoint = pt;//new Point(125, 80);
            Point centerPointRelativeToSCreen = this.PointToScreen(windowCenterPoint);
            Functions_Old.MoveCursor((int)centerPointRelativeToSCreen.X, (int)centerPointRelativeToSCreen.Y);
        }

        #endregion Dynamic Functions

        #region Forms

        private void OpenForm(Window Form, Boolean DialogMode = true)
        {
            Topmost = false;

            if (DialogMode)
            {
                Form.ShowDialog();
                Topmost = Properties.Settings.Default.AlwaysOnTop;
                RefreshUI();
            }
            else
                Form.Show();
        }

        private void btn_OculusServiceManager_Click(object sender, RoutedEventArgs e)
        {
            if (!FireUIEvents)
                return;

            Forms.frm_Oculus_Service_Control ServiceControl = new Forms.frm_Oculus_Service_Control();
            OpenForm(ServiceControl);
        }

        private void btn_OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            Forms.Settings.frm_Settings_v2 Settings = new Forms.Settings.frm_Settings_v2();
            //Forms.frm_Settings Settings = new Forms.frm_Settings();
            OpenForm(Settings);
        }

        private bool Get_Properties_Setting(String SettingName)
        {
            bool Setting = false;

            try
            {
                Setting = (bool)Properties.Settings.Default[SettingName];
            }
            catch (Exception)
            {
                return false;
            }

            return Setting;
        }

        private void btn_Diagnostics_Click(object sender, RoutedEventArgs e)
        {
            Forms.frm_Diagnostics Settings = new Forms.frm_Diagnostics();
            OpenForm(Settings);
        }

        private void btn_CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            if (!FireUIEvents)
                return;

            Forms.frm_UpdateChecker Settings = new Forms.frm_UpdateChecker();
            OpenForm(Settings);
        }

        private void btn_Help_Click(object sender, RoutedEventArgs e)
        {
            Forms.frm_Help Settings = new Forms.frm_Help();
            OpenForm(Settings);
        }

        private void lbl_TestAccess_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftAlt))
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Forms.frm_TestWindow TestWindow = new Forms.frm_TestWindow();
                    OpenForm(TestWindow, false);
                }
            }
        }

        private void btn_OpenSteamVRSettings_Click(object sender, RoutedEventArgs e)
        {
            Forms.frm_SteamVR_Settings Settings = new Forms.frm_SteamVR_Settings();
            OpenForm(Settings);
        }

        #endregion Forms

        public void Cancel_TaskView_And_Focus()
        {
            if (Keyboard_Simuator == null)
                Keyboard_Simuator = new InputSimulator();

            Keyboard_Simuator.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);

            this.Topmost = true;
            this.BringIntoView();
            this.Topmost = Properties.Settings.Default.AlwaysOnTop;
        }

        #region OpenXR Runtime

        private void btn_RunTime_SteamVR_Checked(object sender, RoutedEventArgs e)
        {
            if (!FireUIEvents)
                return;

            Software.Steam_VR_Settings.Set_SteamVR_Runtime();
        }

        private void btn_RunTime_Oculus_Checked(object sender, RoutedEventArgs e)
        {
            if (!FireUIEvents)
                return;

            Software.Oculus_Link.SetToOculusRunTime();
        }

        public void CheckRunTime()
        {
            Software.Steam_VR_Settings.OpenXR_Runtime CurrentRuntime = Software.Steam_VR_Settings.Read_Runtime();

            if (CurrentRuntime == Software.Steam_VR_Settings.OpenXR_Runtime.Oculus)
                Functions_Old.DoAction(this, new Action(delegate () { btn_RunTime_Oculus.IsChecked = true; }));

            if (CurrentRuntime == Software.Steam_VR_Settings.OpenXR_Runtime.SteamVR)
                Functions_Old.DoAction(this, new Action(delegate () { btn_RunTime_SteamVR.IsChecked = true; }));
        }

        #endregion OpenXR Runtime

        public void ErrorLog(Exception e)
        {
            File.AppendAllText("ErrorLog.txt", Environment.NewLine +
                                               Environment.NewLine +
                                               " ------ " +
                                               DateTime.Now.ToString(CultureInfo.InvariantCulture) +
                                               " ------" +
                                               Environment.NewLine +
                                               e.Message +
                                               Environment.NewLine +
                                               e.StackTrace +
                                               Environment.NewLine +
                                               e.TargetSite);
        }

        private static void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs ex)
        {
            Exception e = ex.Exception;

            // log or handle exception
            ex.Handled = true;
            File.AppendAllText("ErrorLog.txt", Environment.NewLine +
                       Environment.NewLine +
                       " ------ " +
                       DateTime.Now.ToString(CultureInfo.InvariantCulture) +
                       " ------" +
                       Environment.NewLine +
                       e.Message +
                       Environment.NewLine +
                       e.StackTrace +
                       Environment.NewLine +
                       e.TargetSite);
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs ex)
        {
            Exception e = (Exception)ex.ExceptionObject;
            File.AppendAllText("ErrorLog.txt", Environment.NewLine +
                                   Environment.NewLine +
                                   " ------ " +
                                   DateTime.Now.ToString(CultureInfo.InvariantCulture) +
                                   " ------" +
                                   Environment.NewLine +
                                   e.Message +
                                   Environment.NewLine +
                                   e.StackTrace +
                                   Environment.NewLine +
                                   e.TargetSite);

            // log or handle exception
        }
    }
}