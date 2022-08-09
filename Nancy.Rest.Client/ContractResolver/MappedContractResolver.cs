using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Nancy.Rest.Client.ContractResolver
{
    public class MappedContractResolver : DefaultContractResolver
    {
        private Dictionary<Type, Type> _mappings;
        public MappedContractResolver(Dictionary<Type, Type> mappings)
        {
            _mappings = mappings;
        }
        public override JsonContract ResolveContract(Type type)
        {
            type = ReplaceType(type);
            return base.ResolveContract(type);
        }

        private Type ReplaceType(Type t)
        {
            if (t.GenericTypeArguments.Length > 0)
            {
                for (int x = 0; x < t.GenericTypeArguments.Length; x++)
                {
                    t.GenericTypeArguments[x] = ReplaceType(t.GenericTypeArguments[x]);
                }
            }
            if (_mappings.ContainsKey(t))
                t = _mappings[t];
            return t;
        }
    }
}
