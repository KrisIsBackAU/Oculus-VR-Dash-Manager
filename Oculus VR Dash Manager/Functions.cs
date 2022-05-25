using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace OVR_Dash_Manager
{
    public static class Functions
    {
        public static void ShowFileInDirectory(String FullPath)
        {
            Process.Start("explorer.exe", $@"/select,""{FullPath}""");
        }

        public static String GetPageHTML(String pURL, String Method = "GET", CookieContainer Cookies = null, String FormParams = "", String ContentType = "")
        {
            if (pURL.Contains("&amp;"))
                pURL = pURL.Replace("&amp;", "&");

            string result = "";
            if (Uri.IsWellFormedUriString(pURL, UriKind.Absolute))
            {
                HttpWebRequest pWebRequest = (HttpWebRequest)WebRequest.Create(pURL);
                pWebRequest.Method = Method;
                pWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0";
                pWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                if (ContentType != "")
                    pWebRequest.ContentType = ContentType;
                pWebRequest.AllowAutoRedirect = true;
                pWebRequest.MaximumAutomaticRedirections = 3;
                pWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                if (Cookies != null)
                    pWebRequest.CookieContainer = Cookies;

                if (FormParams != "")
                {
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(FormParams);
                    pWebRequest.ContentLength = bytes.Length;
                    using (Stream os = pWebRequest.GetRequestStream())
                        os.Write(bytes, 0, bytes.Length);
                }

                WebResponse pWebResponce = null;
                StreamReader pStreamRead = null;

                try
                {
                    pWebResponce = pWebRequest.GetResponse();
                    pStreamRead = new StreamReader(pWebResponce.GetResponseStream(), Encoding.UTF8);
                    result = pStreamRead.ReadToEnd();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("an error"))
                    {
                        if (Regex.IsMatch(ex.Message, @"\(\d{3}\)"))
                        {
                            result = Regex.Match(ex.Message, @"\(\d{3}\)").Value;
                            result = result.Substring(1, 3);
                        }
                    }
                    else if (ex.Message == "Unable to connect to the remote server")
                    {
                        result = "Offline";
                    }
                    else
                    {
                        result = "404";
                    }
                }

                if (pStreamRead != null)
                {
                    pStreamRead.Close();
                    pStreamRead.Dispose();
                }

                if (pWebResponce != null)
                {
                    pWebResponce.Close();
                    pWebResponce.Dispose();
                }
            }

            if (result.Contains("error"))
            {
                if (!result.StartsWith("<html") && !result.ToLower().StartsWith("<!doctype") && !result.StartsWith("{") && !result.StartsWith("["))
                {
                    result = "500";
                }
            }
            return result;
        }

        public static Boolean Get_File(String pFullURL, String pSaveTo)
        {
            try
            {
                WebClient myWebClient = new WebClient
                {
                    CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore)
                };
                myWebClient.DownloadFile(pFullURL, pSaveTo);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void OpenURL(String URL)
        {
            ProcessStartInfo ps = new ProcessStartInfo(URL)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        public static void DoAction(Window Form, Action DoAction)
        {
            Form.Dispatcher.Invoke(DoAction, DispatcherPriority.Normal);
        }

        public static Boolean IsCurrentProcess_Elevated()
        {
            WindowsIdentity vIdentity = GetWindowsIdentity();
            WindowsPrincipal vPrincipal = GetWindowsPrincipal(vIdentity);

            bool pReturn = vPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            vIdentity.Dispose();
            return pReturn;
        }

        public static WindowsIdentity GetWindowsIdentity()
        {
            return WindowsIdentity.GetCurrent();
        }

        public static WindowsPrincipal GetWindowsPrincipal(WindowsIdentity pIdentity)
        {
            return new WindowsPrincipal(pIdentity);
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public static void MoveCursor(int X, int Y)
        {
            SetCursorPos(X, Y);
        }
    }
    public static class Timer_Functions
    {
        private static Dictionary<String, Timer> gTimers = null;
        private static Boolean BeenSetup = false;

        private static void Setup()
        {
            if (gTimers == null)
            {
                gTimers = new Dictionary<string, Timer>();
                BeenSetup = true;
            }
        }

        public static Boolean SetNewInterval(String pTimerID, TimeSpan pInterval)
        {
            if (!BeenSetup)
                return false;


            Boolean pReturn = false;

            if (gTimers.ContainsKey(pTimerID))
            {
                Timer vTimer = gTimers[pTimerID];
                vTimer.Interval = pInterval.TotalMilliseconds;
                pReturn = true;
            }

            return pReturn;
        }

        public static Boolean CreateTimer(String pTimerID, TimeSpan pInterval, ElapsedEventHandler pTickHandler, Boolean pRepeat = true)
        {
            if (!BeenSetup)
                Setup();

            Boolean pReturn = false;

            if (!gTimers.ContainsKey(pTimerID))
            {
                Timer vTimer = new Timer
                {
                    Interval = pInterval.TotalMilliseconds,
                    AutoReset = pRepeat,
                    Enabled = false
                };

                vTimer.Elapsed += pTickHandler;
                vTimer.Start();

                gTimers.Add(pTimerID, vTimer);

                pReturn = true;
            }

            return pReturn;
        }

        public static Boolean StartTimer(String pTimerID)
        {
            if (!BeenSetup)
                return false;


            Boolean pReturn = false;

            if (gTimers.ContainsKey(pTimerID))
            {
                Timer vTimer = gTimers[pTimerID];
                vTimer.Enabled = true;
                pReturn = true;
            }

            return pReturn;
        }

        public static Boolean StopTimer(String pTimerID)
        {
            if (!BeenSetup)
                return false;


            Boolean pReturn = false;

            if (gTimers.ContainsKey(pTimerID))
            {
                Timer vTimer = gTimers[pTimerID];
                vTimer.Enabled = false;
                pReturn = true;
            }

            return pReturn;
        }

        public static Boolean TimerExists(String pTimerID)
        {
            if (!BeenSetup)
                return false;

            Boolean pReturn = false;

            if (gTimers.ContainsKey(pTimerID))
                pReturn = true;

            return pReturn;
        }

        public static void DisposeTimer(String pTimerID)
        {
            if (gTimers.ContainsKey(pTimerID))
            {
                Timer vTimer = gTimers[pTimerID];
                gTimers.Remove(pTimerID);

                vTimer.Stop();
            }
        }

        public static void DisposeAllTimers()
        {
            foreach (KeyValuePair<String, Timer> oTimer in gTimers)
            {
                oTimer.Value.Stop();
                oTimer.Value.Dispose();
            }

            gTimers.Clear();
        }
    }

}