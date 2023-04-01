using System;
using System.Collections.Generic;

namespace OVR_Dash_Manager.Functions
{
    public static class File_Browser
    {
        public static string Open_Single(string DefaultDirectory = "", string DefaultExtenstion = "", string FileExtenstionFilters = "*.*;", bool MustExist = true)
        {
            string pFile = string.Empty;

            List<string> pFiles = Do_FileBrowser(DefaultDirectory, DefaultDirectory, FileExtenstionFilters, false, MustExist);

            if (pFiles.Count == 1)
                pFile = pFiles[0];

            return pFile;
        }

        public static List<string> Open_Multiple(string DefaultDirectory = "", string DefaultExtenstion = "", string FileExtenstionFilters = "*.*;", bool MustExist = true)
        {
            List<string> pFiles = Do_FileBrowser(DefaultDirectory, DefaultDirectory, FileExtenstionFilters, true, MustExist);

            return pFiles;
        }

        private static List<string> Do_FileBrowser(string DefaultDirectory, string DefaultExtenstion, string FileExtenstionFilters, bool MultipleFiles, bool MustExist)
        {
            List<string> pFiles = new List<string>();

            if (DefaultDirectory == "")
            {
                string ProcessDirectory = Process_Functions.GetCurrentProcessDirectory();
                DefaultDirectory = ProcessDirectory;
            }

            Dictionary<string, string> FileTypes = new Dictionary<string, string>();

            if (!FileExtenstionFilters.Contains("*.*"))
            {
                string[] pSplit = FileExtenstionFilters.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string Name;

                foreach (string oExt in pSplit)
                {
                    if (!FileTypes.ContainsKey(oExt))
                    {
                        Name = GetDescription(oExt.Replace("*", ""));

                        if (Name == "")
                            Name = oExt.Replace("*.", "").ToUpper() + " File";

                        FileTypes.Add(oExt, Name);
                    }
                }
            }
            else
                FileTypes.Add("*.*", "All Files");

            string ExtentsionFilterSeperateAll = "Filtered Files|";
            string ExtentsionFilterSeperate = "";

            foreach (KeyValuePair<string, string> oFilter in FileTypes)
            {
                ExtentsionFilterSeperateAll += oFilter.Key + ";";
                ExtentsionFilterSeperate += "|" + oFilter.Value + "|" + oFilter.Key;
            }

            ExtentsionFilterSeperate = String_Functions.RemoveStringFromStart(ExtentsionFilterSeperate, "|");

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = DefaultExtenstion,
                Filter = ExtentsionFilterSeperateAll + "|" + ExtentsionFilterSeperate,
                InitialDirectory = DefaultDirectory,
                AddExtension = (DefaultExtenstion != "" ? true : false),
                CheckFileExists = MustExist,
                CheckPathExists = true,
                ValidateNames = true,
                Multiselect = MultipleFiles
            };

            bool? result = dlg.ShowDialog();

            if (result == true)
                pFiles.AddRange(dlg.FileNames);

            return pFiles;
        }

        private static string ReadDefaultValue(string regKey)
        {
            using (var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(regKey, false))
            {
                if (key != null)
                {
                    return key.GetValue("") as string;
                }
            }
            return null;
        }

        private static string GetDescription(string ext)
        {
            if (ext.StartsWith(".") && ext.Length > 1) ext = ext.Substring(1);

            var retVal = ReadDefaultValue(ext + "file");
            if (!string.IsNullOrEmpty(retVal)) return ext;

            using (var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("." + ext, false))
            {
                if (key == null) return "";

                using (var subkey = key.OpenSubKey("OpenWithProgids"))
                {
                    if (subkey == null) return "";

                    var names = subkey.GetValueNames();
                    if (names == null || names.Length == 0) return "";

                    foreach (var name in names)
                    {
                        retVal = ReadDefaultValue(name);
                        if (!string.IsNullOrEmpty(retVal)) return retVal;
                    }
                }
            }

            return "";
        }
    }
}