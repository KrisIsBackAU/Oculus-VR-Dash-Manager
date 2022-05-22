using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace OVR_Dash_Manager.Dashes
{
    public class OVR_Dash
    {
        public OVR_Dash(String DisplayName, String DashFileName, String ProductName = "", String RepoName = "", String ProjectName = "", String AssetName = "", String ProcessToStop = "")
        {
            this.DisplayName = DisplayName;
            this.DashFileName = DashFileName;
            this.ProductName = ProductName;
            this.RepoName = RepoName;
            this.ProjectName = ProjectName;
            this.AssetName = AssetName;
            this.ProcessToStop = ProcessToStop;
        }

        public readonly String DisplayName = "Dash";

        public String DashFileName = "Dash.exe";
        public String ProductName = "";

        public String RepoName = "";
        public String ProjectName = "";
        public String AssetName = "";
        public String ProcessToStop = "";

        private long _Size = -1;

        private bool _DashActive;

        public bool DashActive
        {
            get { return _DashActive; }
            private set { _DashActive = value; }
        }

        private bool _NeedUpdate;

        public bool NeedUpdate
        {
            get { return _NeedUpdate; }
            private set { _NeedUpdate = value; }
        }

        private bool _Installed;

        public bool Installed
        {
            get { return _Installed; }
            private set { _Installed = value; }
        }

        public bool IsThisYourDash(String Dash_ProductName)
        {
            return Dash_ProductName == ProductName;
        }

        public void CheckInstalled()
        {
            if (Directory.Exists(Oculus_Software.OculusDashDirectory))
            {
                String DashPath = Path.Combine(Oculus_Software.OculusDashDirectory, DashFileName);

                if (File.Exists(DashPath))
                {
                    _Installed = true;
                    CheckUpdate(DashPath);
                }
            }
        }

        private void CheckUpdate(String DashPath)
        {
            if (_Installed)
            {
                if (!String.IsNullOrEmpty(RepoName) && !String.IsNullOrEmpty(ProjectName) && !String.IsNullOrEmpty(AssetName))
                {
                    FileInfo CurrentDash = new FileInfo(DashPath);
                    _Size = CurrentDash.Length;

                    Github Repo = new Github();

                    long LatestSize = Repo.GetLatestSize(RepoName, ProjectName, AssetName);
                    if (LatestSize != _Size)
                        _NeedUpdate = true;
                }
            }
        }

        public void Download()
        {
            String Temp_DashPath = Path.Combine(Oculus_Software.OculusDashDirectory, DashFileName + ".tmp");
            String DashPath = Path.Combine(Oculus_Software.OculusDashDirectory, DashFileName);

            Github Repo = new Github();
            Repo.Download(RepoName, ProjectName, AssetName, Temp_DashPath);

            if (File.Exists(Temp_DashPath))
            {
                File.Move(Temp_DashPath, DashPath, true);
                _Installed = true;
            }
        }

        public bool Activate_Dash()
        {
            Boolean Activated = false;

            if (Installed)
            {
                try
                {
                    CloseProcessBeforeIfRequired();
                }
                catch (Exception)
                {
                    throw;
                }

                try
                {
                    String DashPath = Path.Combine(Oculus_Software.OculusDashDirectory, DashFileName);
                    if (File.Exists(DashPath))
                    {
                        File.Copy(DashPath, Oculus_Software.OculusDashFile, true);
                        Activated = true;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return Activated;
        }

        private void CloseProcessBeforeIfRequired()
        {
            if (!String.IsNullOrEmpty(ProcessToStop))
            {
                Process[] SteamVR = Process.GetProcessesByName(ProcessToStop);
                foreach (Process VR in SteamVR)
                    VR.Kill();

                Thread.Sleep(1000);
            }
        }
    }
}