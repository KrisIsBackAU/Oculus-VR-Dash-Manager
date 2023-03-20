using System;
using System.Diagnostics;
using System.Management;

namespace OVR_Dash_Manager.Functions
{
    public static class Device_Watcher
    {
        public delegate void NewDevice();
        public static event NewDevice DeviceConnected;

        private static ManagementEventWatcher _connected;

        private static bool _isSetup;
        private static bool _running;

        private static DateTime _lastConnectionTime;

        private static void Setup()
        {
            if (_isSetup) return;

            try
            {
                _isSetup = true;
                if (_connected != null) return;

                var query = new WqlEventQuery
                {
                    EventClassName = "Win32_DeviceChangeEvent"
                };

                _connected = new ManagementEventWatcher(query);
                _connected.EventArrived += Handle_DeviceConnected;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error during creation of ManagementEventWatcher for Win32_DeviceChangeEvent");
            }
        }

        public static void Start()
        {
            Setup();

            if (_connected == null) return;
            if (_running) return;
            try
            {
                _connected.Start();
                _running = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static void Stop()
        {
            if (_connected == null) return;
            if (!_running) return;
            try
            {
                _connected.Stop();
                _running = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void Handle_DeviceConnected(object sender, EventArrivedEventArgs e)
        {
            // If nothing is subscribed to the event
            if (DeviceConnected == null) return;

            // Only take action if it's a connection event
            if (int.Parse(e.NewEvent.GetPropertyValue("EventType").ToString()) != 2) return;
            
            // Limits event spam to once per second
            if (DateTime.Now - _lastConnectionTime  < TimeSpan.FromSeconds(1)) return;
            _lastConnectionTime = DateTime.Now;

            DeviceConnected();
        }
    }
}