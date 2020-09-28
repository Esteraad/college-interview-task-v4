using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace college_interview_task_v4
{
    /// <summary>
    /// Handler for requests that return json content
    /// </summary>
    /// <typeparam name="TContent">object of type TContent parsed from content</typeparam>
    public class JsonHttpRequestHandler<TContent> : HttpRequestHandler<TContent>
    {
        public class ResponseParser : IHttpResponseParser<TContent>
        {
            async Task<TContent> IHttpResponseParser<TContent>.ParseAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
                string content = await response.Content.ReadAsStringAsync();
                cancellationToken.ThrowIfCancellationRequested();

                TContent parsedContent = JsonConvert.DeserializeObject<TContent>(content);
                return parsedContent;
            }
        }

        public JsonHttpRequestHandler(HttpClient httpClientProxy) : base(httpClientProxy, new ResponseParser()) { }
    }
}
