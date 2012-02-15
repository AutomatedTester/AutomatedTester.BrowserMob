using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutomatedTester.BrowserMob
{
    public class Client
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public Client(String url)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void NewHar()
        {
            NewHar("");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        public void NewHar(String reference)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        public void NewPage(String reference)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<String, T> Har
        {
            get
            {
                
            }
        }   

        public void Limits(Dictionary<String, int> options)
        {
            
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
            
        }

    }
}