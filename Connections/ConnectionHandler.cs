using System;
using System.Net.Http;
using System.Threading.Tasks;
namespace odysseyAnalytics.Connections
{
    public class ConnectionHandler : IDisposable
    {
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private HttpClient _httpClient;
        
        
        public ConnectionHandler(string apiKey, string baseUrl)
        {
            _apiKey = apiKey;
            _baseUrl = baseUrl;
            InitializeConnection();

        }
        private void InitializeConnection()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }
        public async Task<HttpResponseMessage> SendRequestAsync(string endpoint, HttpMethod method, HttpContent content = null)
        {
            var request = new HttpRequestMessage(method, endpoint) { Content = content };
            try
            {
                return await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending request: {ex.Message}");
                return null;
            }
        }
        private void CloseConnection()
        {
            _httpClient?.Dispose();
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}