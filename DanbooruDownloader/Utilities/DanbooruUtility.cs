using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DanbooruDownloader.Utilities
{
    public static class DanbooruUtility
    {

        public static Task<JObject[]> GetPosts(long startId, string username, string apikey)
        {
            string query = $"id:>={startId} order:id_asc";
            string urlEncodedQuery = WebUtility.UrlEncode(query);

            string url = $"https://danbooru.donmai.us/posts.json?tags={urlEncodedQuery}&page=1&limit=1000&login={WebUtility.UrlEncode(username)}&api_key={WebUtility.UrlEncode(apikey)}";
            return GetPosts(url);
        }

        //Get posts with a score greater than or equal to arg "score". Result of search will order by id descending 
        public static Task<JObject[]> GetPosts(long page, long score, string order, string username, string apikey)
        {
            string query = $"score:>={score} order:{order}";
            string urlEncodedQuery = WebUtility.UrlEncode(query);
            string url = $"https://danbooru.donmai.us/posts.json?tags={urlEncodedQuery}&page={page}&limit=1000&login={WebUtility.UrlEncode(username)}&api_key={WebUtility.UrlEncode(apikey)}";
            return GetPosts(url);
        }

        public static async Task<JObject[]> GetPosts(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                //Add User-Agent to prevent receiving a 403 Forbidden response
                client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.43.0");

                string jsonString = await client.GetStringAsync(url);

                JArray jsonArray = JArray.Parse(jsonString);

                return jsonArray.Cast<JObject>().ToArray();
            }
        }

    }
}
