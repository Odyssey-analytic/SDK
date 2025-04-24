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
        IGatewayPortResponse gatewayPortResponse;
        public RESTAdapter(string baseUrl)
        {
            this.baseUrl = baseUrl;
            gatewayPortResponse = new IGatewayPortResponse();
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

        public async Task<IGatewayPortResponse> FetchAsync(GatewayPayload payload)
        {
            try
            {
                string url= $"{baseUrl}{payload.Endpoint}";
                Console.WriteLine(url);
                
                if (payload.Data != null)
                {
                    string query = string.Join("&",
                        payload.Data.Select(kv =>
                            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value.ToString())}"));
                    url = $"{baseUrl}{payload.Endpoint}?{query}";
                }
                ApplyAuthHeader(payload.AccessToken);
                Console.WriteLine("Applied");
                var response = await httpClient.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
                Console.WriteLine(response.StatusCode);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("1111111");
                    gatewayPortResponse.Data = responseString;
                    gatewayPortResponse.StatusCode = "OK";
                    return gatewayPortResponse;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null; // Return empty on failure
        }

        private void ApplyAuthHeader(string accessToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"{accessToken}");
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