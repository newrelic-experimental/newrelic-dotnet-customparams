using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using NewRelic.Agent.Extensions.Logging;
using NewRelic.Agent.Api;

namespace Custom.Providers.Wrapper.AspNet
{
    public class RequestParser
    {
        private IAgent agent;
        private bool initialized = false;

        /// <summary>
        /// Store the header names read from the newrelic.config file 
        /// </summary>
        private string[] configuredHeaders = null;
        private string[] configuredParams = null;
        private string[] configuredCookies = null;
        private string prefix = null;

        public RequestParser(IAgent agent)
        {
            this.agent = agent;
        }

        public void init()
        {
            /// Read and setup the configured headers from newrelic.config file
            IReadOnlyDictionary<string, string> appSettings = agent.Configuration.GetAppSettings();
            string reqHeaders = null;
            if (appSettings.TryGetValue("requestHeaders", out reqHeaders))
            {
                configuredHeaders = reqHeaders?.Split(',').Select(p => p.Trim()).ToArray<string>();
                agent.Logger.Log(Level.Info, "Custom AspNet Extension: These HTTP headers will be read and added to NewRelic transaction: " + "[" + String.Join(",", configuredHeaders) + "]");
            }
            else
            {
                configuredHeaders = null;
            }

            string reqParams = null;
            if (appSettings.TryGetValue("requestParams", out reqParams))
            {
                configuredParams = reqParams?.Split(',').Select(p => p.Trim()).ToArray<string>();
                agent.Logger.Log(Level.Info, "Custom AspNet Extension: These HTTP params will be read and added to NewRelic transaction: " + "[" + String.Join(",", configuredParams) + "]");
            }
            else
            {
                configuredParams = null;
            }

            string reqCookies = null;
            if (appSettings.TryGetValue("requestCookies", out reqCookies))
            {
                configuredCookies = reqCookies?.Split(',').Select(p => p.Trim()).ToArray<string>();
                agent.Logger.Log(Level.Info, "Custom AspNet Extension: These HTTP cookies will be read and added to NewRelic transaction: " + "[" + String.Join(",", configuredCookies) + "]");
            }
            else
            {
                configuredCookies = null;
            }

            if (appSettings.TryGetValue("prefix", out prefix))
            {
                prefix = prefix ?? "";
            }
            else
            {
                prefix = "";
            }

            initialized = true;
        }

        public void parse(object request)
        {
            if (!initialized)
            {
                init();
            }
            if (configuredHeaders != null)
            {
                object headers = request?.GetType()?.GetProperty("Headers")?.GetValue(request);
                NameValueCollection headerCollection = headers as NameValueCollection;
                foreach (var cHeader in configuredHeaders)
                {
                    string headerValue = headerCollection?.Get(cHeader);
                    if (headerValue != null)
                    {
                        //InternalApi.AddCustomParameter(prefix + cHeader, headerValue);
                       agent.CurrentTransaction.AddCustomAttribute(prefix + cHeader, headerValue);

                    }
                }
            }
            if (configuredParams != null)
            {
                object queryStringParams = request?.GetType()?.GetProperty("QueryString")?.GetValue(request);
                object contentType = request?.GetType()?.GetProperty("ContentType")?.GetValue(request);
                object formParams = null;
                if (contentType.Equals("application/x-www-form-urlencoded"))
                {
                    formParams = request?.GetType()?.GetProperty("Form")?.GetValue(request);
                }
                
                NameValueCollection queryStringParamCollection = queryStringParams as NameValueCollection;
                NameValueCollection formParamCollection = formParams as NameValueCollection;

                foreach (var cParam in configuredParams)
                {
                    string paramValue = queryStringParamCollection?.Get(cParam);
                    if (paramValue != null)
                    {
                        //InternalApi.AddCustomParameter(prefix + cParam, paramValue);
                        agent.CurrentTransaction.AddCustomAttribute(prefix + cParam, paramValue);
                    }
                    paramValue = formParamCollection?.Get(cParam);
                    if (paramValue != null)
                    {
                        //InternalApi.AddCustomParameter(prefix + cParam, paramValue);
                        agent.CurrentTransaction.AddCustomAttribute(prefix + cParam, paramValue);
                    }
                }
            }
            if (configuredCookies != null)
            {
                object cookies = request?.GetType()?.GetProperty("Cookies")?.GetValue(request);
                
                foreach (var cCookie in configuredCookies)
                {
                    object httpCookieObject = cookies?.GetType()?.GetMethod("Get", new Type[] { typeof(string)}).Invoke(cookies, new object[] { cCookie });
                    
                    if (httpCookieObject != null)
                    {
                        //Does this work for mulitple cookie values? Code probably needs review
                        object cval = httpCookieObject?.GetType()?.GetProperty("Value")?.GetValue(httpCookieObject);
                        //InternalApi.AddCustomParameter(prefix + cCookie, cval.ToString());
                        agent.CurrentTransaction.AddCustomAttribute(prefix + cCookie, cval.ToString());
                        //agent.Logger.Log(Level.Info, "Custom AspNet Extension: Value of HTTP cookie" + cCookie + "]=" + cval);
                    }
                }
            }
        }
    }
}
