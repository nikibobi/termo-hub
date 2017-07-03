using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TermoHub.Formatters
{
    public class CsvOutputFormatter : TextOutputFormatter
    {
        public string Separator { get; private set; }

        public CsvOutputFormatter(string separator = ",")
        {
            SupportedMediaTypes.Add("text/csv");
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            Separator = separator;
        }

        protected override bool CanWriteType(Type type)
        {
            if (IsEnumerable(type))
            {
                return base.CanWriteType(type);
            }
            return false;
        }

        private bool IsEnumerable(Type type)
        {
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return true;
            }
            foreach (Type t in type.GetInterfaces())
            {
                if (IsEnumerable(t))
                {
                    return true;
                }
            }
            return false;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            HttpResponse response = context.HttpContext.Response;
            var buffer = new StringBuilder();
            if (context.Object is IEnumerable<object> enumerable)
            {
                Type element = enumerable.GetType().GenericTypeArguments[0];
                PropertyInfo[] properties = element.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                string header = MakeLine(properties.Select(p => p.Name));
                buffer.AppendLine(header);
                foreach (object obj in enumerable)
                {
                    string line = MakeLine(properties.Select(p => p.GetValue(obj)));
                    buffer.AppendLine(line);
                }
            }
            return response.WriteAsync(buffer.ToString());
        }

        private string MakeLine(IEnumerable<object> values) => string.Join(Separator, values);
    }
}
