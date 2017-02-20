using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;

namespace LTDBot.Modules.Clients
{
    public abstract class Client
    {
        protected const string BaseAddress = "https://api.londontheatredirect.com/rest/v2/";

        public T Get<T>(string url, string query = "")
        {
            var client = new HttpClient {BaseAddress = new Uri(url)};
            client.DefaultRequestHeaders.Add("API-KEY", ConfigurationManager.AppSettings.Get("LtdApiKey"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.GetAsync(query).Result;

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);

            throw new HttpException($"Http Get failed. Status code: {response.StatusCode}, Message: {response.Content.ReadAsStringAsync().Result} ");
        }
    }
}