using System;
using System.Windows;
using System.Windows.Controls;

namespace OVR_Dash_Manager.Forms.Settings
{
    /// <summary>
    /// Interaction logic for uc_Setting.xaml
    /// </summary>
    public partial class uc_Setting : UserControl
    {
        public uc_Setting()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string Setting { get; set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Update_Buttons();
        }

        private void btn_Disabled_Checked(object sender, RoutedEventArgs e)
        {
            Update_Properties_Setting(false);
        }

        private void btn_Enabled_Checked(object sender, RoutedEventArgs e)
        {
            Update_Properties_Setting(true);
        }

        private void Update_Buttons()
        {
            bool Current = Get_Properties_Setting(Setting);

            btn_Enabled.IsChecked = Current;
            btn_Disabled.IsChecked = !Current;
        }

        private void Update_Properties_Setting(bool Value)
        {
            bool Current;

            try
            {
                Current = Get_Properties_Setting(Setting);
            }
            catch (Exception)
            {
                return;
            }

            if (Current != Value)
            {
                Properties.Settings.Default[Setting] = Value;
                Properties.Settings.Default.Save();
                Update_Buttons();
            }
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
    }
}