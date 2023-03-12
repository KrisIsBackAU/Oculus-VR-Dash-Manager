using System;
using System.Windows.Controls;

namespace OVR_Dash_Manager
{
    internal class Hover_Button
    {
        public bool Enabled { get; set; } = false;
        public bool Hovering { get; private set; } = false;
        public DateTime Hover_Started { get; private set; } = DateTime.Now;
        public Int32 Hovered_Seconds_To_Activate { get; set; } = 5;
        public Action Hover_Complete_Action { get; set; }
        public ProgressBar Bar { get; set; }
        public bool Check_SteamVR { get; set; }

        public void Reset()
        {
            Hovering = false;
            Hover_Started = DateTime.Now;
            Bar.Value = 0;
        }

        public void SetHovering()
        {
            Hovering = true;
            Hover_Started = DateTime.Now;
            Bar.Value = 10;
        }

        public void StopHovering()
        {
            Reset();
        }
    }
}