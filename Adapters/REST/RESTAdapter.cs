using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using odysseyAnalytics.Core.Application.Gateway;
using System.Linq;
using odysseyAnalytics.Core.Ports;

namespace odysseyAnalytics.Adapters.REST
{
    public class RESTAdapter : IGatewayPort
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private bool disposed;

        public RESTAdapter(string baseUrl)
        {
            this.baseUrl = baseUrl.TrimEnd('/');
            httpClient = new HttpClient();
        }

        public async Task<bool> SendAsync(GatewayPayload payload)
        {
            try
            {
                string url = $"{baseUrl}{payload.Endpoint}";
                var json = JsonConvert.SerializeObject(payload.Data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                ApplyAuthHeader(payload.AccessToken);

                var response = await httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<string, object>> FetchAsync(GatewayPayload payload)
        {
            try
            {
                string query = string.Join("&",
                    payload.Data.Select(kv =>
                        $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value.ToString())}"));
                string url = $"{baseUrl}{payload.Endpoint}?{query}";

                ApplyAuthHeader(payload.AccessToken);

                var response = await httpClient.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);
                }
            }
            catch
            {
                // Optionally log
            }

            return new Dictionary<string, object>(); // Return empty on failure
        }

        private void ApplyAuthHeader(string accessToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                httpClient?.Dispose();
                disposed = true;
            }
        }

        ~RESTAdapter()
        {
            Dispose(false);
        }
    }
}