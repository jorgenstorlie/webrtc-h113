using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebRTC.Abstraction;

namespace WebRTC.AppRTC
{
    public class ARDTURNClient
    {
        private const string TURNRefererURLString = @"https://appr.tc";

        private readonly HttpClient _httpClient = new HttpClient();

        private readonly Uri _url;

        public ARDTURNClient(string url)
        {
            _url = new Uri(url);
            
            _httpClient.DefaultRequestHeaders.Referrer = new Uri(TURNRefererURLString);
        }

        public async Task<IceServer[]> RequestServersAsync()
        {
            var response = await _httpClient.PostAsync(_url,new StringContent("")).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var turnResponse = JsonConvert.DeserializeObject<ARDTurnResponse>(json);
            
            var array = turnResponse.IceServers
                .Select(iceServer => new IceServer(iceServer.Urls, iceServer.Username, iceServer.Credential)).ToArray();

            return array;
        }
    }
}