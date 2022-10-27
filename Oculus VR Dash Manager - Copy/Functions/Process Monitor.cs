using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace OVR_Dash_Manager.Functions
{
    public static class Process_Watcher
    {
        public delegate void NewProcess(String pProcessName, Int32 pProcessID);

        public static event NewProcess ProcessStarted;

        public delegate void ClosedProcess(String pProcessName, Int32 pProcessID);

        public static event ClosedProcess ProcessExited;

        private static ManagementEventWatcher Process_StartEvent = null;
        private static ManagementEventWatcher Process_StopEvent = null;

        private static Boolean Running = false;
        private static Boolean _Is_Setup = false;

        private static HashSet<String> IgnoredEXE_Names;

        public static void Setup()
        {
            if (_Is_Setup)
                return;

            try
            {
                _Is_Setup = true;

                if (Process_StartEvent == null)
                {
                    IgnoredEXE_Names = new HashSet<string>();

                    // Stop Truncated ? known bug .. whyyyyyy
                    //Process_StartEvent = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStartTrace");
                    //Process_StopEvent = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStopTrace");

                    Process_StartEvent = new ManagementEventWatcher("SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'");
                    Process_StopEvent = new ManagementEventWatcher("SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'");

                    Process_StartEvent.EventArrived += Process_StartEvent_EventArrived;
                    Process_StopEvent.EventArrived += Process_StopEvent_EventArrived;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error during creation of ManagementEventWatcher for __InstanceCreationEvent / __InstanceDeletionEvent");
            }
        }

        public static void IngoreEXEName(String EXEName)
        {
            IgnoredEXE_Names?.Add(EXEName);
        }

        public static void Remove_IgnoreEXEName(String EXEName)
        {
            IgnoredEXE_Names?.Remove(EXEName);
        }

        public static Boolean Start()
        {
            Boolean pReturn = false;

            Setup();

            if (Process_StartEvent != null)
            {
                if (!Running)
                {
                    try
                    {
                        Process_StartEvent.Start();
                        Process_StopEvent.Start();
                        pReturn = true;

                        Running = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            return pReturn;
        }

        public static Boolean Stop()
        {
            Boolean pReturn = false;
            if (Process_StartEvent != null)
            {
                if (Running)
                {
                    try
                    {
                        Process_StartEvent.Stop();
                        Process_StopEvent.Stop();
                        pReturn = true;

                        Running = false;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            return pReturn;
        }

        public static void Dispose()
        {
            if (Process_StartEvent != null)
            {
                Stop();

                IgnoredEXE_Names?.Clear();
                Process_StartEvent.Dispose();
                Process_StopEvent.Dispose();

                _Is_Setup = false;
            }
        }

        private static void Process_StartEvent_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (ProcessStarted == null) return;

            //string Name = (string)e.NewEvent.Properties["ProcessName"].Value;
            //int ID = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);

            var targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            var Name = targetInstance["Name"]?.ToString();
            var ID = Convert.ToInt32(targetInstance["Handle"]?.ToString());

            if (!IgnoredEXE_Names.Contains(Name))
                ProcessStarted(Name, ID);
        }

        private static void Process_StopEvent_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (ProcessExited == null) return;

            //string Name = (string)e.NewEvent.Properties["ProcessName"].Value;
            //int ID = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);

            var targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            var Name = targetInstance["Name"]?.ToString();
            var ID = Convert.ToInt32(targetInstance["Handle"]?.ToString());

            if (!IgnoredEXE_Names.Contains(Name))
                ProcessExited(Name, ID);
        }
    }
}