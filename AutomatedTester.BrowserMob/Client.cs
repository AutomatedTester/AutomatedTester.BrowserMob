using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutomatedTester.BrowserMob
{
    public class Client
    {
        private readonly string _url;
        private readonly Int16 _port;
        private readonly string _proxy;
        private readonly string _baseUrlProxy;

        public Client(string url)
        {
            if (String.IsNullOrEmpty(url))
                throw new ArgumentException("url not supplied", "url");

            _url = url;
            _baseUrlProxy = String.Format("{0}/proxy", _url);
            using (var response = MakeRequest(_baseUrlProxy, "POST"))
            {
                var responseStream = response.GetResponseStream();
                if (responseStream == null)
                    throw new Exception("No response from proxy");

                using (var responseStreamReader = new StreamReader(responseStream))
                {
                    var jsonReader = new JsonTextReader(responseStreamReader);
                    var token = JToken.ReadFrom(jsonReader);
                    var portToken = token.SelectToken("port");                    
                    if (portToken == null) 
                        throw new Exception("No port number returned from proxy");

                    _port = (Int16) portToken;
                }            
            }

            var parts = url.Split(':');
            _proxy = parts[1].TrimStart('/') + ":" + _port;
        }
        
        public void NewHar(string reference = null)
        {
            MakeRequest(String.Format("{0}/{1}/har", _baseUrlProxy, _port), "PUT", reference);
        }

        private static WebResponse MakeRequest(string url, string method, string reference = null)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);     
            request.Method = method;
            if (reference != null)
            {
                byte[] requestBytes = Encoding.UTF8.GetBytes(reference);
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(requestBytes, 0, requestBytes.Length);
                    requestStream.Close();
                }
                
                request.ContentType = "application/x-www-form-urlencoded";
            }
            else            
                request.ContentLength = 0;
            
            return request.GetResponse();
        }

        public void NewPage(string reference)
        {
            MakeRequest(String.Format("{0}/{1}/har/pageRef", _baseUrlProxy, _port), "PUT", reference);            
        }

        public string GetHar()
        {
            var response = MakeRequest(String.Format("{0}/{1}/har", _baseUrlProxy, _port), "GET");
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null)
                    return null;

                using (var responseStreamReader = new StreamReader(responseStream))
                {
                    return responseStreamReader.ReadToEnd();
                }
            }
        }

        public void Limits(Dictionary<string, int> options)
        {

        }

        public string SeleniumProxy
        {
            get { return _proxy; }
        }       

        public void WhiteList(string regexp, int statusCode)
        {
            string data = FormatBlackOrWhiteListFormData(regexp, statusCode);
            MakeRequest(String.Format("{0}/{1}/whitelist", _baseUrlProxy, _port), "PUT", data);                                    
        }

        public void Blacklist(string regexp, int statusCode)
        {
            string data = FormatBlackOrWhiteListFormData(regexp, statusCode);
            MakeRequest(String.Format("{0}/{1}/blacklist", _baseUrlProxy, _port), "PUT", data); 
        }        

        private static string FormatBlackOrWhiteListFormData(string regexp, int statusCode)
        {
            return String.Format("regex={0}&status={1}", HttpUtility.UrlEncode(regexp), statusCode);
        }

        /// <summary>
        /// shuts down the proxy and closes the port
        /// </summary>
        public void Close()
        {
            MakeRequest(String.Format("{0}/{1}", _baseUrlProxy, _port), "DELETE");
        }

    }
}