using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using Nancy.Rest.Annotations.Enums;


namespace Nancy.Rest.Client.Helpers
{
    internal static class Extensions
    {

        internal static bool IsAsyncMethod(this MethodInfo minfo)
        {
            return (minfo.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null);
        }

        internal static HttpMethod ToHttpMethod(this Verbs verb)
        {
            switch (verb)
            {
                case Verbs.Get:
                    return HttpMethod.Get;
                case Verbs.Delete:
                    return HttpMethod.Delete;
                case Verbs.Head:
                    return HttpMethod.Head;
                case Verbs.Options:
                    return HttpMethod.Options;
                case Verbs.Patch:
                    return new HttpMethod("PATCH");
                case Verbs.Post:
                    return HttpMethod.Post;
                case Verbs.Put:
                    return HttpMethod.Put;
            }
            return HttpMethod.Get;
        }
        internal static List<T> GetCustomAttributesFromInterfaces<T>(this Type minfo) where T : Attribute
        {
            List<T> rests = new List<T>();
            List<Type> types = new List<Type> { minfo };
            types.AddRange(minfo.GetInterfaces());
            foreach (Type t in types)
            {
                rests.AddRange(t.GetCustomAttributes(typeof(T)).Cast<T>().ToList());
            }
            return rests;

        }
    }
}
