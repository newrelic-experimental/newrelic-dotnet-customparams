using NewRelic.Agent.Api;
using NewRelic.Agent.Extensions.Providers.Wrapper;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Custom.Providers.Wrapper.AspNet
{
    public class ReflectedHttpActionDescriptorWrapper : IWrapper
    {
        private const string AssemblyName = "System.Web.Http";
        private const string TypeName = "System.Web.Http.Controllers.ReflectedHttpActionDescriptor";
        private const string MethodName = "ExecuteAsync";
        private const string REQUEST_PROP_NAME = "Request";
        private ApiRequestParser requestParser;


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

        public AfterWrappedMethodDelegate BeforeWrappedMethod(InstrumentedMethodCall instrumentedMethodCall,
            IAgent agent, ITransaction transaction)
        {
            requestParser = requestParser ?? new ApiRequestParser(agent);

            object[] methodArgs = instrumentedMethodCall.MethodCall.MethodArguments;
            if (methodArgs.Length > 0)
            {
                //Effectively calling something like this below: object request = controllerContext.Request"); 
                object controllerContextObject = methodArgs[0];
                if (controllerContextObject == null)
                    throw new NullReferenceException(nameof(controllerContextObject));
                object request = controllerContextObject?.GetType()?.GetProperty(REQUEST_PROP_NAME)?.GetValue(controllerContextObject);
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