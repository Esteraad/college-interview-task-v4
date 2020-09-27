using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Xunit;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Moq;
using Moq.Protected;
using System.Threading;
using System.Linq;
using college_interview_task_v4.Tests.models;

namespace college_interview_task_v4.Tests
{
    public class JsonHttpRequestHandlerTests
    {
        private readonly string spacexRocketBaseListJson = GetAssemblyFileText(@"TestContent\SpacexRocketBaseList.json");

        private readonly string spacexRocketBaseListInvalidJson = GetAssemblyFileText(@"TestContent\SpacexRocketBaseListInvalid.json");

        private readonly string spacexRocketsBaseAddress = "https://api.spacexdata.com/v3/rockets";

        [Fact]
        public async Task ParseAsync_SpacexRocketBaseListShouldParse() {
            string json = spacexRocketBaseListJson;
            var cts = new CancellationTokenSource();
            HttpContent content = new StringContent(json);
            HttpResponseMessage response = new HttpResponseMessage { Content = content };
            HttpRequestHandler<List<SpacexRocketBase>>.IHttpResponseParser<List<SpacexRocketBase>> parser = 
                new JsonHttpRequestHandler<List<SpacexRocketBase>>.ResponseParser();

            List<SpacexRocketBase> expected = GetSpacexRocketBaseList();

            List<SpacexRocketBase> actual = await parser.ParseAsync(response, cts.Token);

            for (int i = 0; i < actual.Count; i++) {
                Assert.True(actual[i].Equals(expected[i]));
            }
        }
        
        [Fact]
        public async Task ParseAsync_InvalidSpacexRocketBaseListShouldThrowJsonSerializationException() {
            string json = spacexRocketBaseListInvalidJson;
            var cts = new CancellationTokenSource();
            HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(json) };

            HttpRequestHandler<List<SpacexRocketBase>>.IHttpResponseParser<List<SpacexRocketBase>> parser =
                new JsonHttpRequestHandler<List<SpacexRocketBase>>.ResponseParser();

            await Assert.ThrowsAsync<JsonSerializationException>(() => parser.ParseAsync(response, cts.Token));
        }

        [Fact]
        public async Task Handle_SpacexRocketBaseListShouldParse() {
            List<int> SuccessStatusCodes = new List<int>((IEnumerable<int>)Enum.GetValues(typeof(HttpStatusCode)))
                .Where(sc => sc >= 200 && sc <= 299).ToList();

            foreach (int SuccessStatusCode in SuccessStatusCodes) {
                string json = spacexRocketBaseListJson;
                Mock<HttpMessageHandler> handlerMock = GetMockedHandler(json, (HttpStatusCode)SuccessStatusCode);
                HttpClient httpClient = GetClient(spacexRocketsBaseAddress, handlerMock);
                var jsonHttpRequestHandler =
                    new JsonHttpRequestHandler<List<SpacexRocketBase>>(httpClient, new JsonHttpRequestHandler<List<SpacexRocketBase>>.ResponseParser());
                var cts = new CancellationTokenSource();
                var relativeUrl = $"?filter=id,rocket_id";
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri($"{spacexRocketsBaseAddress}{relativeUrl}"));

                List<SpacexRocketBase> expected = GetSpacexRocketBaseList();

                // Testing both handle overloads.
                List<List<SpacexRocketBase>> actuals = new List<List<SpacexRocketBase>>();
                actuals.Add(await jsonHttpRequestHandler.Handle(HttpMethod.Get, relativeUrl, cts.Token));
                actuals.Add(await jsonHttpRequestHandler.Handle(httpRequestMessage, cts.Token));
                    
                foreach (var actual in actuals) {
                    Assert.NotNull(actual);

                    for (int i = 0; i < actual.Count; i++) {
                        Assert.True(actual[i].Equals(expected[i]));
                    }
                }

                var expectedUri = new Uri(spacexRocketsBaseAddress + relativeUrl);

                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Exactly(2),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get
                        && req.RequestUri == expectedUri
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );
            }
        }

        [Fact]
        public async Task Handle_ShouldThrowHttpRequestException() {
            List<int> failStatusCodes = new List<int>((IEnumerable<int>)Enum.GetValues(typeof(HttpStatusCode)))
                .Where(sc => sc < 200 || sc > 299).ToList();

            foreach (int failStatusCode in failStatusCodes) {
                Mock<HttpMessageHandler> handlerMock = GetMockedHandler("", (HttpStatusCode)failStatusCode);
                HttpClient httpClient = GetClient(spacexRocketsBaseAddress, handlerMock);

                var jsonHttpRequestHandler
                    = new JsonHttpRequestHandler<List<SpacexRocketBase>>(httpClient, new JsonHttpRequestHandler<List<SpacexRocketBase>>.ResponseParser());
                var cts = new CancellationTokenSource();
                var relativeUrl = $"?filter=id,rocket_id";
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri($"{spacexRocketsBaseAddress}{relativeUrl}"));

                // Testing both handle overloads
                await Assert.ThrowsAsync<HttpRequestException>(() => jsonHttpRequestHandler.Handle(HttpMethod.Get, relativeUrl, cts.Token));
                await Assert.ThrowsAsync<HttpRequestException>(() => jsonHttpRequestHandler.Handle(httpRequestMessage, cts.Token));
            }
        }

        private static HttpClient GetClient(string baseAddress, Mock<HttpMessageHandler> handlerMock) {
            var httpClient = new HttpClient(handlerMock.Object) {
                BaseAddress = new Uri(baseAddress),
            };
            return httpClient;
        }

        private static Mock<HttpMessageHandler> GetMockedHandler(string json, HttpStatusCode statusCode) {
            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage() {
                    StatusCode = statusCode,
                    Content = new StringContent(json),
                })
                .Verifiable();
            return handlerMock;
        }

        private static List<SpacexRocketBase> GetSpacexRocketBaseList() {
            return new List<SpacexRocketBase>{
                new SpacexRocketBase{ id = 1, rocket_id = "falcon1"},
                new SpacexRocketBase{ id = 2, rocket_id = "falcon9"},
                new SpacexRocketBase{ id = 3, rocket_id = "falconheavy"},
                new SpacexRocketBase{ id = 4, rocket_id = "starship"}
            };
        }

        private static string GetAssemblyFileText(string relativePath) {
            return File.ReadAllText(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), relativePath));
        }
    }
}
