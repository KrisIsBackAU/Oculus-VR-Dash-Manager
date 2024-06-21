using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OVR_Dash_Manager.Forms.Settings
{
    /// <summary>
    /// Interaction logic for frm_Settings_v2.xaml
    /// </summary>
    public partial class frm_Settings_v2 : Window
    {
        private bool Audio_DevicesChanged = false;
        private bool FireEvents = false;

        public frm_Settings_v2()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbo_NormalSpeaker.ItemsSource = Software.Windows_Audio_v2.Speakers;
            cbo_QuestSpeaker.ItemsSource = Software.Windows_Audio_v2.Speakers;

            cbo_NormalMicrophone.ItemsSource = Software.Windows_Audio_v2.Microphones;
            cbo_QuestMicrophone.ItemsSource = Software.Windows_Audio_v2.Microphones;

            cbo_NormalSpeaker.SelectedItem = Software.Windows_Audio_v2.Speakers.FirstOrDefault(a => a.Normal_Speaker);
            cbo_QuestSpeaker.SelectedItem = Software.Windows_Audio_v2.Speakers.FirstOrDefault(a => a.Quest_Speaker);

            cbo_NormalMicrophone.SelectedItem = Software.Windows_Audio_v2.Microphones.FirstOrDefault(a => a.Normal_Speaker);
            cbo_QuestMicrophone.SelectedItem = Software.Windows_Audio_v2.Microphones.FirstOrDefault(a => a.Quest_Speaker);

            Audio_DevicesChanged = false;
            FireEvents = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Audio_DevicesChanged)
            {
                if (MessageBox.Show(this, "Automatic Audio Devices Changed - Are you sure you want to save this ?", "Confirm Automatic Audio Device Change", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Software.Windows_Audio_v2.IDevice_Ext NormalSpeaker = Software.Windows_Audio_v2.Speakers.FirstOrDefault(a => a.Normal_Speaker);
                    Software.Windows_Audio_v2.IDevice_Ext QuestSpeaker = Software.Windows_Audio_v2.Speakers.FirstOrDefault(a => a.Quest_Speaker);

                    Software.Windows_Audio_v2.IDevice_Ext NormalMicrophone = Software.Windows_Audio_v2.Microphones.FirstOrDefault(a => a.Normal_Speaker);
                    Software.Windows_Audio_v2.IDevice_Ext QuestMicrophone = Software.Windows_Audio_v2.Microphones.FirstOrDefault(a => a.Quest_Speaker);

                    Properties.Settings.Default.Normal_Speaker_GUID = NormalSpeaker != null ? NormalSpeaker.ID : new System.Guid();
                    Properties.Settings.Default.Quest_Speaker_GUID = QuestSpeaker != null ? QuestSpeaker.ID : new System.Guid();

                    Properties.Settings.Default.Normal_Microphone_GUID = NormalMicrophone != null ? NormalMicrophone.ID : new System.Guid();
                    Properties.Settings.Default.Quest_Microphone_GUID = QuestMicrophone != null ? QuestMicrophone.ID : new System.Guid();

                    Properties.Settings.Default.Save();
                }
            }
        }

        private void CheckSpeaker(Software.Windows_Audio_v2.IDevice_Ext Speaker, bool Checked, bool Normal, bool Quest)
        {
            if (!FireEvents)
                return;

            FireEvents = false;

            // Force reset to make sure it cant be the same audio device
            Audio_DevicesChanged = true;
            Speaker.Quest_Speaker = false;
            Speaker.Normal_Speaker = false;

            if (Normal)
            {
                Software.Windows_Audio_v2.Speakers.ForEach(a => a.Normal_Speaker = false); // Remove all normals and just set the current
                Speaker.Normal_Speaker = Checked;
            }

            if (Quest)
            {
                Software.Windows_Audio_v2.Speakers.ForEach(a => a.Quest_Speaker = false); // Remove all quests and just set the current
                Speaker.Quest_Speaker = Checked;
            }

            FireEvents = true;
        }

        private void CheckMicrophone(Software.Windows_Audio_v2.IDevice_Ext Speaker, bool Checked, bool Normal, bool Quest)
        {
            if (!FireEvents)
                return;

            FireEvents = false;

            // Force reset to make sure it cant be the same audio device
            Audio_DevicesChanged = true;
            Speaker.Quest_Speaker = false;
            Speaker.Normal_Speaker = false;

            if (Normal)
            {
                Software.Windows_Audio_v2.Microphones.ForEach(a => a.Normal_Speaker = false); // Remove all normals and just set the current
                Speaker.Normal_Speaker = Checked;
            }

            if (Quest)
            {
                Software.Windows_Audio_v2.Microphones.ForEach(a => a.Quest_Speaker = false); // Remove all quests and just set the current
                Speaker.Quest_Speaker = Checked;
            }

            FireEvents = true;
        }

        private void btn_Set_Default_Normal_Click(object sender, RoutedEventArgs e)
        {
            Software.Windows_Audio_v2.IDevice_Ext NormalSpeaker = Software.Windows_Audio_v2.Speakers.FirstOrDefault(a => a.Normal_Speaker);
            if (NormalSpeaker != null)
                Software.Windows_Audio_v2.Set_Default_PlaybackDevice(NormalSpeaker.ID);
            else
                MessageBox.Show(this, "Normal Speaker Not Found", "Normal Speaker Not Found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void btn_Set_Default_Quest_Click(object sender, RoutedEventArgs e)
        {
            Software.Windows_Audio_v2.IDevice_Ext QuestSpeaker = Software.Windows_Audio_v2.Speakers.FirstOrDefault(a => a.Quest_Speaker);
            if (QuestSpeaker != null)
                Software.Windows_Audio_v2.Set_Default_PlaybackDevice(QuestSpeaker.ID);
            else
                MessageBox.Show(this, "Quest Speaker Not Set", "Quest Speaker Not Set", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void cbo_NormalSpeaker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbo_NormalSpeaker.SelectedItem is Software.Windows_Audio_v2.IDevice_Ext Speaker)
            {
                CheckSpeaker(Speaker, true, true, false);
            }
        }

        private void cbo_QuestSpeaker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbo_QuestSpeaker.SelectedItem is Software.Windows_Audio_v2.IDevice_Ext Speaker)
            {
                CheckSpeaker(Speaker, true, false, true);
            }
        }

        private void cbo_NormalMicrophone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbo_NormalMicrophone.SelectedItem is Software.Windows_Audio_v2.IDevice_Ext Speaker)
            {
                CheckMicrophone(Speaker, true, true, false);
            }
        }

        private void cbo_QuestMicrophone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbo_QuestMicrophone.SelectedItem is Software.Windows_Audio_v2.IDevice_Ext Speaker)
            {
                CheckMicrophone(Speaker, true, false, true);
            }
        }

        private void btn_Open_Auto_Launch_Settings_Click(object sender, RoutedEventArgs e)
        {
            Auto_Program_Launch.frm_Auto_Program_Launch_Settings pShow = new Auto_Program_Launch.frm_Auto_Program_Launch_Settings();
            pShow.ShowDialog();
        }

        private void btn_Set_Default_Microphone_Normal_Click(object sender, RoutedEventArgs e)
        {
            Software.Windows_Audio_v2.IDevice_Ext NormalSpeaker = Software.Windows_Audio_v2.Microphones.FirstOrDefault(a => a.Normal_Speaker);
            if (NormalSpeaker != null)
                Software.Windows_Audio_v2.Set_Default_CaptureDevice(NormalSpeaker.ID);
            else
                MessageBox.Show(this, "Normal Microphone Not Found", "Normal Microphone Not Found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void btn_Set_Default_Microphone_Quest_Click(object sender, RoutedEventArgs e)
        {
            Software.Windows_Audio_v2.IDevice_Ext QuestSpeaker = Software.Windows_Audio_v2.Microphones.FirstOrDefault(a => a.Quest_Speaker);
            if (QuestSpeaker != null)
                Software.Windows_Audio_v2.Set_Default_CaptureDevice(QuestSpeaker.ID);
            else
                MessageBox.Show(this, "Quest Microphone Not Found", "Quest Microphone Not Found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}