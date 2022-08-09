using System;
using System.Collections.Generic;

namespace Nancy.Rest.Client.Helpers
{
    internal class MethodDefinition
    {
        public Type ReturnType { get; set; }
        public List<Tuple<string,Type>> Parameters { get; set; }

        public Annotations.Attributes.Rest RestAttribute { get; set; }

        public string BasePath { get; set; }
    }
}
