using NewRelic.Agent.Api;
using NewRelic.Agent.Configuration;
using NewRelic.Agent.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Custom.Providers.Wrapper.AspNet
{
    public class ApiRequestParser
    {

        private IAgent agent;
        private bool initialized = false;

        /// <summary>
        /// Store the header names read from the newrelic.config file 
        /// </summary>
        private string[] configuredRequestProperties = null;
        private string[] configuredHeaders = null;
        private string prefix = null; 
        private const string REQUEST_URI_PROP_NAME = "RequestUri";
        private const string URL_PROP_NAME = "Url";

        public ApiRequestParser(IAgent agent)
        {
            this.agent = agent;
        }

        public void init()
        {
            /// Read and setup the configured headers from newrelic.config file
            IReadOnlyDictionary<string, string> appSettings = agent.Configuration.GetAppSettings();

            string reqPropeties = null;
            if (appSettings.TryGetValue("requestProperties", out reqPropeties))
            {
                configuredRequestProperties = reqPropeties?.Split(',').Select(p => p.Trim()).ToArray<string>();
                agent.Logger.Log(Level.Info, "Custom AspNet Extension: These HTTP request properties will be read and added to NewRelic transaction by API Wrapper: " + "[" + String.Join(",", configuredRequestProperties) + "]");
            }
            else
            {
                configuredRequestProperties = null;
            }
            string reqHeaders = null;
            if (appSettings.TryGetValue("requestHeaders", out reqHeaders))
            {
                configuredHeaders = reqHeaders?.Split(',').Select(p => p.Trim()).ToArray<string>();
                agent.Logger.Log(Level.Info, "Custom AspNet Extension: These HTTP headers will be read and added to NewRelic transaction by API Wrapper: " + "[" + String.Join(",", configuredHeaders) + "]");
            }
            else
            {
                configuredHeaders = null;
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
            if (configuredRequestProperties != null)
            {
                foreach (var cRequestProperty in configuredRequestProperties)
                {
                    if (cRequestProperty.Equals("Url"))
                    {
                        object requestUriProp = request?.GetType()?.GetProperty(REQUEST_URI_PROP_NAME)?.GetValue(request);
                        Uri requestUri = requestUriProp as Uri;
                        string requestUriValue = requestUri.AbsoluteUri;
                        if (requestUriValue != null)
                        {
                            agent.CurrentTransaction.AddCustomAttribute(prefix + URL_PROP_NAME, requestUriValue);
                        }
                    }
                    else
                    {
                    }
                }
            }
            if (configuredHeaders != null)
            {
                object headers = request?.GetType()?.GetProperty("Headers")?.GetValue(request);
                foreach (var cHeader in configuredHeaders)
                {
                    object headerValueObject = headers.GetType()?.GetMethod("GetValues", new Type[] { typeof(string) })?.Invoke(headers, new object[] { cHeader });
                    if (headerValueObject != null)
                    {
                        IEnumerable<string> strs = headerValueObject as IEnumerable<string>;
                        string headerValue = string.Join(",", strs.ToArray<string>());
                        if (headerValue != null)
                        {
                            agent.CurrentTransaction.AddCustomAttribute(prefix + cHeader, headerValue);
                        }
                    }
                }
            }
        }

    }
}