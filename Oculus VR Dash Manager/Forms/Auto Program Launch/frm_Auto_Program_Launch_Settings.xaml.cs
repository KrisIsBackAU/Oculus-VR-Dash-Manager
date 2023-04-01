using System;
using System.Linq;
using System.Windows;

namespace OVR_Dash_Manager.Forms.Auto_Program_Launch
{
    /// <summary>
    /// Interaction logic for frm_Auto_Program_Launch_Settings.xaml
    /// </summary>
    public partial class frm_Auto_Program_Launch_Settings : Window
    {
        private Boolean Programs_Removed = false;

        public frm_Auto_Program_Launch_Settings()
        {
            InitializeComponent();
        }

        private void btn_Add_Program_Click(object sender, RoutedEventArgs e)
        {
            String FilePath = Functions.File_Browser.Open_Single();
            if (!String.IsNullOrEmpty(FilePath))
            {
                Software.Auto_Launch_Programs.Add_New_Program(FilePath);
                lv_Programs.Items.Refresh();
            }
        }

        private void btn_Remove_Program_Click(object sender, RoutedEventArgs e)
        {
            if (lv_Programs.SelectedItem is Software.Auto_Program Program)
            {
                Programs_Removed = true;
                Software.Auto_Launch_Programs.Remove_Program(Program);
                lv_Programs.Items.Refresh();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lv_Programs.ItemsSource = Software.Auto_Launch_Programs.Programs;
            lv_Programs.Items.Refresh();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool Changed = Programs_Removed;

            if (Changed == false & Software.Auto_Launch_Programs.Programs != null)
                Changed = Software.Auto_Launch_Programs.Programs.Count(a => a.Changed) > 0;

            if (Changed)
            {
                if (MessageBox.Show(this, "Programs Have Been Changed - Are you Sure you want to save changes ?", "Confirm Saving Changes", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    Software.Auto_Launch_Programs.Save_Program_List(); // Save it
                else
                    Software.Auto_Launch_Programs.Generate_List(); // Regenerate and ignore changes
            }
        }

        private void btn_Open_Program_Folder_Click(object sender, RoutedEventArgs e)
        {
            if (lv_Programs.SelectedItem is Software.Auto_Program Program)
                Functions.Process_Functions.StartProcess("explorer.exe", Program.Folder_Path);
        }
    }
}