// AnalyticsService.cs - dEngine
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Settings.Global;
using dEngine.Settings.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable ClassNeverInstantiated.Local

namespace dEngine.Services
{
    /// <summary>
    /// Service for handling analytics.
    /// </summary>
    [TypeId(205), ExplorerOrder(-1)]
    public class AnalyticsService : Service
    {
        private static readonly ConcurrentQueue<Event> _eventQueue = new ConcurrentQueue<Event>();
        private static string _productionGameKey;
        private static string _productionSecretKey;
        private static Stopwatch _sessionStopwatch;

        internal static AnalyticsService Service;
        private static bool _useProductionApi;
        private static string _authHash;

        /// <summary />
        public AnalyticsService()
        {
            Service = this;
        }

        internal static void ReportEventQueue()
        {
            var defaultAnnotations = GetDefaultAnnotations();
            while (!_eventQueue.IsEmpty)
            {
                Event e;
                if (_eventQueue.TryDequeue(out e))
                {
                    e.Send(defaultAnnotations);
                }
            }
        }

        internal static string SessionId { get; private set; }

        internal static int ServerTimestamp { get; private set; }

        /// <summary>
        /// Determines whether the production or sandbox endpoint is used.
        /// </summary>
        public static bool UseProductionAPI
        {
            get { return _useProductionApi; }
            set
            {
                if (value == _useProductionApi)
                    return;
                _useProductionApi = value;
                UpdateRoutes();
            }
        }

        /// <summary>
        /// Determines whether the production or sandbox endpoint is used.
        /// </summary>
        internal static string GameKey => UseProductionAPI ? _productionGameKey : "5c6bcb5402204249437fb5a7a80a4959";

        /// <summary>
        /// Determines whether the production or sandbox endpoint is used.
        /// </summary>
        internal static string SecretKey
            => UseProductionAPI ? _productionSecretKey : "16813a12f718bc5c620f56944e1abc3ea13ccbac";

        private static string InitRoute { get; set; }
        private static string EventsRoute { get; set; }

        internal static bool Initialized { get; set; }

        /// <summary>
        /// Sets the analytics game key to use for production.
        /// </summary>
        public void SetProductionGameKey(string key)
        {
            _productionGameKey = key;
            UpdateRoutes();
        }

        /// <summary>
        /// Sets the analytics secret key to use for production.
        /// </summary>
        public void SetProductionSecretKey(string key)
        {
            _productionSecretKey = key;
            UpdateRoutes();
        }

        private static bool Init()
        {
            var json =
                $"{{\"platform\":\"{DebugSettings.OsPlatform}\", \"os_version\":\"{DebugSettings.OsVersion}\", \"sdk_version\":\"rest api v2\"}}";

            // TODO: SecretKey might have to be provided by the server
            _authHash = GenerateHmac(json, SecretKey);

            string resultJson;

            try
            {
                resultJson =
                    HttpService.Post(InitRoute, json, "application/json", true, new Dictionary<string, object>
                    {
                        {"Authorization", _authHash}
                    }).Result;
            }
            catch (AggregateException)
            {
                //Service.Logger.Error(e.Message);
                return false;
            }

            var result = JsonConvert.DeserializeObject<AnalyticsInitResult>(resultJson);
            ServerTimestamp = result.ServerTimestamp;
            return result.Enabled;
        }

        private static string GenerateHmac(string json, string secretKey)
        {
            var encoding = new System.Text.UTF8Encoding();

            var messageBytes = encoding.GetBytes(json);
            var keyByte = encoding.GetBytes(secretKey);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

        private static void UpdateRoutes()
        {
        }

        internal static object GetExisting()
        {
            return DataModel.GetService<AnalyticsService>();
        }

        private static Dictionary<string, object> GetDefaultAnnotations()
        {
            var annotations = new Dictionary<string, object>
            {
                ["device"] = UserAnalyticsSettings.Device,
                ["v"] = UserAnalyticsSettings.Version,
                ["user_id"] = LoginService.SteamId.ToString(),
                ["client_ts"] = DateTime.UnixTimestamp(),
                ["sdk_version"] = UserAnalyticsSettings.SdkVersion,
                ["os_Version"] = DebugSettings.OsVersion,
                ["manufacturer"] = DebugSettings.Manufacturer,
                ["platform"] = DebugSettings.OsPlatform.ToString(),
                ["session_id"] = SessionId,
                ["session_num"] = UserAnalyticsSettings.SessionCount,
                ["engine_version"] = UserAnalyticsSettings.EngineVersion,
                ["connection_type"] = DebugSettings.ConnectionType
            };

            if (UserAnalyticsSettings.LimitAdTracking)
                annotations["limit_ad_tracking"] = true;

            if (UserAnalyticsSettings.Jailbroken)
                annotations["jailbroken"] = true;

            if (UserAnalyticsSettings.Gender != Gender.Unspecified)
                annotations["gender"] = UserAnalyticsSettings.Gender.ToString();

            if (UserAnalyticsSettings.BirthYear != 0)
                annotations["birth_year"] = UserAnalyticsSettings.BirthYear;

            return annotations;
        }

        /// <summary>
        /// Sends a custom report to the analytics service.
        /// </summary>
        public void Report(string id, float value)
        {
            if (id.Length > 32)
                throw new ArgumentException("Id string was too long.", nameof(id));

            var segmentCount = id.Count(c => c == ':');

            if (segmentCount > 5)
                throw new ArgumentException("Id string contains too many segments. (5 max)", nameof(id));
            if (segmentCount < 1)
                throw new ArgumentException("Id string did not contain any segments. (1 min)", nameof(id));

            var design = new Design(id, value);
            design.Enqueue();
        }

        internal static void BeginSession()
        {
            if (!Initialized)
                Initialized = Init();

            if (!Initialized)
            {
                Service.Logger.Warn("Could not begin analytics session: init failed.");
                return;
            }

            _sessionStopwatch = Stopwatch.StartNew();

            SessionId = Guid.NewGuid().ToString("D");
            var startEvent = new SessionStart();
            startEvent.Enqueue();
        }

        internal static void EndSession()
        {
            if (_sessionStopwatch == null)
                return;
            _sessionStopwatch.Stop();
            var endEvent = new SessionEnd((int)_sessionStopwatch.Elapsed.TotalSeconds);
            endEvent.Enqueue();
            _sessionStopwatch = null;
        }

        [JsonObject(MemberSerialization.OptIn)]
        private class AnalyticsInitResult
        {
#pragma warning disable 649
#pragma warning disable 169
            [JsonProperty("enabled")]
            public bool Enabled;

            [JsonProperty("flags")]
            public string[] Flags;

            [JsonProperty("server_ts")]
            public int ServerTimestamp;
#pragma warning restore 649
#pragma warning restore 169
        }

        [JsonObject(MemberSerialization.OptIn)]
        internal abstract class Event
        {
            public abstract string Category { get; }

            internal void Send(Dictionary<string, object> defaultAnnotations)
            {
                var obj = JObject.FromObject(this);
                var defaultAnnos = JObject.FromObject(defaultAnnotations);
                obj.Merge(defaultAnnos);
#pragma warning disable 4014
                HttpService.Post(EventsRoute, obj.ToString(), "application/json", true,
                    new Dictionary<string, object> { { "Authorization", _authHash } });
#pragma warning restore 4014
            }

            public void Enqueue()
            {
                _eventQueue.Enqueue(this);
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        internal class SessionStart : Event
        {
            [JsonProperty("category")]
            public override string Category => "user";
        }

        [JsonObject(MemberSerialization.OptIn)]
        internal class SessionEnd : Event
        {
            public SessionEnd(int length)
            {
                Length = length;
            }

            [JsonProperty("category")]
            public override string Category => "session_end";

            public int Length { get; set; }
        }


        [JsonObject(MemberSerialization.OptIn)]
        internal class ReceiptInfo
        {
            public ReceiptInfo(string store, string receipt, string signature)
            {
                Store = store;
                Receipt = receipt;
                Signature = signature;
            }

            [JsonProperty("store")]
            public string Store { get; set; }

            [JsonProperty("receipt")]
            public string Receipt { get; set; }

            [JsonProperty("signature")]
            public string Signature { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        internal class Business : Event
        {
            public Business(string eventId, int amount, string current, int transactionCount, ReceiptInfo receiptInfo)
            {
                EventId = eventId;
                Amount = amount;
                Currency = current;
                TransactionCount = transactionCount;
                ReceiptInfo = receiptInfo;
            }

            [JsonProperty("category")]
            public override string Category => "business";

            [JsonProperty("event_id")]
            public string EventId { get; set; }

            [JsonProperty("amount")]
            public int Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }

            [JsonProperty("transaction_num")]
            public int TransactionCount { get; set; }

            [JsonProperty("cart_type")]
            public string Cart { get; set; }

            [JsonProperty("receipt_info")]
            public ReceiptInfo ReceiptInfo { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        internal class Resource : Event
        {
            public Resource(string eventId, int amount)
            {
                EventId = eventId;
                Amount = amount;
            }

            [JsonProperty("category")]
            public override string Category => "resource";

            [JsonProperty("event_id")]
            public string EventId { get; set; }

            [JsonProperty("amount")]
            public int Amount { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        internal class Progression : Event
        {
            public Progression(string eventId, int attemptNum)
            {
                EventId = eventId;
                AttemptNum = attemptNum;
            }

            [JsonProperty("category")]
            public override string Category => "progression";

            [JsonProperty("event_id")]
            public string EventId { get; set; }

            [JsonProperty("attempt_num")]
            public int AttemptNum { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        internal class Design : Event
        {
            public Design(string eventId, float value)
            {
                EventId = eventId;
                Value = value;
            }

            [JsonProperty("category")]
            public override string Category => "design";

            [JsonProperty("event_id")]
            public string EventId { get; set; }

            [JsonProperty("value")]
            public float Value { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        internal class Error : Event
        {
            [JsonProperty("category")]
            public override string Category => "error";

            [JsonProperty("severity")]
            public LogLevel LogLevel { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }
    }
}