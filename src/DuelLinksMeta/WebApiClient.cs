using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DuelLinksMeta
{
    internal class WebApiClient : IWebApiClient
    {
        private readonly string _baseAddress;

        public WebApiClient(string baseAddress)
        {
            _baseAddress = baseAddress;
        }

        public TResponse Get<TResponse>(string path)
        {
            return Get<TResponse>(path, null);
        }
        public dynamic Get(string path)
        {
            return Get(path, null);
        }
        public HtmlDocument GetHtmlDocument(string path)
        {
            return GetHtmlDocument(path, null);
        }

        public TResponse Get<TResponse>(string path, IDictionary<string, object> urlParameters)
        {
            var pathWithQuery = BuildPathWithQuery(path, urlParameters);
            using (var requestMessage = CreateRequestMessage(HttpMethod.Get, pathWithQuery, null))
            using (var responseMessage = SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
                return ParseResponseMessage<TResponse>(responseMessage);
            }
        }
        public dynamic Get(string path, IDictionary<string, object> urlParameters)
        {
            var pathWithQuery = BuildPathWithQuery(path, urlParameters);
            using (var requestMessage = CreateRequestMessage(HttpMethod.Get, pathWithQuery, null))
            using (var responseMessage = SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
                return ParseResponseMessage(responseMessage);
            }
        }

        public HtmlDocument GetHtmlDocument(string path, IDictionary<string, object> urlParameters)
        {
            var pathWithQuery = BuildPathWithQuery(path, urlParameters);
            using (var requestMessage = CreateRequestMessage(HttpMethod.Get, pathWithQuery, null))
            using (var responseMessage = SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(responseMessage.Content.ReadAsStringAsync().Result);
                return htmlDocument;
            }
        }

        public void Put(string path, object content)
        {
            Put(path, null, content);
        }

        public void Put(string path, IDictionary<string, object> urlParameters, object content)
        {
            var pathWithQuery = BuildPathWithQuery(path, urlParameters);
            using (var requestMessage = CreateRequestMessage(HttpMethod.Put, pathWithQuery, content))
            using (var responseMessage = SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
            }
        }

        public TResponse Post<TResponse>(string path, object content)
        {
            using (var requestMessage = CreateRequestMessage(HttpMethod.Post, path, content))
            using (var responseMessage = SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
                return ParseResponseMessage<TResponse>(responseMessage);
            }
        }

        public void Post(string path, object content)
        {
            Post(path, null, content);
        }

        public void Post(string path, IDictionary<string, object> urlParameters, object content)
        {
            var pathWithQuery = BuildPathWithQuery(path, urlParameters);
            using (var requestMessage = CreateRequestMessage(HttpMethod.Post, pathWithQuery, content))
            using (var responseMessage = SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
            }
        }

        public void Delete(string path, IDictionary<string, object> urlParameters)
        {
            var pathWithQuery = BuildPathWithQuery(path, urlParameters);
            using (var requestMessage = CreateRequestMessage(HttpMethod.Delete, pathWithQuery, null))
            using (var responseMessage = SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
            }
        }

        public void Delete(string path, object content)
        {
            using (var requestMessage = CreateRequestMessage(HttpMethod.Delete, path, content))
            using (var responseMessage = SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
            }
        }

        protected virtual HttpClient CreateHttpClient()
        {
            return new HttpClient(new HttpClientHandler()
            {
                // Redirect bedeutet wahrscheinlich Weiterleitung auf Nutzeranmeldeseite
                AllowAutoRedirect = false,
                UseCookies = false
            });
        }

        private static string BuildPathWithQuery(string path, IDictionary<string, object> urlParameters)
        {
            if (urlParameters == null)
            {
                return path;
            }

            var query = string.Join("&", urlParameters.Select(p => p.Value != null ? $"{p.Key}={p.Value.ToString()}" : p.Key));
            if (!string.IsNullOrEmpty(query))
            {
                return path + "?" + query;
            }

            return path;
        }

        private static HttpRequestMessage CreateRequestMessage(HttpMethod method, string path, object content)
        {
            var message = new HttpRequestMessage(method, path);

            if (IsNonEmptyContent(content))
            {
                AttachContent(content, message);
            }

            return message;
        }

        private static void AttachContent(object content, HttpRequestMessage message)
        {
            var json = JsonConvert.SerializeObject(content);
            message.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static bool IsNonEmptyContent(object content)
        {
            return content != null;
        }

        private static void CheckResponseMessage(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Webservice meldet Status {response.StatusCode}");
            }
        }

        private static T ParseResponseMessage<T>(HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(content);
        }

        private static dynamic ParseResponseMessage(HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(content);
        }

        private HttpResponseMessage SendRequestMessage(HttpRequestMessage message)
        {
            using (var client = CreateHttpClient())
            {
                client.BaseAddress = new Uri(_baseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return client.SendAsync(message).Result;
            }
        }
    }
}
