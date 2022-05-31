using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;

namespace OVR_Dash_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Boolean FireUIEvents = false;
        private Hover_Button Oculus_Dash;
        private Hover_Button Exit_Link;
        private InputSimulator Keyboard_Simuator;

        public MainWindow()
        {
            InitializeComponent();

            Title += " v" + typeof(MainWindow).Assembly.GetName().Version;
            Topmost = Properties.Settings.Default.AlwaysOnTop;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Disable_Dash_Buttons();

            Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Starting Up"; }));

            LinkDashesToButtons();
            Generate_Hover_Buttons();

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

        private void Generate_Hover_Buttons()
        {
            Oculus_Dash = new Hover_Button { Hover_Complete_Action = Oculus_Dash_Hover_Activate, Bar = pb_Normal, Check_SteamVR = true };
            Exit_Link = new Hover_Button { Hover_Complete_Action = Exit_Link_Hover_Activate, Bar = pb_Exit, Check_SteamVR = true };
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
            btn_ActivateDash_Click(btn_ExitOculusLink, null);
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

        private void Startup()
        {
            if (Functions.IsCurrentProcess_Elevated())
            {
                if (Debugger.IsAttached)
                    Dashes.Dash_Manager.PassMainForm(this);

                Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Checking Installed Dashes & Updates"; }));
                Dashes.Dash_Manager.GenerateDashes();

                if (!Oculus_Software.OculusInstalled)
                {
                    Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Oculus Directory Not Found"; }));
                    return;
                }

                if (!Dashes.Dash_Manager.Oculus_Offical_Dash_Installed())
                {
                    Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Offical Oculus Dash Not Found, Replace Original Oculus Dash"; }));
                    return;
                }

                Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Starting SteamVR Watcher"; }));
                SteamVR.Setup();
                SteamVR.StartTimer();

                Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Starting Hover Buttons"; }));

                Timer_Functions.CreateTimer("Hover Checker", TimeSpan.FromMilliseconds(250), Check_Hover);
                Timer_Functions.StartTimer("Hover Checker");

                Timer_Functions.CreateTimer("SteamVR Focus Fix", TimeSpan.FromSeconds(1), Check_SteamVR_FocusProblem);
                Timer_Functions.StartTimer("SteamVR Focus Fix");

                Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Starting Service Manager"; }));

                Service_Manager.RegisterService("OVRLibraryService");
                Service_Manager.RegisterService("OVRService");

                if (Properties.Settings.Default.RunOculusClientOnStartup)
                {
                    if (!Debugger.IsAttached)
                    {
                        Functions.DoAction(this, new Action(delegate () { lbl_CurrentSetting.Content = "Starting Oculus Client"; }));
                        Oculus_Software.StartOculusClient();
                    }
                }

                Functions.DoAction(this, new Action(delegate ()
                {
                    lbl_CurrentSetting.Content = Oculus_Software.CustomDashName;
                    Update_Dash_Buttons();
                }));

                FireUIEvents = true;
            }
            else
                NotElevated();
        }

        #region URL Links

        private void lbl_ItsKaitlyn03_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Functions.OpenURL("https://github.com/ItsKaitlyn03/OculusKiller");
        }

        private void lbl_Title_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Functions.OpenURL("https://github.com/KrisIsBackAU/Oculus-VR-Dash-Manager");
        }

        #endregion URL Links

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

        #region Static Functions

        private void img_Discord_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Functions.OpenURL("https://github.com/KrisIsBackAU/Oculus-VR-Dash-Manager");
        }

        private void NotElevated()
        {
            Functions.DoAction(this, new Action(delegate ()
            {
                lbl_CurrentSetting.Content = "Run as Admin Required";
                MessageBox.Show(this, "This proram must be run with Admin Permissions" + Environment.NewLine + Environment.NewLine + "Right click Program File then click - Run as administrator" + Environment.NewLine + Environment.NewLine + " or Right Click Program - Properties - Compatibility then Check - Run this program as an administrator", "This proram must be run with Admin Permissions", MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SteamVR.StopTimer();
            SteamVR.Dispose();

            Timer_Functions.StopTimer("Hover Checker");
            Timer_Functions.DisposeTimer("Hover Checker");

            StopOculusServices();
        }

        private void btn_OpenDashLocation_Click(object sender, RoutedEventArgs e)
        {
            if (Oculus_Software.OculusInstalled)
                Functions.ShowFileInDirectory(Oculus_Software.OculusDashFile);
        }

        #endregion Static Functions

        #region Dynamic Functions

        private void btn_ActivateDash_Click(object sender, RoutedEventArgs e)
        {
            Disable_Dash_Buttons();

            if (sender is Button button)
                ActivateDash(button);

            Thread ReactivateButtons = new Thread(Thread_ReactivateButtons);
            ReactivateButtons.IsBackground = true;
            ReactivateButtons.Start();
        }

        private void Thread_ReactivateButtons()
        {
            Thread.Sleep(10000);
            Functions.DoAction(this, new Action(delegate () { Update_Dash_Buttons(); }));
        }

        private void ActivateDash(Button Clicked)
        {
            MoveMouseToElement(lbl_CurrentSetting);
            Reset_Hover_Buttons();

            if (Clicked.Tag is Dashes.Dash_Type Dash)
            {
                if (Properties.Settings.Default.FastSwitch)
                    Dashes.Dash_Manager.Activate_FastTransition(Dash);
                else
                    Dashes.Dash_Manager.Activate(Dash);

                Oculus_Software.Check_Current_Dash();

                lbl_CurrentSetting.Content = Oculus_Software.CustomDashName;
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
                    if (!SteamVR.SteamVR_Running)
                        return;
                }

                TimeSpan Passed = DateTime.Now.Subtract(Button.Hover_Started);
                Functions.DoAction(this, new Action(delegate () { Button.Bar.Value = Passed.TotalMilliseconds; }));

                if (Passed.TotalSeconds >= Button.Hovered_Seconds_To_Activate)
                    Functions.DoAction(this, new Action(delegate () { Button.Reset(); Button.Bar.Value = Button.Bar.Maximum; MoveMouseToElement(lbl_CurrentSetting); Button.Hover_Complete_Action.Invoke(); }));
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

        #endregion Dynamic Functions

        #region Forms

        private void OpenForm(Window Form, Boolean DialogMode = true)
        {
            Topmost = false;

            if (DialogMode)
            {
                Form.ShowDialog();
                Topmost = Properties.Settings.Default.AlwaysOnTop;
            }
            else
                Form.Show();
        }

        private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Topmost = Properties.Settings.Default.AlwaysOnTop;
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
            Forms.frm_Settings Settings = new Forms.frm_Settings();
            OpenForm(Settings);
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

        #endregion Forms

        #region Close Oculus Services

        private void StopOculusServices()
        {
            if (Properties.Settings.Default.CloseOculusClientOnExit)
            {
                if (Debugger.IsAttached)
                    return;

                Process[] Dashes = Process.GetProcessesByName("OculusClient");

                foreach (Process item in Dashes)
                    item.CloseMainWindow();
            }

            if (Properties.Settings.Default.CloseOculusServicesOnExit)
            {
                if (Debugger.IsAttached)
                    return;

                lbl_CurrentSetting.Content = "Stopping Oculus Services";
                lbl_CurrentSetting.UpdateLayout();

                if (Service_Manager.GetStartup("OVRLibraryService") == "Manual")
                    Service_Manager.StopService("OVRLibraryService");

                if (Service_Manager.GetStartup("OVRService") == "Manual")
                    Service_Manager.StopService("OVRService");
            }


        }

        #endregion Close Oculus Services

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

        public void Cancel_TaskView_And_Focus()
        {
            if (Keyboard_Simuator == null)
                Keyboard_Simuator = new InputSimulator();

            Keyboard_Simuator.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);

            this.Topmost = true;
            this.BringIntoView();
            this.Topmost = Properties.Settings.Default.AlwaysOnTop;

            SteamVR.FocusSteamVR();
        }

        #region SteamVR Focus Fix
        // Janky
        private void Check_SteamVR_FocusProblem(object sender, ElapsedEventArgs args)
        {
            if (SteamVR.SteamVR_Running)
            {
                if (Properties.Settings.Default.SteamVRFocusFix)
                {
                    switch (Functions.GetActiveWindowTitle())
                    {
                        case "Task View":
                            Dashes.Dash_Manager.FocusMainForm();
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        #endregion


    }
}