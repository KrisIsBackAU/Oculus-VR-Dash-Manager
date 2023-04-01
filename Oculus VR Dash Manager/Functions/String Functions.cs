using System;

namespace OVR_Dash_Manager.Functions
{
    public static class String_Functions
    {
        public static string RemoveStringFromStart(string Text, string Remove)
        {
            if (Text.StartsWith(Remove))
                Text = Text.Substring(Remove.Length, Text.Length - Remove.Length);

            return Text;
        }

        public static bool IsValidURL(string pURL)
        {
            bool result = Uri.TryCreate(pURL, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!result)
            {
                if (!pURL.StartsWith("http"))
                    result = Uri.TryCreate("http://" + pURL, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            }

            return result;
        }

        public static string GetFullURL(string pURL)
        {
            if (IsValidURL(pURL))
            {
                if (!pURL.StartsWith("http"))
                    pURL = "http://" + pURL;

                Uri pBuild = new Uri(pURL);
                return pBuild.AbsoluteUri;
            }
            else
                return pURL;
        }
    }
}