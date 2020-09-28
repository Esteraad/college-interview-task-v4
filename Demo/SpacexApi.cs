using college_interview_task_v4;
using Demo.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
    public class SpacexApi
    {
        private readonly string rocketsBaseAddress = "https://api.spacexdata.com/v3/rockets";

        private HttpClient httpClient;

        public SpacexApi(HttpClient httpClient) {
            this.httpClient = httpClient;
            httpClient.BaseAddress = new Uri(rocketsBaseAddress);
        }

        public async Task<SpacexRocket> GetRocketAsync(string rocketId, CancellationToken cancellationToken) {
            var relativeUrl = $"/{rocketId}?filter=id,active,height/meters,diameter/meters,mass/kg,payload_weights,engines/thrust_to_weight,rocket_name,rocket_id";
            var jsonHttpRequestHandler = new JsonHttpRequestHandler<SpacexRocket>(httpClient, new JsonHttpRequestHandler<SpacexRocket>.ResponseParser());

            SpacexRocket spacexRocket = await jsonHttpRequestHandler.Handle(HttpMethod.Get, relativeUrl, cancellationToken);
            return spacexRocket;
        }

        public async Task<Dictionary<int, string>> GetRocketsAsync(CancellationToken cancellationToken) {
            var relativeUrl = $"?filter=id,rocket_id";
            var jsonHttpRequestHandler = new JsonHttpRequestHandler<List<SpacexRocketBase>>(httpClient, new JsonHttpRequestHandler<List<SpacexRocketBase>>.ResponseParser());
            List<SpacexRocketBase> spacexRocketBases = await jsonHttpRequestHandler.Handle(HttpMethod.Get, relativeUrl, cancellationToken);
            return spacexRocketBases.ToDictionary(r => r.id, r => r.rocket_id);
        }
    }
}
