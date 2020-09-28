using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace college_interview_task_v4
{
    public abstract class HttpRequestHandler<TResponse>
    {
        private HttpClient httpClient;

        private IHttpResponseParser<TResponse> parser;

        protected HttpRequestHandler(HttpClient httpClient, IHttpResponseParser<TResponse> parser) {
            this.parser = parser;
            this.httpClient = httpClient;
        }

        public async Task<TResponse> Handle(HttpMethod httpMethod, string relativeUrl, CancellationToken cancellationToken,
            IDictionary<string, string>? headers = null, HttpContent? payload = null) {
            
            string uri = GetUri(relativeUrl);
            var httpRequestMessage = GetHttpRequestMessage(httpMethod, headers, payload, uri);
            var response = await GetResponse(httpRequestMessage, cancellationToken);
            return await ParseResponse(response, cancellationToken);
        }

        public async Task<TResponse> Handle(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken) {
            HttpResponseMessage response = await GetResponse(httpRequestMessage, cancellationToken);
            return await ParseResponse(response, cancellationToken);
        }

        private string GetUri(string relativeUrl) {
            return $"{httpClient.BaseAddress}{relativeUrl}";
        }

        private static HttpRequestMessage GetHttpRequestMessage(HttpMethod httpMethod, IDictionary<string, string>? headers, HttpContent? payload, string uri) {
            var httpRequestMessage = new HttpRequestMessage(httpMethod, new Uri(uri));
            if (payload != null) httpRequestMessage.Content = payload;
            if (headers != null) {
                foreach (KeyValuePair<string, string> header in headers) {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);
                }
            }
            return httpRequestMessage;
        }

        private async Task<HttpResponseMessage> GetResponse(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken) {
            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
            if (!response.IsSuccessStatusCode) {
                throw new HttpRequestException($"{(int)response.StatusCode} {response.StatusCode}");
            }

            return response;
        }

        private async Task<TResponse> ParseResponse(HttpResponseMessage response, CancellationToken cancellationToken) {
            try {
                return await parser.ParseAsync(response, cancellationToken);
            }
            catch (Exception ex) {
                throw new FormatException("Error occured while parsing http response. Details: " + Environment.NewLine + ex.Message, ex);
            }
        }

        protected interface IHttpResponseParser<TResult>
        {
            public Task<TResult> ParseAsync(HttpResponseMessage response, CancellationToken cancellationToken);
        }

    }
}