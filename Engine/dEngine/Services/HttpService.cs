// HttpService.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Utility;
using dEngine.Utility.Extensions;
using Neo.IronLua;
using Newtonsoft.Json;


namespace dEngine.Services
{
    /// <summary>
    /// A service for sending and receiving HTTP data.
    /// </summary>
    [TypeId(12), ExplorerOrder(-1)]
    public partial class HttpService : Service
    {
        /// <inheritdoc/>
        public HttpService()
        {
            Service = this;
        }

        internal static T JsonDecode<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Generates a new GUID.
        /// </summary>
        /// <param name="wrapInCurlyBraces">If true, returned GUID string is wrapped in curly braces.</param>
        public string GenerateGuid(bool wrapInCurlyBraces = true)
        {
            return Guid.NewGuid().ToString(wrapInCurlyBraces ? "B" : "D");
        }

        /// <summary>
        /// Encodes an URL string.
        /// </summary>
        public string UrlEncode(string input)
        {
            return HttpUtility.UrlEncode(input);
        }

        /// <summary>
        /// Serializes object into JSON.
        /// </summary>
        public string JsonEncode(object @object)
        {
            return JsonConvert.SerializeObject(@object);
        }

        /// <summary>
        /// Deserializes JSON into object.
        /// </summary>
        public LuaTable JsonDecode(string json)
        {
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
            return dictionary.ToLuaTable();
        }

        /// <summary>
        /// Sends an HTTP GET request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="noCache"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        [YieldFunction]
        public string GetAsync(string url, bool noCache = false, Dictionary<string, object> headers = null)
        {
            string result = null;
            var thread = ScriptService.CurrentThread;
            Task.Run(() => Get(url, noCache, headers))
                .ContinueWith(t =>
                {
                    result = t.Result.ReadString();
                    ScriptService.ResumeThread(thread);
                });
            ScriptService.YieldThread();
            return result;
        }

        /// <summary>
        /// Performs an HTTP POST request.
        /// </summary>
        /// <param name="url">The request destination.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="contentType">The value for the 'Content-Type' header.</param>
        /// <param name="compress">Determines whether data should be gzip'd.</param>
        /// <param name="headers">The headers to send with the request.</param>
        /// <returns></returns>
        [YieldFunction]
        public string PostAsync(string url, string data,
            string contentType = "application/json", bool compress = false, Dictionary<string, object> headers = null)
        {
            string result = null;
            var thread = ScriptService.CurrentThread;
            Task.Run(() => Post(url, data, contentType, compress, headers))
                .ContinueWith(t =>
                {
                    result = t.Result;
                    ScriptService.ResumeThread(thread);
                });
            ScriptService.YieldThread();
            return result;
        }
    }

    public partial class HttpService
    {
        internal static HttpService Service;

        /// <summary>
        /// If the data to be sent in a POST request exceeds this size it will be compressed.
        /// </summary>
        public const int NumberOfBytesForGZip = 256;

        private static ILogger _logger;
        private static CookieContainer _robloxCookieContainer;

        internal static object GetExisting()
        {
            return DataModel.GetService<HttpService>();
        }

        internal static void Init()
        {
            var httpService = DataModel.GetService<HttpService>();
            _logger = httpService.Logger;

            var cookies = new[]
            {
                "GuestData", "UserID=-777175668",
                "RBXEventTrackerV2", "CreateDate=10/21/2015 8:26:24 AM&rbxid=60320231&browserid=3706651814",
                "rbx-ip", "",
                "RBXMarketing", "FirstHomePageVisit=1",
                "RBXSource",
                "rbx_acquisition_time=10/21/2015 8:26:12 AM&rbx_acquisition_referrer=&rbx_medium=Direct&rbx_source=&rbx_campaign=&rbx_adgroup=&rbx_keyword=&rbx_matchtype=&rbx_send_info=1",
                "RBXViralAcquisition", "time=10/21/2015 8:26:12 AM&referrer=&originatingsite="
            };

            _robloxCookieContainer = new CookieContainer(cookies.Length);

            for (var i = 0; i < cookies.Length; i += 2)
            {
                var key = cookies[i];
                var val = cookies[i + 1];
                var cookie = new Cookie(key, val) { Domain = "www.roblox.com" };
                _robloxCookieContainer.Add(cookie);
            }
        }

        internal static async Task<MemoryStream> Get(string url, bool noCache = false, Dictionary<string, object> headers = null)
        {
            try
            {
                var uri = new Uri(url);

                var cookieContainer = new CookieContainer();

                if (uri.Host == "www.roblox.com")
                {
                    cookieContainer = _robloxCookieContainer;
                }

                using (
                    var handler = new HttpClientHandler
                    {
                        CookieContainer = cookieContainer,
                        AllowAutoRedirect = true,
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    })
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("Accept",
                        "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
                    client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en-US;q=0.8,en;q=0.6");
                    client.DefaultRequestHeaders.Add("User-Agent",
                        $"Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10136 dEngine/{Engine.Version}");
                    client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                    client.DefaultRequestHeaders.Add("Connection", "keep-alive");

                    if (noCache)
                        client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");


                    if (headers != null)
                        foreach (var header in headers)
                            client.DefaultRequestHeaders.Add(header.Key, header.Value.ToString());

                    using (var response = await client.GetAsync(uri))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            using (var content = response.Content)
                            {
                                var mem = new MemoryStream();
                                // when content is disposed by the using statement, it also closes the stream, so create a new copy of it.
                                var stream = content.ReadAsStreamAsync().Result;
                                stream.CopyTo(mem);
                                stream.Close();
                                mem.Position = 0;
                                return mem;
                            }
                        }
                        throw new HttpRequestException("HTTP bad response code: " + response.StatusCode);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"HTTP GET request failed for \"{url}\".");
                return null;
            }
        }

        internal static async Task<string> Post(string url, string data, string contentType, bool compress, Dictionary<string, object> headers)
        {
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var contentLength = stream.Length;

            if (compress || data.Length * sizeof(char) > NumberOfBytesForGZip)
            {
                stream = new GZipStream(stream, CompressionMode.Compress);
            }

            var content = new StreamContent(stream);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Length", contentLength.ToString());
                if (headers != null)
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value.ToString());
                try
                {
                    var result = await client.PostAsync(url, content);
                    return await result.Content.ReadAsStringAsync();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            }
        }

        private static Stream StringToStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}