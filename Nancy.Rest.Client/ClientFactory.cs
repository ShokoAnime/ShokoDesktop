using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dynamitey;
using ImpromptuInterface;
using Nancy.Rest.Annotations.Attributes;
using Nancy.Rest.Client.ContractResolver;
using Nancy.Rest.Client.Helpers;
using Nancy.Rest.Client.Rest;
using Newtonsoft.Json;
using ParameterType = Nancy.Rest.Client.Helpers.ParameterType;

namespace Nancy.Rest.Client
{
    public class ClientFactory
    {

        public static T Create<T>(string path, Dictionary<Type, Type> deserializationmappings=null, string defaultlevelqueryparametername="level", string defaultexcludtagsqueryparametername="excludetags", int defaulttimeoutinseconds=60, IWebProxy proxy=null) where T : class
        {
            List<RestBasePath> paths = typeof(T).GetCustomAttributesFromInterfaces<RestBasePath>().ToList();
            if (paths.Count > 0)
            {
                string s = paths[0].BasePath;
                if (path.EndsWith("/"))
                    path = path.Substring(0, path.Length - 1);
                if (s.StartsWith("/"))
                    s = s.Substring(1);
                path = path + "/" + s;
            }
            if (!path.EndsWith("/"))
                path = path + "/";
            return Create<T>(path, int.MaxValue, null, deserializationmappings,defaultlevelqueryparametername, defaultexcludtagsqueryparametername,defaulttimeoutinseconds);
        }
        private static T Create<T>(string path, int level, IEnumerable<string> tags, Dictionary<Type, Type> deserializationmappings, string defaultlevelqueryparametername, string defaultexcludtagsqueryparametername, int defaulttimeoutinseconds,bool filter=true, IWebProxy proxy=null) where T: class
        {
            dynamic dexp = new ExpandoObject();

            IDictionary<string, object> exp = (IDictionary<string, object>) dexp;
            dexp.DYN_defaultlevelqueryparametername = defaultlevelqueryparametername;
            dexp.DYN_defaultexcludtagsqueryparametername = defaultexcludtagsqueryparametername;
            dexp.DYN_level = level;
            dexp.DYN_tags = tags;
            dexp.DYN_deserializationmappings = deserializationmappings;
            dexp.DYN_defaulttimeoutinseconds = defaulttimeoutinseconds;
            if (filter && deserializationmappings != null)
            {
                foreach (Type t in deserializationmappings.Keys)
                {
                    if (!t.IsAssignableFrom(deserializationmappings[t]))
                    {
                        throw new ArgumentException("The mapping type '"+deserializationmappings[t].Name +"' is not child of '"+t.Name+"'");
                    }
                }
            }
            bool hasfilterinterface = (typeof(T).GetInterfaces().Any(a => a.Name == typeof(Interfaces.IFilter<>).Name));
            List<Type> ifaces=new List<Type>() { typeof(T)};
            ifaces.AddRange(typeof(T).GetInterfaces());
            foreach (MethodInfo m in ifaces.SelectMany(a=>a.GetMethods()))
            {
                List<Annotations.Attributes.Rest> rests = m.GetCustomAttributes<Annotations.Attributes.Rest>().ToList();
                if (rests.Count > 0)
                {
                    MethodDefinition defs = new MethodDefinition
                    {
                        RestAttribute = rests[0],
                        BasePath = path,
                        Parameters = m.GetParameters().Select(a => new Tuple<string, Type>(a.Name, a.ParameterType))
                            .ToList(),
                        ReturnType = m.ReturnType
                    };
                    if (hasfilterinterface && (m.Name == "FilterWithLevel" || m.Name== "FilterWithTags" || m.Name== "FilterWithLevelAndTags"))
                        continue;

                    if (m.IsAsyncMethod())
                    {

                        switch (defs.Parameters.Count)
                        {
                            case 0:
                                exp[m.Name] = Return<dynamic>.Arguments(() => DoAsyncClient(dexp,defs, proxy));
                                break;
                            case 1:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic>((a) => DoAsyncClient(dexp, defs, proxy, a));
                                break;
                            case 2:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic>((a, b) => DoAsyncClient(dexp, defs, proxy, a, b));
                                break;
                            case 3:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic>((a, b, c) => DoAsyncClient(dexp, defs, proxy, a, b, c));
                                break;
                            case 4:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic>((a, b, c, d) => DoAsyncClient(dexp, defs, proxy, a, b, c, d));
                                break;
                            case 5:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e) => DoAsyncClient(dexp, defs, proxy, a, b, c, d, e));
                                break;
                            case 6:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f) => DoAsyncClient(dexp, defs, proxy, a, b, c, d, e, f));
                                break;
                            case 7:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f, g) => DoAsyncClient(dexp, defs, proxy, a, b, c, d, e, f, g));
                                break;
                            case 8:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f, g, h) => DoAsyncClient(dexp, defs, proxy, a, b, c, d, e, f, g, h));
                                break;
                            default:
                                throw new NotImplementedException("It only support till 8 parameters feel free to add more here :O");
                        }
                    }
                    else
                    {
                        switch (defs.Parameters.Count)
                        {
                            case 0:
                                exp[m.Name] = Return<dynamic>.Arguments(() => DoSyncClient(dexp, defs, proxy));
                                break;
                            case 1:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic>((a) => DoSyncClient(dexp, defs, proxy, a));
                                break;
                            case 2:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic>((a, b) => DoSyncClient(dexp, defs, proxy, a, b));
                                break;
                            case 3:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic>((a, b, c) => DoSyncClient(dexp, defs, proxy, a, b, c));
                                break;
                            case 4:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic>((a, b, c, d) => DoSyncClient(dexp, defs, proxy, a, b, c, d));
                                break;
                            case 5:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e) => DoSyncClient(dexp, defs, proxy, a, b, c, d, e));
                                break;
                            case 6:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f) => DoSyncClient(dexp, defs, proxy, a, b, c, d, e, f));
                                break;
                            case 7:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f, g) => DoSyncClient(dexp, defs, proxy, a, b, c, d, e, f, g));
                                break;
                            case 8:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f, g, h) => DoSyncClient(dexp, defs, proxy, a, b, c, d, e, f, g, h));
                                break;
                            default:
                                throw new NotImplementedException("It only support till 8 parameters feel free to add more here :O");
                        }                    
                    }
                }
            }
            T inter = Impromptu.ActLike<T>(dexp);
            if (hasfilterinterface)
            {
                if (filter)
                {
                    exp["FilterWithLevel"] = Return<T>.Arguments<int>((a) => Create<T>(path, a, null, deserializationmappings, defaultlevelqueryparametername, defaultexcludtagsqueryparametername, defaulttimeoutinseconds, false));
                    exp["FilterWithTags"] = Return<T>.Arguments<IEnumerable<string>>((a) => Create<T>(path, int.MaxValue, a, deserializationmappings, defaultlevelqueryparametername, defaultexcludtagsqueryparametername, defaulttimeoutinseconds, false));
                    exp["FilterWithLevelAndTags"] = Return<T>.Arguments<int, IEnumerable<string>>((a, b) => Create<T>(path, a, b, deserializationmappings, defaultlevelqueryparametername, defaultexcludtagsqueryparametername, defaulttimeoutinseconds, false));
                }
                else
                {
                    exp["FilterWithLevel"] = Return<T>.Arguments<int>((a) => inter);
                    exp["FilterWithTags"] = Return<T>.Arguments<IEnumerable<string>>((a) => inter);
                    exp["FilterWithLevelAndTags"] = Return<T>.Arguments<int, IEnumerable<string>>((a, b) => inter);
                }
            }
            return inter;
        }

        private static dynamic DoSyncClient(dynamic dexp, MethodDefinition def, IWebProxy proxy, params dynamic[] parameters)
        {
            Request req = CreateRequest(dexp, def, parameters);
            return Task.Run(async () => await SmallWebClient.RestRequest(req,proxy)).Result;
        }
        private static async Task<dynamic> DoAsyncClient(dynamic dexp, MethodDefinition def, IWebProxy proxy, params dynamic[] parameters)
        {
            Request req = CreateRequest(dexp, def, parameters);
            return await SmallWebClient.RestRequest(req,proxy);
        }

        private static Request CreateRequest(dynamic dexp, MethodDefinition def, dynamic[] parameters)
        {
            string defaultlevelqueryparametername = dexp.DYN_defaultlevelqueryparametername;
            string defaultexcludtagsqueryparametername = dexp.DYN_defaultexcludtagsqueryparametername;
            int level = dexp.DYN_level;
            List<string> tags = dexp.DYN_tags;
            Request request = ProcessPath(def.RestAttribute.Route, def, parameters);
            if (level != int.MaxValue)
                request.AddQueryParamater(defaultlevelqueryparametername, level.ToString());
            if (tags != null && tags.Count > 0)
                request.AddQueryParamater(defaultexcludtagsqueryparametername, string.Join(",", tags));
            request.SerializerSettings = new JsonSerializerSettings{ReferenceLoopHandling = ReferenceLoopHandling.Serialize};;
            if (dexp.DYN_deserializationmappings != null)
                request.SerializerSettings.ContractResolver = new MappedContractResolver((Dictionary<Type, Type>)dexp.DYN_deserializationmappings);
            request.Timeout = def.RestAttribute.TimeOutSeconds == 0
                ? TimeSpan.FromSeconds(dexp.DYN_defaulttimeoutinseconds)
                : TimeSpan.FromSeconds(def.RestAttribute.TimeOutSeconds);
            return request;
        }

        private static Regex rpath=new Regex("\\{(.*?)\\}",RegexOptions.Compiled);
        private static Regex options = new Regex("\\((.*?)\\)", RegexOptions.Compiled);

        //TODO It only support Nancy constrains except version and optional parameters, others types should be added.

        private static Request ProcessPath(string path, MethodDefinition def, dynamic[] parameters)
        {
            List<Parameter> pars=new List<Parameter>();
            MatchCollection collection = rpath.Matches(path);
            foreach (Match m in collection)
            {
                if (m.Success)
                {
                    string value = m.Groups[1].Value;
                    Parameter p = new Parameter();
                    p.Original = value;
                    bool optional = false;
                    string constraint = null;
                    string ops = null;
                    int idx = value.LastIndexOf('?');
                    if (idx > 0)
                    {
                        value = value.Substring(0, idx);
                        optional = true;
                    }
                    idx = value.LastIndexOf(':');
                    if (idx >= 0)
                    {
                        constraint = value.Substring(idx + 1);
                        Match optmatch = options.Match(constraint);
                        if (optmatch.Success)
                        {
                            ops = optmatch.Groups[1].Value;
                            constraint = constraint.Substring(0, optmatch.Groups[1].Index);
                        }
                        value = value.Substring(0, idx);
                    }
                    Tuple<string, Type> tx = def.Parameters.FirstOrDefault(a => a.Item1 == value);
                    if (tx == null)
                        throw new Exception("Unable to find parameter '" + value + "' in method with route : " + def.RestAttribute.Route);
                    p.Name = tx.Item1;
                    dynamic par = parameters?[def.Parameters.IndexOf(tx)];
                    if (par == null && optional)
                        p.Value = string.Empty;
                    else if (constraint != null)
                    {
                        ParameterType type = ParameterType.InstanceTypes.FirstOrDefault(a => a.Name == constraint);
                        if (type==null)
                            throw new Exception("Invalid Contraint: " + constraint);
                        ParameterResult res = type.Convert((object) par, ops, p.Name, optional);
                        if (!res.Success)
                            throw new Exception(res.Error);
                        p.Value = res.Value;
                    }
                    else
                    {
                        if (par is DateTime)
                        {
                            p.Value = ((DateTime) par).ToString("o",CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            TypeConverter c = TypeDescriptor.GetConverter(par.GetType());
                            if (c.CanConvertTo(typeof(string)))
                                p.Value = c.ConvertToInvariantString(par);
                            else
                                throw new Exception("Unable to convert parameter '" + value + "' to string");
                        }
                    }
                    pars.Add(p);
                }
            }
            foreach (Parameter p in pars)
            {
                // URLEncode shouldn't cause any issues, but it will hopefully prevent some
                path = path.Replace("{" + p.Original + "}", WebUtility.UrlEncode(p.Value));
            }
            List<string> names = pars.Select(a => a.Name).ToList();
            List<int> bodyitems = def.Parameters.Where(a => !names.Contains(a.Item1)).Select(a => def.Parameters.IndexOf(a)).ToList();
            object body = null;
            bool processbody = bodyitems.Count > 0;

            if (bodyitems.Count == 1)
            {
                Type t= parameters[bodyitems[0]].GetType();
                if (!(t.IsValueType || typeof(string) == t))
                {
                    body = parameters[bodyitems[0]];
                    processbody = false;
                }
            }
            Request r = new Request();
            if (processbody)
            {
                StringBuilder bld=new StringBuilder();
                foreach (int p in bodyitems)
                {
                    if (bld.Length > 0)
                        bld.Append("&");
                    bld.Append(WebUtility.UrlEncode(def.Parameters[p].Item1));
                    bld.Append("=");
                    TypeConverter c = TypeDescriptor.GetConverter(parameters[p].GetType());
                    if (c.CanConvertTo(typeof(string)))
                        bld.Append(WebUtility.UrlEncode(c.ConvertToInvariantString(parameters[p])));
                    else
                        throw new Exception("Unable to convert parameter '" + def.Parameters[p].Item1 + "' to string");
                }
                body = bld.ToString();
                r.IsWWWFormUrlencoded = true;
            }
            r.Path = path;
            r.BaseUri = new Uri(def.BasePath);
            r.BodyObject = body;
            r.Method = def.RestAttribute.Verb.ToHttpMethod();
            r.ReturnType = def.ReturnType;
            return r;
        }
    }
}
