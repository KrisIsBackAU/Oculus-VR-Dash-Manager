using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OVR_Dash_Manager
{
    public class Github
    {
        public long GetLatestSize(String Repo, String Project, String AssetName)
        {
            long Size = 0;

            String JSON = Functions.GetPageHTML($"https://api.github.com/repos/{Repo}/{Project}/releases/latest");
            if (JSON.Contains("browser_download_url"))
            {
                GitResponse Git = JsonConvert.DeserializeObject<GitResponse>(JSON);
                if (Git.assets?.Count > 0)
                {
                    foreach (Asset item in Git.assets)
                    {
                        if (item.name == AssetName)
                        {
                            Size = item.size;
                            break;
                        }
                    }
                }
            }

            return Size;
        }

        public long Download(String Repo, String Project, String AssetName, String FilePath)
        {
            long Size = 0;

            String JSON = Functions.GetPageHTML($"https://api.github.com/repos/{Repo}/{Project}/releases/latest");
            if (JSON.Contains("browser_download_url"))
            {
                GitResponse Git = JsonConvert.DeserializeObject<GitResponse>(JSON);
                if (Git.assets?.Count > 0)
                {
                    foreach (Asset item in Git.assets)
                    {
                        if (item.name == AssetName)
                        {
                            Functions.Get_File(item.browser_download_url, FilePath);
                        }
                    }
                }
            }

            return Size;
        }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    internal class Asset
    {
        public string url { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string name { get; set; }
        public object label { get; set; }
        public Uploader uploader { get; set; }
        public string content_type { get; set; }
        public string state { get; set; }
        public long size { get; set; }
        public int download_count { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string browser_download_url { get; set; }
    }

    internal class Author
    {
        public string login { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
    }

    internal class GitResponse
    {
        public string url { get; set; }
        public string assets_url { get; set; }
        public string upload_url { get; set; }
        public string html_url { get; set; }
        public int id { get; set; }
        public Author author { get; set; }
        public string node_id { get; set; }
        public string tag_name { get; set; }
        public string target_commitish { get; set; }
        public string name { get; set; }
        public bool draft { get; set; }
        public bool prerelease { get; set; }
        public DateTime created_at { get; set; }
        public DateTime published_at { get; set; }
        public List<Asset> assets { get; set; }
        public string tarball_url { get; set; }
        public string zipball_url { get; set; }
        public string body { get; set; }
        public int mentions_count { get; set; }
    }

    public class Uploader
    {
        public string login { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
    }
}