using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace OVR_Dash_Manager.Forms
{
    /// <summary>
    /// Interaction logic for frm_UpdateChecker.xaml
    /// </summary>
    public partial class frm_UpdateChecker : Window
    {
        private GitHub_Reply ItsKaitlyn03_GitHub;

        public frm_UpdateChecker()
        {
            InitializeComponent();
        }

        private void btn_DashManager_OpenWebsite_Click(object sender, RoutedEventArgs e)
        {
            Functions_Old.OpenURL("https://github.com/KrisIsBackAU/Oculus-VR-Dash-Manager");
        }

        private void btn_ItsKaitlyn03_OpenWebsite_Click(object sender, RoutedEventArgs e)
        {
            Functions_Old.OpenURL("https://github.com/ItsKaitlyn03/OculusKiller");
        }

        private void Thread_CheckUpdates()
        {
            Check_DashManager_Update();
            Check_ItsKaitlyn03_Update();
        }

        private void Check_DashManager_Update()
        {
            Github Check = new Github();
            String Version = Check.GetLatestReleaseName("KrisIsBackAU", "Oculus-VR-Dash-Manager");
            String CurrentVersion = typeof(MainWindow).Assembly.GetName().Version.ToString();
            Functions_Old.DoAction(this, new Action(delegate () { lbl_DashManager_LastCheck.Content = DateTime.Now.ToString(); lbl_DashManager_CurrentVersion.Content = CurrentVersion; lbl_DashManager_AvaliableVersion.Content = Version; }));
        }

        private void Check_ItsKaitlyn03_Update()
        {
            Dashes.OVR_Dash ItsKaitlyn03 = Dashes.Dash_Manager.GetDash(Dashes.Dash_Type.OculusKiller);

            if (ItsKaitlyn03 == null)
                Functions_Old.DoAction(this, new Action(delegate () { lbl_ItsKaitlyn03_CurrentVersion.Content = "Not Loaded"; }));
            else if (!ItsKaitlyn03.Installed)
                Functions_Old.DoAction(this, new Action(delegate () { lbl_ItsKaitlyn03_CurrentVersion.Content = "Not Downloaded"; }));
            else
            {
                FileVersionInfo Info = FileVersionInfo.GetVersionInfo(Path.Combine(Software.Oculus.Oculus_Dash_Directory, ItsKaitlyn03.DashFileName));
                Functions_Old.DoAction(this, new Action(delegate () { lbl_ItsKaitlyn03_CurrentVersion.Content = Info.FileVersion; }));
            }

            Github Check = new Github();
            ItsKaitlyn03_GitHub = Check.GetLatestReleaseInfo("ItsKaitlyn03", "OculusKiller");
            Functions_Old.DoAction(this, new Action(delegate () { lbl_ItsKaitlyn03_LastCheck.Content = DateTime.Now.ToString(); lbl_ItsKaitlyn03_AvaliableVersion.Content = ItsKaitlyn03_GitHub.Release_Version; }));

            if (ItsKaitlyn03_GitHub.AssetURLs.ContainsKey("OculusDash.exe"))
                Functions_Old.DoAction(this, new Action(delegate () { btn_ItsKaitlyn03_Download.IsEnabled = true; }));
        }

        private void Check_Offical_Update()
        {
            Dashes.OVR_Dash Offical = Dashes.Dash_Manager.GetDash(Dashes.Dash_Type.Normal);
            FileVersionInfo Info = FileVersionInfo.GetVersionInfo(Path.Combine(Software.Oculus.Oculus_Dash_Directory, Offical.DashFileName));


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lbl_DashManager_LastCheck.Content = "Checking";
            lbl_ItsKaitlyn03_LastCheck.Content = "Checking";

            lbl_DashManager_CurrentVersion.Content = "";
            lbl_DashManager_AvaliableVersion.Content = "";

            lbl_ItsKaitlyn03_CurrentVersion.Content = "";
            lbl_ItsKaitlyn03_AvaliableVersion.Content = "";

            Thread CheckUpdate = new Thread(Thread_CheckUpdates);
            CheckUpdate.IsBackground = true;
            CheckUpdate.Start();
        }

        private void btn_ItsKaitlyn03_Download_Click(object sender, RoutedEventArgs e)
        {
            Dashes.OVR_Dash ItsKaitlyn03 = Dashes.Dash_Manager.GetDash(Dashes.Dash_Type.OculusKiller);
            ItsKaitlyn03.Download();
            Check_ItsKaitlyn03_Update();
        }
    }
}