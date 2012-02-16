using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutomatedTester.BrowserMob
{
    public class Client
    {
        private readonly string _url;
        private readonly Int16 _port;
        private readonly string _proxy;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public Client(string url)
        {
            if (String.IsNullOrWhiteSpace(url))
                throw new ArgumentException("url not supplied", "url");

            _url = url;

            using (var response = MakeRequest(String.Format("{0}/proxy", url), "POST"))
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        public void NewHar(string reference = null)
        {
            MakeRequest(String.Format("{0}/proxy/{1}/har", _url, _port), "PUT", reference);
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
            }
            else            
                request.ContentLength = 0;

            return request.GetResponse();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        public void NewPage(string reference)
        {        
            MakeRequest(String.Format("{0}/proxy/{1}/har/pageRef", _url, _port), "PUT", reference);            
        }

        /// <summary>
        /// 
        /// </summary>
        public string Har
        {
            get
            {
                var response = MakeRequest(String.Format("{0}/proxy/{1}/har", _url, _port), "GET");
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
        }

        public void Limits(Dictionary<String, int> options)
        {

        }

        public string SeleniumProxy
        {
            get { return _proxy; }
        }

        public void WhiteList(String regexp, int statusCode)
        {

        }
        public void Blacklist(String regexp, int statusCode)
        {

        }        

        /// <summary>
        /// shuts down the proxy and closes the port
        /// </summary>
        public void Close()
        {
            MakeRequest(String.Format("{0}/proxy/{1}", _url, _port), "DELETE");
        }

    }
}