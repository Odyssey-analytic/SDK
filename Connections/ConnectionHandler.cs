using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using odysseyAnalytics.Exceptions;
using odysseyAnalytics.Logging;

namespace odysseyAnalytics.Connections
{
    public class ConnectionHandler : IDisposable
    {
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private HttpClient _httpClient;
        private static DefaultLogger _logger = new DefaultLogger(); 

        public ConnectionHandler(string apiKey, string baseUrl)
        {
            _apiKey = apiKey;
            _baseUrl = baseUrl;
            InitializeConnection();
        }

        private void InitializeConnection()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"{_apiKey}");
        }

        public async Task<HttpResponseMessage> SendRequestAsync(string endpoint, HttpMethod method,
            HttpContent content = null)
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

        public static async Task<bool> IsConnectedToInternet()
        {
            try
            {
                using (var testClient = new HttpClient())
                {
                    testClient.Timeout = TimeSpan.FromSeconds(5);
                    var response = await testClient.GetAsync("https://www.google.com/generate_204");
                    _logger.Log("Internet connection is up and running.");
                    return response.StatusCode == System.Net.HttpStatusCode.NoContent;
                }
            }
            catch
            {
                _logger.Error("Internet connection is not established.");
                throw new NoInternetConnectionException("Cannot reach the internet.");
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