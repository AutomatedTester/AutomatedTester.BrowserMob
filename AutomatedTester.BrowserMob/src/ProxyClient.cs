using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

using AutomatedTester.BrowserMob.HAR;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutomatedTester.BrowserMob {

    public class ProxyClient : IDisposable {

        private readonly String _browserMobHost;
        private readonly Int32 _browserMobPort;
        private readonly String _browserMobUrl;

        private Int32 _proxyPort;

        /// <summary>
        ///     Create new ProxyClient
        /// </summary>
        /// <param name="browserMobHost">host which BrowserMobProxy is bound to</param>
        /// <param name="browserMobPort">port on which BrowserMobProxy is listening</param>
        public ProxyClient(String browserMobHost, Int32 browserMobPort) {
            if (String.IsNullOrEmpty(browserMobHost)) {
                throw new ArgumentNullException("browserMobHost");
            }

            if (browserMobPort <= 0) {
                throw new ArgumentOutOfRangeException("browserMobPort");
            }

            _browserMobHost = browserMobHost;
            _browserMobPort = browserMobPort;
            _browserMobUrl = String.Format("http://{0}:{1}/proxy", browserMobHost, browserMobPort);
        }

        /// <summary>
        ///     Host which BrowserMobProxy is bound to
        /// </summary>
        public String BrowserMobHost {
            get { return _browserMobHost; }
        }

        /// <summary>
        ///     Port on which BrowserMobProxy is listening
        /// </summary>
        public Int32 BrowserMobPort {
            get { return _browserMobPort; }
        }

        /// <summary>
        ///     Port of current client proxy
        /// </summary>
        public Int32 ProxyPort {
            get {
                if (_proxyPort == 0) {
                    _proxyPort = StartProxy();
                }

                return _proxyPort;
            }
        }

        public String SeleniumProxyUrl {
            get { return String.Format("http://{0}:{1}", _browserMobHost, ProxyPort); }
        }

        private Int32 StartProxy() {
            using (var response = MakeRequest(_browserMobUrl, "POST")) {
                var responseStream = response.GetResponseStream();

                if (responseStream == null) {
                    throw new IOException("No response from BrowserMobProxy");
                }

                using (var streamReader = new StreamReader(responseStream)) {
                    var jsonReader = new JsonTextReader(streamReader);
                    var token = JToken.ReadFrom(jsonReader);
                    var portToken = token.SelectToken("port");

                    if (portToken == null) {
                        throw new IOException("No port number returned from BrowserMobProxy");
                    }

                    return (Int32) portToken;
                }
            }
        }

        public void NewHar(string reference = null) {
            MakeRequest(String.Format("{0}/{1}/har", _browserMobUrl, ProxyPort), "PUT", reference);
        }

        private static WebResponse MakeRequest(string url, string method, string reference = null) {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = method;

            if (reference != null) {
                byte[] requestBytes = Encoding.UTF8.GetBytes(reference);
                using (var requestStream = request.GetRequestStream()) {
                    requestStream.Write(requestBytes, 0, requestBytes.Length);
                    requestStream.Close();
                }

                request.ContentType = "application/x-www-form-urlencoded";
            } else {
                request.ContentLength = 0;
            }

            return request.GetResponse();
        }

        private static WebResponse MakeJsonRequest(string url, string method, string payload) {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = method;

            if (payload != null) {
                request.ContentType = "text/json";
                request.ContentLength = payload.Length;
                using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                    streamWriter.Write(payload);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            } else {
                request.ContentLength = 0;
            }

            return request.GetResponse();
        }

        public void NewPage(string reference) {
            MakeRequest(String.Format("{0}/{1}/har/pageRef", _browserMobUrl, ProxyPort), "PUT", reference);
        }

        public HarResult GetHar() {
            var response = MakeRequest(String.Format("{0}/{1}/har", _browserMobUrl, ProxyPort), "GET");
            using (var responseStream = response.GetResponseStream()) {
                if (responseStream == null) {
                    return null;
                }

                using (var responseStreamReader = new StreamReader(responseStream)) {
                    var json = responseStreamReader.ReadToEnd();
                    return JsonConvert.DeserializeObject<HarResult>(json);
                }
            }
        }

        public void SetLimits(LimitOptions options) {
            if (options == null) {
                throw new ArgumentNullException("options", "LimitOptions must be supplied");
            }

            MakeRequest(String.Format("{0}/{1}/limit", _browserMobUrl, ProxyPort), "PUT", options.ToFormData());
        }

        public void WhiteList(string regexp, int statusCode) {
            string data = FormatBlackOrWhiteListFormData(regexp, statusCode);
            MakeRequest(String.Format("{0}/{1}/whitelist", _browserMobUrl, ProxyPort), "PUT", data);
        }

        public void Blacklist(string regexp, int statusCode) {
            string data = FormatBlackOrWhiteListFormData(regexp, statusCode);
            MakeRequest(String.Format("{0}/{1}/blacklist", _browserMobUrl, ProxyPort), "PUT", data);
        }

        public void RemapHost(string host, string ipAddress) {
            MakeJsonRequest(String.Format("{0}/{1}/hosts", _browserMobUrl, ProxyPort), "POST", "{\"" + host + "\":\"" + ipAddress + "\"}");
        }

        private static string FormatBlackOrWhiteListFormData(string regexp, int statusCode) {
            return String.Format("regex={0}&status={1}", HttpUtility.UrlEncode(regexp), statusCode);
        }

        /// <summary>
        ///     Shuts down the proxy and closes the port
        /// </summary>
        public void Close() {
            if (_proxyPort == 0) {
                return;
            }

            MakeRequest(String.Format("{0}/{1}", _browserMobUrl, _proxyPort), "DELETE");

            _proxyPort = 0;
        }

        /// <summary>
        ///     Invoke <see cref="Close"/>.
        /// </summary>
        public void Dispose() {
            Close();
        }

    }

}