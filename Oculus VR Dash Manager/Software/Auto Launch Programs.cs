using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OVR_Dash_Manager.Software
{
    public static class Auto_Launch_Programs
    {
        public static List<Auto_Program> Programs;

        public static List<Auto_Program> Generate_List()
        {
            if (Programs != null)
                Programs.Clear();

            Programs = new List<Auto_Program>();

            try
            {
                String ProgramData = Properties.Settings.Default.Auto_Programs_JSON;

                List<Slim_Auto_Program> Slim_Programs = Functions.JSON_Functions.DeseralizeClass<List<Slim_Auto_Program>>(ProgramData);
                if (Slim_Programs.Count > 0)
                {
                    foreach (Slim_Auto_Program item in Slim_Programs)
                    {
                        try
                        {
                            Programs.Add(new Auto_Program(item.FullPath, item.Startup_Launch, item.Closing_Launch));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                Slim_Programs = null;
            }
            catch (Exception)
            {
            }

            return Programs;
        }

        public static void Run_Startup_Programs()
        {
            if (Programs != null)
            {
                foreach (Auto_Program item in Programs)
                {
                    if (item.Startup_Launch)
                        Functions.Process_Functions.StartProcess(item.Full_Path);
                }
            }
        }

        public static void Run_Closing_Programs()
        {
            if (Programs != null)
            {
                foreach (Auto_Program item in Programs)
                {
                    if (item.Closing_Launch)
                        Functions.Process_Functions.StartProcess(item.Full_Path);
                }
            }
        }

        public static void Save_Program_List()
        {
            if (Programs != null)
            {
                List<Slim_Auto_Program> Slim_Programs = new List<Slim_Auto_Program>();

                foreach (Auto_Program item in Programs)
                {
                    Slim_Programs.Add(new Slim_Auto_Program(item.Full_Path, item.Startup_Launch, item.Closing_Launch));
                    item.Changed = false;
                }

                Properties.Settings.Default.Auto_Programs_JSON = Functions.JSON_Functions.SerializeClass(Slim_Programs);
                Properties.Settings.Default.Save();
            }
        }

        public static void Add_New_Program(String Path)
        {
            if (Programs != null)
            {
                if (File.Exists(Path))
                {
                    try
                    {
                        Programs.Add(new Auto_Program(Path, false, false, true));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public static void Remove_Program(Auto_Program Program)
        {
            if (Programs != null)
                Programs.Remove(Program);
        }
    }

    public class Slim_Auto_Program
    {
        public Slim_Auto_Program()
        { }

        public Slim_Auto_Program(String FullPath, Boolean Startup_Launch, Boolean Closing_Launch)
        {
            this.FullPath = FullPath;
            this.Startup_Launch = Startup_Launch;
            this.Closing_Launch = Closing_Launch;
        }

        public String FullPath { get; set; }
        public Boolean Startup_Launch { get; set; }
        public Boolean Closing_Launch { get; set; }
    }

    public class Auto_Program : INotifyPropertyChanged
    {
        #region Notify Property Changed Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion Notify Property Changed Members

        public Auto_Program(String FilePath, Boolean Startup_Launch, Boolean Closing_Launch, Boolean Changed = false)
        {
            Full_Path = FilePath;
            _File_Name = Path.GetFileName(FilePath);
            _Folder_Path = Path.GetDirectoryName(FilePath);
            _Program_Found = File.Exists(FilePath);
            _Startup_Launch = Startup_Launch;
            _Closing_Launch = Closing_Launch;
            this.Changed = Changed;
            _Program_Icon = null;

            if (_Program_Found)
            {
                try
                {
                    using (Icon ico = Icon.ExtractAssociatedIcon(FilePath))
                    {
                        _Program_Icon = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public String Full_Path { get; set; }

        private ImageSource _Program_Icon;

        public ImageSource Program_Icon
        {
            get { return _Program_Icon; }
            set { if (value != null || value != _Program_Icon) _Program_Icon = value; OnPropertyChanged("Program_Icon"); }
        }

        private string _File_Name;

        public string File_Name
        {
            get { return _File_Name; }
            set { if (value != null || value != _File_Name) _File_Name = value; OnPropertyChanged("File_Name"); }
        }

        private string _Folder_Path;

        public string Folder_Path
        {
            get { return _Folder_Path; }
            set { if (value != null || value != _Folder_Path) _Folder_Path = value; OnPropertyChanged("Folder_Path"); }
        }

        private bool _Startup_Launch;

        public bool Startup_Launch
        {
            get { return _Startup_Launch; }
            set { if (value != _Startup_Launch) _Startup_Launch = value; OnPropertyChanged("Startup_Launch"); Changed = true; }
        }

        private bool _Closing_Launch;

        public bool Closing_Launch
        {
            get { return _Closing_Launch; }
            set { if (value != _Closing_Launch) _Closing_Launch = value; OnPropertyChanged("Closing_Launch"); Changed = true; }
        }

        private bool _Program_Found;

        public bool Program_Found
        {
            get { return _Program_Found; }
            set { if (value != _Program_Found) _Program_Found = value; OnPropertyChanged("Program_Found"); }
        }

        public bool Changed { get; set; }
    }
}