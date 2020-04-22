using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using DevExpress.Utils;
using DevExpress.Xpo.Logger;
using DevExpress.Xpo.Logger.Transport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XpoProfilerConsole {
    internal class WebApiLogClient : ILogClient {
        Host host;
        static readonly TimeSpanJsonConverter timeSpanConverter = new TimeSpanJsonConverter();
        public string Login { get; set; }
        public string Password { get; set; }
        public string AuthToken { get; set; }
        public WebApiLogClient(Host host) {
            this.host = host;
        }
        public WebApiLogClient(Host host, string login, string password, string token) {
            this.host = host;
            this.Login = login;
            this.Password = password;
            this.AuthToken = token;
        }
        string GetServiceUrl() {
            string protocol = "";
            if(!host.HostAddress.StartsWithInvariantCultureIgnoreCase("http://")
                && !host.HostAddress.StartsWithInvariantCultureIgnoreCase("https://")) {
                protocol = "http://";
            };
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}:{2}/{3}/", protocol, host.HostAddress, host.Port, host.ServiceName);
        }
        string GetHttpResponse(string url) {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            if(!string.IsNullOrEmpty(Login)) {
                request.Credentials = new NetworkCredential(Login, Password);
            }
            if(!string.IsNullOrEmpty(AuthToken)) {
                request.Headers.Add("Authorization", string.Concat("XpoProfilerToken ", AuthToken));
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using(StreamReader str = new StreamReader(response.GetResponseStream())) {
                return str.ReadToEnd();
            }
        }
        public LogMessage[] GetCompleteLog() {
            string url = string.Concat(GetServiceUrl(), "GetCompleteLog");
            string json = GetHttpResponse(url);
            return JsonConvert.DeserializeObject<LogMessage[]>(json, timeSpanConverter);
        }
        public LogMessage GetMessage() {
            string url = string.Concat(GetServiceUrl(), "GetMessage");
            string json = GetHttpResponse(url);
            return JsonConvert.DeserializeObject<LogMessage>(json, timeSpanConverter);
        }
        public LogMessage[] GetMessages(int messagesAmount) {
            string url = string.Concat(GetServiceUrl(), "GetCompleteLog?messagesAmount=", messagesAmount.ToString());
            string json = GetHttpResponse(url);
            return JsonConvert.DeserializeObject<LogMessage[]>(json, timeSpanConverter);
        }
        public bool Start() {
            return true;
        }
        public bool Stop() {
            return true;
        }
        public void Dispose() {
        }
    }

    class TimeSpanJsonConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(TimeSpan);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            switch(reader.TokenType) {
                case JsonToken.String:
                    string val = reader.Value.ToString();
                    TimeSpan ts;
                    if(TimeSpan.TryParse(val, CultureInfo.InvariantCulture, out ts)) {
                        return ts;
                    }
                    throw new InvalidOperationException(string.Format("Cannot deserialize TimeSpan from string '{0}'.", val));
                case JsonToken.StartObject:
                    JObject jObject = JObject.Load(reader);
                    JToken ticks = jObject["ticks"];
                    if(ticks != null) {
                        return new TimeSpan(ticks.ToObject<int>());
                    };
                    throw new InvalidOperationException(string.Format("Cannot deserialize TimeSpan from JSON '{0}'.", jObject));
            }
            throw new InvalidOperationException(string.Format("Cannot deserialize TimeSpan from token '{0}'", reader.TokenType));
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
