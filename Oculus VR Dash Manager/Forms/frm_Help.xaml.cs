using System.Windows;

namespace OVR_Dash_Manager.Forms
{
    /// <summary>
    /// Interaction logic for frm_Help.xaml
    /// </summary>
    public partial class frm_Help : Window
    {
        public frm_Help()
        {
            InitializeComponent();
        }

        private void btn_GitHub_Click(object sender, RoutedEventArgs e)
        {
            Functions.OpenURL("https://github.com/KrisIsBackAU/Oculus-VR-Dash-Manager");
        }

        private void btn_Discord_Click(object sender, RoutedEventArgs e)
        {
            Functions.OpenURL("https://discord.gg/ANeSUXRwgC");
        }
    }
}