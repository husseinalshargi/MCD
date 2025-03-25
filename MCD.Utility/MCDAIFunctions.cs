using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.Utility
{
    public class MCDAIFunctions
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://0.0.0.0:8001/") // Ensure this matches your API
        };
        public static async Task<string> PostAsync<T>(string endpoint, T data)
        {
            string jsonContent = JsonConvert.SerializeObject(data);
            HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

    }
}
