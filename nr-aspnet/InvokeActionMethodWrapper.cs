
using NewRelic.Agent.Extensions.Providers.Wrapper;
using System;

namespace Custom.Providers.Wrapper.AspNet
{
    public class InvokeActionMethodWrapper : IWrapper
    {
        private const string AssemblyName = "System.Web.Mvc";
        private const string TypeName = "System.Web.Mvc.ControllerActionInvoker";
        private const string MethodName = "InvokeActionMethod";

        private RequestParser requestParser = null;

        public bool IsTransactionRequired => true;

        public CanWrapResponse CanWrap(InstrumentedMethodInfo instrumentedMethodInfo)
        {
            var method = instrumentedMethodInfo.Method;
            var canWrap = method.MatchesAny(
                assemblyNames: new[] { AssemblyName },
                typeNames: new[] { TypeName },
                methodNames: new[] { MethodName }
            );

            return new CanWrapResponse(canWrap);
        }
        

        /// <summary>
        /// 
        /// Executes before the InvokeActionMethod execution to look up configured headers and adds to current transaction as custom parameters
        /// </summary>
        /// <param name="instrumentedMethodCall">The method that was called.</param>
        /// <param name="agent">Agent Wrapper API.</param>
        /// <param name="transaction">Transaction Wrapper API</param>
        /// <returns></returns>
        public AfterWrappedMethodDelegate BeforeWrappedMethod(InstrumentedMethodCall instrumentedMethodCall,
            IAgent agent, ITransaction transaction)
        {
            requestParser = requestParser ?? new RequestParser(agent);
            
            object[] methodArgs = instrumentedMethodCall.MethodCall.MethodArguments;
            if (methodArgs.Length > 0)
            {
                //Effectively calling something like this below: string header = controllerContext.HttpContext.Request.Headers.Get("HEADER_NAME"); 
                object controllerContextObject = methodArgs[0];
                if (controllerContextObject == null)
                    throw new NullReferenceException(nameof(controllerContextObject));
                object httpContext = controllerContextObject?.GetType()?.GetProperty("HttpContext")?.GetValue(controllerContextObject);
                object request = httpContext?.GetType()?.GetProperty("Request")?.GetValue(httpContext);
                requestParser.parse(request);
            }


            // We are using a variant of GetDelegateFor that doesn't pass down a return value to the local methods since we don't need the return.
            return Delegates.GetDelegateFor(
                onSuccess: OnSuccess
            );

            void OnSuccess()
            {
            }
        }
    }

}
