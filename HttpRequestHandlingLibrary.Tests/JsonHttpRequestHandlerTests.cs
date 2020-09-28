using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Xunit;
using System.Threading.Tasks;
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

        private string spacexRocketsRelativeUrl = $"?filter=id,rocket_id";

        private CancellationTokenSource cts = new CancellationTokenSource();

        private List<int> successStatusCodes = new List<int>((IEnumerable<int>)Enum.GetValues(typeof(HttpStatusCode)))
                .Where(sc => sc >= 200 && sc <= 299).ToList();

        private List<int> failStatusCodes = new List<int>((IEnumerable<int>)Enum.GetValues(typeof(HttpStatusCode)))
                .Where(sc => sc < 200 || sc > 299).ToList();


        [Fact]
        public async Task Handle_SpacexRocketBaseListShouldParse() {
            foreach (int SuccessStatusCode in successStatusCodes) {
                Mock<HttpMessageHandler> handlerMock = GetMockedHandler(spacexRocketBaseListJson, (HttpStatusCode)SuccessStatusCode);
                HttpClient httpClient = GetClient(spacexRocketsBaseAddress, handlerMock);
                var jsonHttpRequestHandler = new JsonHttpRequestHandler<List<SpacexRocketBaseTest>>(httpClient);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                    new Uri($"{spacexRocketsBaseAddress}{spacexRocketsRelativeUrl}"));

                List<SpacexRocketBaseTest> expected = GetSpacexRocketBaseList();
                var expectedUri = new Uri(spacexRocketsBaseAddress + spacexRocketsRelativeUrl);

                // Testing both handle overloads.
                var actuals = new List<List<SpacexRocketBaseTest>>();
                actuals.Add(await jsonHttpRequestHandler.Handle(HttpMethod.Get, spacexRocketsRelativeUrl, cts.Token));
                actuals.Add(await jsonHttpRequestHandler.Handle(httpRequestMessage, cts.Token));
                    
                foreach (var actual in actuals) {
                    Assert.NotNull(actual);
                    Assert.True(actual.Count == expected.Count);

                    for (int i = 0; i < actual.Count; i++) {
                        Assert.True(actual[i].Equals(expected[i]));
                    }
                }

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
            foreach (int failStatusCode in failStatusCodes) {
                Mock<HttpMessageHandler> handlerMock = GetMockedHandler("", (HttpStatusCode)failStatusCode);
                HttpClient httpClient = GetClient(spacexRocketsBaseAddress, handlerMock);

                var jsonHttpRequestHandler = new JsonHttpRequestHandler<List<SpacexRocketBaseTest>>(httpClient);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                    new Uri($"{spacexRocketsBaseAddress}{spacexRocketsRelativeUrl}"));

                // Testing both handle overloads
                await Assert.ThrowsAsync<HttpRequestException>(
                    () => jsonHttpRequestHandler.Handle(HttpMethod.Get, spacexRocketsRelativeUrl, cts.Token));
                await Assert.ThrowsAsync<HttpRequestException>(
                    () => jsonHttpRequestHandler.Handle(httpRequestMessage, cts.Token));
            }
        }

        [Fact]
        public async Task Handle_ShouldThrowFormatException() {
            foreach (int SuccessStatusCode in successStatusCodes) {
                Mock<HttpMessageHandler> handlerMock = GetMockedHandler(spacexRocketBaseListInvalidJson, (HttpStatusCode)SuccessStatusCode);
                HttpClient httpClient = GetClient(spacexRocketsBaseAddress, handlerMock);

                var jsonHttpRequestHandler = new JsonHttpRequestHandler<List<SpacexRocketBaseTest>>(httpClient);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                    new Uri($"{spacexRocketsBaseAddress}{spacexRocketsRelativeUrl}"));

                // Testing both handle overloads
                await Assert.ThrowsAsync<FormatException>(
                    () => jsonHttpRequestHandler.Handle(HttpMethod.Get, spacexRocketsRelativeUrl, cts.Token));
                await Assert.ThrowsAsync<FormatException>(
                    () => jsonHttpRequestHandler.Handle(httpRequestMessage, cts.Token));
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

        private static List<SpacexRocketBaseTest> GetSpacexRocketBaseList() {
            return new List<SpacexRocketBaseTest>{
                new SpacexRocketBaseTest{ id = 1, rocket_id = "falcon1"},
                new SpacexRocketBaseTest{ id = 2, rocket_id = "falcon9"},
                new SpacexRocketBaseTest{ id = 3, rocket_id = "falconheavy"},
                new SpacexRocketBaseTest{ id = 4, rocket_id = "starship"}
            };
        }

        private static string GetAssemblyFileText(string relativePath) {
            return File.ReadAllText(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), relativePath));
        }
    }
}
