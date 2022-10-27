using Microsoft.Win32;
using System;

namespace OVR_Dash_Manager.Functions
{
    public static class Registry_Functions
    {
        public static RegistryKey GetRegistryKey(RegistryKey_Type Type, String KeyLocation)
        {
            RegistryKey pReturn = null;

            switch (Type)
            {
                case RegistryKey_Type.ClassRoot:
                    pReturn = Registry.ClassesRoot.OpenSubKey(KeyLocation, true);
                    break;

                case RegistryKey_Type.CurrentUser:
                    pReturn = Registry.CurrentUser.OpenSubKey(KeyLocation, true);
                    break;

                case RegistryKey_Type.LocalMachine:
                    pReturn = Registry.LocalMachine.OpenSubKey(KeyLocation, true);
                    break;

                case RegistryKey_Type.Users:
                    pReturn = Registry.Users.OpenSubKey(KeyLocation, true);
                    break;

                case RegistryKey_Type.CurrentConfig:
                    pReturn = Registry.CurrentConfig.OpenSubKey(KeyLocation, true);
                    break;

                default:
                    break;
            }

            return pReturn;
        }

        public static Boolean SetKeyValue(RegistryKey Key, String KeyName, String Value)
        {
            Boolean pReturn = false;

            if (Key != null)
            {
                Boolean pSetValue = false;

                try
                {
                    string Old_Info = Key.GetValue(KeyName).ToString();
                    if (Old_Info != Value)
                        pSetValue = true;
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Object reference not set to an instance of an object.")
                        pSetValue = true;
                }

                if (pSetValue)
                {
                    Key.SetValue(KeyName, Value);
                    pReturn = true;
                }
            }

            return pReturn;
        }

        public static String GetKeyValue_String(RegistryKey Key, String KeyName)
        {
            String pReturn = null;

            if (Key != null)
            {
                try
                {
                    pReturn = Key.GetValue(KeyName).ToString();
                }
                catch (NullReferenceException)
                {
                    pReturn = null;
                }
                catch (Exception)
                {
                }
            }

            return pReturn;
        }

        public static String GetKeyValue_String(RegistryKey_Type Type, String KeyLocation, String KeyName)
        {
            String pReturn = null;

            RegistryKey Key = Functions.Registry_Functions.GetRegistryKey(RegistryKey_Type.LocalMachine, KeyLocation);

            if (Key != null)
            {
                try
                {
                    pReturn = Key.GetValue(KeyName).ToString();
                }
                catch (NullReferenceException)
                {
                    pReturn = null;
                }
                catch (Exception)
                {
                }

                CloseKey(Key);
            }

            return pReturn;
        }

        public static void CloseKey(RegistryKey Key)
        {
            if (Key != null)
            {
                Key.Close();
                Key.Dispose();
            }
        }
    }
}