using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using LTDBot.Modules.Models.ApiAi;
using Newtonsoft.Json;

namespace LTDBot.Modules.Clients
{
    public static class ApiAiClient
    {
        public static void SendEntity(Entity entity)
        {
            string baseAddress = ConfigurationManager.AppSettings.Get("ApiAiBaseAddress");
            string url = $"entities/{entity.Id}";
            var client = new HttpClient { BaseAddress = new Uri(baseAddress) };
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ConfigurationManager.AppSettings.Get("ApiAiDeveloperKey")}");
            var content = new StringContent(JsonConvert.SerializeObject(entity));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            var response = client.PutAsync(url, content).Result;

            if (!response.IsSuccessStatusCode)
                Logger.Logger.GetInstance().Log($"ApiAi update ended with error: {response.Content.ReadAsStringAsync().Result}");
        }
    }
}