using System;
using System.Diagnostics;
using System.IO;

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

        private string _CurrentVersion;

        public string CurrentVersion
        {
            get { return _CurrentVersion; }
            private set { _CurrentVersion = value; }
        }

        private string _AvaliableVersion;

        public string AvaliableVersion
        {
            get { return _AvaliableVersion; }
            private set { _AvaliableVersion = value; }
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
                    _Installed = true;
            }
        }

        public void CheckUpdate()
        {
            if (_Installed)
            {
                if (!String.IsNullOrEmpty(RepoName) && !String.IsNullOrEmpty(ProjectName) && !String.IsNullOrEmpty(AssetName))
                {
                    String DashPath = Path.Combine(Oculus_Software.OculusDashDirectory, DashFileName);
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
            Boolean Active_NeedUpdateTo = false;
            Boolean ShouldUpdate = true;

            if (_DashActive)
            {
                try
                {
                    // try and remove live version of this dash (we always have our backup incase)
                    File.Delete(Oculus_Software.OculusDashFile);
                    Active_NeedUpdateTo = true;
                }
                catch (Exception)
                {
                    ShouldUpdate = false;
                }
            }

            if (ShouldUpdate)
            {
                Github Repo = new Github();
                try
                {
                    Repo.Download(RepoName, ProjectName, AssetName, Temp_DashPath);
                }
                catch (Exception)
                {
                }

                if (File.Exists(Temp_DashPath))
                {
                    try
                    {
                        File.Move(Temp_DashPath, DashPath, true);
                        _Installed = true;

                        if (Active_NeedUpdateTo)
                            File.Copy(DashPath, Oculus_Software.OculusDashFile, true);
                    }
                    catch (Exception ex)
                    {
                        // try and update unless its already running, but just checked that .. hmm
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            if (_DashActive)
            {
                if (!File.Exists(Oculus_Software.OculusDashFile))
                    File.Copy(DashPath, Oculus_Software.OculusDashFile, true); // Copy backup file back incase something failed above
            }
        }

        public bool Activate_Dash()
        {
            Boolean Activated = false;

            if (Installed)
            {

                try
                {
                    if (!String.IsNullOrEmpty(ProcessToStop))
                    {
                        Debug.WriteLine("Attempting to kill: " + ProcessToStop);
                        CloseProcessBeforeIfRequired();
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("Unable to kill required process: " + ProcessToStop);
                    throw;
                }

                String DashPath = Path.Combine(Oculus_Software.OculusDashDirectory, DashFileName);

                try
                {
                    Activated = SwitchFiles(DashPath, Oculus_Software.OculusDashFile);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("because it is being used by another process"))
                        Activated = false;
                    else
                        throw;
                }
            }

            return Activated;
        }

        public bool Activate_Dash_Fast()
        {
            Boolean Activated = false;

            if (Installed)
            {
                Process[] Dashes = Process.GetProcessesByName("OculusDash");
                if (Dashes.Length > 0)
                {
                    Process RunningDash = Dashes[0];
                    if (!RunningDash.HasExited)
                    {
                        Debug.WriteLine("Killing: " + RunningDash.Id);
                        RunningDash.Kill();
                    }
                }

                String DashPath = Path.Combine(Oculus_Software.OculusDashDirectory, DashFileName);

                try
                {
                    Activated = SwitchFiles(DashPath, Oculus_Software.OculusDashFile);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("because it is being used by another process"))
                        Activated = false;
                    else
                        throw;
                }

                try
                {
                    if (!String.IsNullOrEmpty(ProcessToStop))
                    {
                        Debug.WriteLine("Attempting to kill: " + ProcessToStop);
                        CloseProcessBeforeIfRequired();
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("Unable to kill required process: " + ProcessToStop);
                    throw;
                }
            }

            return Activated;
        }

        public bool Activate_Dash_Fast_v2()
        {
            Boolean Activated = false;

            if (Installed)
            {
                String DashPath = Path.Combine(Oculus_Software.OculusDashDirectory, DashFileName);

                try
                {
                    if (File.Exists(Oculus_Software.OculusDashFile))
                    {
                        Debug.WriteLine("Moving Active Dash");

                        File.Move(Oculus_Software.OculusDashFile, $"{Oculus_Software.OculusDashFile}.delete", true);
                    }

                    Debug.WriteLine("Switching in New Dash");
                    Activated = SwitchFiles(DashPath, Oculus_Software.OculusDashFile);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("because it is being used by another process"))
                        Activated = false;
                    else
                        throw;
                }

                Process[] Dashes = Process.GetProcessesByName("OculusDash");
                if (Dashes.Length > 0)
                {
                    Process RunningDash = Dashes[0];
                    if (!RunningDash.HasExited)
                    {
                        Debug.WriteLine("Killing Dash: " + RunningDash.Id);
                        RunningDash.Kill();

                        if (File.Exists($"{Oculus_Software.OculusDashFile}.delete"))
                        {
                            RunningDash.WaitForExit();
                            File.Delete($"{Oculus_Software.OculusDashFile}.delete");
                            Debug.WriteLine("Removed Old Dash File");
                        }
                    }
                }

                try
                {
                    if (File.Exists($"{Oculus_Software.OculusDashFile}.delete"))
                    {
                        File.Delete($"{Oculus_Software.OculusDashFile}.delete");
                        Debug.WriteLine("Removed Old Dash File");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed removing old file: " + ex.Message);
                }

                try
                {
                    if (!String.IsNullOrEmpty(ProcessToStop))
                    {
                        Debug.WriteLine("Attempting to kill: " + ProcessToStop);
                        CloseProcessBeforeIfRequired();
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("Unable to kill required process: " + ProcessToStop);
                    throw;
                }
            }

            return Activated;
        }

        private bool SwitchFiles(String NewFile, String OldFile)
        {
            bool Activated = false;
            
            String DashPath = Path.Combine(Oculus_Software.OculusDashDirectory, DashFileName);
            if (File.Exists(NewFile))
            {
                File.Copy(NewFile, OldFile, true);

                FileInfo Source = new FileInfo(NewFile);
                FileInfo Target = new FileInfo(OldFile);

                if (Source.Length == Target.Length)
                    Activated = true;
            }

            return Activated;
        }

        private void CloseProcessBeforeIfRequired()
        {
            if (!String.IsNullOrEmpty(ProcessToStop))
            {
                Process[] SteamVR = Process.GetProcessesByName(ProcessToStop);
                foreach (Process VR in SteamVR)
                    VR.CloseMainWindow();
            }
        }
    }
}