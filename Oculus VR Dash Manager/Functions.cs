using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace OVR_Dash_Manager
{
    public static class Functions
    {
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
    }

    public static class Dispatcher_Timer_Functions
    {
        private static Dictionary<String, DispatcherTimer> gTimers = null;
        private static Dictionary<String, Boolean> gAutoRepeat = null;
        private static Boolean BeenSetup = false;

        private static void Setup()
        {
            if (gTimers == null)
            {
                gTimers = new Dictionary<string, DispatcherTimer>();
                gAutoRepeat = new Dictionary<string, bool>();
                BeenSetup = true;
            }
        }

        public static Boolean CreateTimer(String pTimerID, TimeSpan pInterval, EventHandler pTickHandler, Boolean pRepeat = true)
        {
            if (!BeenSetup)
                Setup();

            Boolean pReturn = false;

            if (!gTimers.ContainsKey(pTimerID))
            {
                DispatcherTimer vTimer = new DispatcherTimer
                {
                    Interval = pInterval,
                    IsEnabled = true,
                    Tag = pTimerID
                };

                vTimer.Tick += ElpasedAutoRetrigger;
                vTimer.Tick += pTickHandler;

                vTimer.Stop();

                gTimers.Add(pTimerID, vTimer);
                gAutoRepeat.Add(pTimerID, pReturn);

                pReturn = true;
            }

            return pReturn;
        }

        private static void ElpasedAutoRetrigger(object sender, EventArgs args)
        {
            DispatcherTimer pTimer = (DispatcherTimer)sender;
            Boolean Repeat = false;
            gAutoRepeat.TryGetValue(pTimer.Tag.ToString(), out Repeat);
            if (Repeat)
            {
                pTimer.Stop();
                pTimer.Start();
            }
        }

        public static Boolean StartTimer(String pTimerID)
        {
            if (!BeenSetup)
                return false;

            Boolean pReturn = false;

            if (gTimers.ContainsKey(pTimerID))
            {
                DispatcherTimer vTimer = gTimers[pTimerID];
                vTimer.Start();
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
                DispatcherTimer vTimer = gTimers[pTimerID];
                vTimer.Stop();
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
            if (!BeenSetup)
                return;

            if (gTimers.ContainsKey(pTimerID))
            {
                DispatcherTimer vTimer = gTimers[pTimerID];
                gTimers.Remove(pTimerID);
                gAutoRepeat.Remove(pTimerID);
                vTimer.Stop();
            }
        }

        public static void DisposeAllTimers()
        {
            if (!BeenSetup)
                return;

            foreach (KeyValuePair<String, DispatcherTimer> oTimer in gTimers)
            {
                oTimer.Value.Stop();
            }

            gTimers.Clear();
            gAutoRepeat.Clear();
        }
    }
}