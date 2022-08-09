using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nancy.Rest.Client.Helpers
{

    internal class ParameterType
    {
        public static List<ParameterType> InstanceTypes = new List<ParameterType>()
        {
            new ParameterType("int", new[] {typeof(int), typeof(int?)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(int o) => new ParameterResult {Success = true, Value = o.ToString(CultureInfo.InvariantCulture)};
                if (obj is int?)
                    return ProcessNullable((int?) obj, pars, value, optional, Parser);
                return new ParameterResult {Success = false, Error = "Parameter: " + value + " is not an int"};

            }),
            new ParameterType("long", new[] {typeof(long), typeof(long?)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(long o) => new ParameterResult {Success = true, Value = o.ToString(CultureInfo.InvariantCulture)};
                if (obj is long?)
                    return ProcessNullable((long?) obj, pars, value, optional, Parser);
                return new ParameterResult {Success = false, Error = "Parameter: " + value + " is not a long"};
            }),
            new ParameterType("decimal", new[] {typeof(decimal), typeof(decimal?)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(decimal o) => new ParameterResult {Success = true, Value = o.ToString(CultureInfo.InvariantCulture)};
                if (obj is decimal?)
                    return ProcessNullable((decimal?) obj, pars, value, optional, Parser);
                return new ParameterResult {Success = false, Error = "Parameter: " + value + " is not a decimal"};
            }),
            new ParameterType("bool", new[] {typeof(bool), typeof(bool?)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(bool o) => new ParameterResult {Success = true, Value = o.ToString(CultureInfo.InvariantCulture)};
                if (obj is bool?)
                    return ProcessNullable((bool?) obj, pars, value, optional, Parser);
                return new ParameterResult {Success = false, Error = "Parameter: " + value + " is not a bool"};
            }),
            new ParameterType("guid", new[] {typeof(Guid), typeof(Guid?)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(Guid o) => new ParameterResult {Success = true, Value = o.ToString()};
                if (obj is Guid?)
                    return ProcessNullable((Guid?) obj, pars, value, optional, Parser);
                return new ParameterResult {Success = false, Error = "Parameter: " + value + " is not a Guid"};
            }),
            new ParameterType("alpha", new[] {typeof(string)}, (obj, pars, value, optional) =>
            {
                ParameterResult r = new ParameterResult();
                bool valid = false;
                if (obj == null && optional)
                    valid = true;
                else if(obj != null)
                {
                    valid = ((string) obj).All(Char.IsLetter);
                }
                if (!valid)
                {
                    r.Success = false;
                    r.Error = "Parameter: " + value + " is not alpha";
                }
                else
                {

                    r.Success = true;
                    r.Value = (string) obj;
                }
                return r;
            }),
            new ParameterType("datetime", new[] {typeof(DateTime), typeof(DateTime?)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(DateTime o) => new ParameterResult {Success = true, Value = !string.IsNullOrEmpty(pars) ? o.ToString(pars, CultureInfo.InvariantCulture) : o.ToString(CultureInfo.InvariantCulture)};
                if (obj is DateTime?)
                    return ProcessNullable((DateTime?) obj, pars, value, optional, Parser);
                return new ParameterResult {Success = false, Error = "Parameter: " + value + " is not a DateTime"};
            }),
            new ParameterType("min", new[] {typeof(int), typeof(int?)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(int o)
                {
                    int minimal = 0;
                    if (!int.TryParse(pars, out minimal))
                        return new ParameterResult {Success = false, Error = "Not valid minimiun value in parameter: " + value};
                    if ((int) obj < minimal)
                        return new ParameterResult {Success = false, Error = "The minimal value in parameter '" + value + "' is '" + minimal + "' the value posted is '" + (int) obj + "'"};
                    return new ParameterResult {Success = true, Value = o.ToString(CultureInfo.InvariantCulture)};
                }

                if (obj is int?)
                    return ProcessNullable((int?) obj, pars, value, optional, Parser);
                return new ParameterResult {Success = false, Error = "Parameter: " + value + " is not an int"};
            }),
            new ParameterType("max", new[] {typeof(int), typeof(int?)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(int o)
                {
                    int maximun = 0;
                    if (!int.TryParse(pars, out maximun))
                        return new ParameterResult {Success = false, Error = "Not valid maximun value in parameter: " + value};
                    if ((int) obj > maximun)
                        return new ParameterResult {Success = false, Error = "The maximun value in parameter '" + value + "' is '" + maximun + "' the value posted is '" + ((int) obj) + "'"};
                    return new ParameterResult {Success = true, Value = o.ToString(CultureInfo.InvariantCulture)};
                }

                if (obj is int?)
                    return ProcessNullable((int?) obj, pars, value, optional, Parser);
                return new ParameterResult {Success = false, Error = "Parameter: " + value + " is not an int"};
            }),
            new ParameterType("range", new[] {typeof(int), typeof(int?)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(int o)
                {
                    string[] parts = pars.Split(',');
                    if (parts.Length != 2)
                        return new ParameterResult {Success = false, Error = "Not valid range in parameter: " + value};
                    int minimal = 0;
                    if (!int.TryParse(parts[0], out minimal))
                        return new ParameterResult {Success = false, Error = "Not valid minimiun value in parameter: " + value};

                    int maximun = 0;
                    if (!int.TryParse(parts[1], out maximun))
                        return new ParameterResult {Success = false, Error = "Not valid maximun value in parameter: " + value};
                    if ((int) obj < minimal)
                        return new ParameterResult {Success = false, Error = "The minimal value in parameter '" + value + "' is '" + minimal + "' the value posted is '" + ((int) obj) + "'"};
                    if ((int) obj > maximun)
                        return new ParameterResult {Success = false, Error = "The maximun value in parameter '" + value + "' is '" + maximun + "' the value posted is '" + ((int) obj) + "'"};
                    return new ParameterResult {Success = true, Value = o.ToString(CultureInfo.InvariantCulture)};
                }

                if (obj is int?)
                    return ProcessNullable((int?) obj, pars, value, optional, Parser);
                return new ParameterResult {Success = false, Error = "Parameter: " + value + " is not an int"};
            }),
            new ParameterType("minlength", new[] {typeof(string)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(string o)
                {
                    int minimal = 0;
                    if (!int.TryParse(pars, out minimal))
                        return new ParameterResult {Success = false, Error = "Not valid length value in parameter: " + value};
                    if (((string) obj).Length < minimal)
                        return new ParameterResult {Success = false, Error = "The minimal length in parameter '" + value + "' is '" + minimal + "' the value posted has a length of '" + ((string) obj).Length + "'"};
                    return new ParameterResult {Success = true, Value = o};
                }

                if (obj == null && optional)
                    return new ParameterResult {Success = true, Value=null};
                if (obj != null)
                    return Parser((string) obj);
                return new ParameterResult {Success = true, Value = ""};
            }),
            new ParameterType("maxlength", new[] {typeof(string)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(string o)
                {
                    int maximun = 0;
                    if (!int.TryParse(pars, out maximun))
                        return new ParameterResult {Success = false, Error = "Not valid length value in parameter: " + value};
                    if (((string) obj).Length > maximun)
                        return new ParameterResult {Success = false, Error = "The maximal length in parameter '" + value + "' is '" + maximun + "' the value posted has a length of '" + ((string) obj).Length + "'"};
                    return new ParameterResult {Success = true, Value = o};
                }

                if (obj == null && optional)
                    return new ParameterResult {Success = true, Value=null};
                if (obj != null)
                    return Parser((string) obj);
                return new ParameterResult {Success = true, Value = ""};
            }),
            new ParameterType("length", new[] {typeof(string)}, (obj, pars, value, optional) =>
            {
                ParameterResult Parser(string o)
                {
                    string[] parts = pars.Split(',');
                    if (parts.Length != 2)
                        return new ParameterResult {Success = false, Error = "Not valid range in parameter: " + value};
                    int minimal = 0;
                    if (!int.TryParse(parts[0], out minimal))
                        return new ParameterResult {Success = false, Error = "Not valid length value in parameter: " + value};
                    int maximun = 0;
                    if (!int.TryParse(parts[1], out maximun))
                        return new ParameterResult {Success = false, Error = "Not valid length value in parameter: " + value};
                    if (((string) obj).Length < minimal)
                        return new ParameterResult {Success = false, Error = "The minimal length in parameter '" + value + "' is '" + minimal + "' the value posted has a length of '" + ((string) obj).Length + "'"};
                    if (((string) obj).Length > maximun)
                        return new ParameterResult {Success = false, Error = "The maximal length in parameter '" + value + "' is '" + maximun + "' the value posted has a length of '" + ((string) obj).Length + "'"};
                    return new ParameterResult {Success = true, Value = o};
                }

                if (obj == null && optional)
                    return new ParameterResult {Success = true, Value=null};
                if (obj != null)
                    return Parser((string) obj);
                return new ParameterResult {Success = true, Value = ""};
            }),
        };

        public static ParameterResult ProcessNullable<T>(T? obj, string pars, string value, bool optional, Func<T, ParameterResult> parser) where T: struct
        {
            if (obj.HasValue)
            {
                return parser(obj.Value);
            }
            if (optional)
            {
                return new ParameterResult {Success = true, Value = null};
            }
            return new ParameterResult { Success = false, Error = "Parameter '" + value + "' expects '" + typeof(T).GenericTypeArguments[0].Name + "" };
        }       

        public ParameterType(string name, IEnumerable<Type> types, Func<dynamic, string, string, bool, ParameterResult> function)
        {
            Name = name;
            Types = types.ToList();
            Convert = function;
        }

        public bool Supports(Type t)
        {
            return Types.Contains(t);
        }

        public string Name { get; set; }
        public List<Type> Types { get; set; }

        public Func<object, string, string, bool, ParameterResult> Convert;

    }
}
