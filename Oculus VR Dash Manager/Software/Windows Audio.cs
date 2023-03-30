using PlaybackDeviceSwitcher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace OVR_Dash_Manager.Software
{
    public static class Windows_Audio
    {
        public static List<PlayBackDevice_Ext> Speakers;

        public static void Setup()
        {
            PlayBackDevice[] outputs = PlaybackDeviceSwitcherWrapper.GetPlayBackDevices();
            Speakers = new List<PlayBackDevice_Ext>();

            foreach (PlayBackDevice item in outputs)
            {
                PlayBackDevice_Ext Speaker = new PlayBackDevice_Ext(item);

                if (Speaker.ID == Properties.Settings.Default.Normal_Speaker_ID)
                    Speaker.Normal_Speaker = true;

                if (Speaker.ID == Properties.Settings.Default.Quest_Speaker_ID)
                    Speaker.Quest_Speaker = true;

                Speakers.Add(Speaker);
            }
        }

        public static void Set_To_Normal_Speaker_Auto(Boolean Force = false)
        {
            if (Properties.Settings.Default.Automatic_Audio_Switching || Force)
            {
                PlayBackDevice_Ext Speaker = Speakers.FirstOrDefault(a => a.ID == Properties.Settings.Default.Normal_Speaker_ID);
                if (Speaker != null)
                    SetDefaultPlayBackDevice(Speaker.ID);
            }
        }

        public static void Set_To_Quest_Speaker_Auto(Boolean Force = false)
        {
            if (Properties.Settings.Default.Automatic_Audio_Switching || Force)
            {
                PlayBackDevice_Ext Speaker = Speakers.FirstOrDefault(a => a.ID == Properties.Settings.Default.Quest_Speaker_ID);
                if (Speaker != null)
                    SetDefaultPlayBackDevice(Speaker.ID);
            }
        }

        public static void SetDefaultPlayBackDevice(int ID)
        {
            PlaybackDeviceSwitcherWrapper.SetDefaultPlayBackDevice(ID);
            Debug.WriteLine($"Switched to device " + ID);
        }

        public class PlayBackDevice_Ext : INotifyPropertyChanged
        {
            // Add To Class : INotifyPropertyChanged
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
            #endregion

            public PlayBackDevice_Ext(PlayBackDevice Speaker)
            {
                this.Name = Speaker.Name;
                this.ID = Speaker.Id;
            }

            private bool _Normal_Speaker;
            public bool Normal_Speaker
            {
                get { return _Normal_Speaker; }
                set { if (value != _Normal_Speaker) _Normal_Speaker = value; OnPropertyChanged("Normal_Speaker"); }
            }



            private bool _Quest_Speaker;
            public bool Quest_Speaker
            {
                get { return _Quest_Speaker; }
                set { if (value != _Quest_Speaker) _Quest_Speaker = value; OnPropertyChanged("Quest_Speaker"); }
            }

            private string _Name;
            public string Name
            {
                get { return _Name; }
                set { if (value != null || value != _Name) _Name = value; OnPropertyChanged("Name"); }
            }


            private int _ID;
            public int ID
            {
                get { return _ID; }
                set { if (value != _ID) _ID = value; OnPropertyChanged("ID"); }
            }

        }
    }
}
