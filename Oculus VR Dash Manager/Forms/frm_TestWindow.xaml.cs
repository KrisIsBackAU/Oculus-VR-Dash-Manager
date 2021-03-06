using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;

namespace OVR_Dash_Manager.Forms
{
    /// <summary>
    /// Interaction logic for frm_TestWindow.xaml
    /// </summary>
    public partial class frm_TestWindow : Window
    {
        public frm_TestWindow()
        {
            InitializeComponent();
        }

        private void AddToReadOut(String Text)
        {
            Functions.DoAction(this, new Action(delegate () { txtbx_ReadOut.AppendText(Text + "\r\n"); txtbx_ReadOut.ScrollToEnd(); }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Timer_Functions.CreateTimer("Test_Function", TimeSpan.FromSeconds(1), Test_Function);
            Timer_Functions.StartTimer("Test_Function");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Timer_Functions.StopTimer("Test_Function");
            Timer_Functions.DisposeTimer("Test_Function");
        }

        private void Test_Function(object sender, ElapsedEventArgs args)
        {

        }
    }
}