using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace LWFStatsWeb.Formatters
{
    /// <summary>
    /// Copied from
    /// https://github.com/damienbod/AspNetCoreCsvImportExport
    /// Original code taken from
    /// http://www.tugberkugurlu.com/archive/creating-custom-csvmediatypeformatter-in-asp-net-web-api-for-comma-separated-values-csv-format
    /// Adapted for ASP.NET Core and uses ; instead of , for delimiters
    /// </summary>
    public class CsvOutputFormatter : OutputFormatter
    {
        private readonly CsvFormatterOptions _options;

        public string ContentType { get; private set; }

        public CsvOutputFormatter(CsvFormatterOptions csvFormatterOptions)
        {
            ContentType = "text/csv";
            SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/csv"));

            if (csvFormatterOptions == null)
            {
                throw new ArgumentNullException(nameof(csvFormatterOptions));
            }

            _options = csvFormatterOptions;

            //SupportedEncodings.Add(Encoding.GetEncoding("utf-8"));
        }

        protected override bool CanWriteType(Type type)
        {

            if (type == null)
                throw new ArgumentNullException("type");

            return isTypeOfIEnumerable(type);
        }

        private bool isTypeOfIEnumerable(Type type)
        {

            foreach (Type interfaceType in type.GetInterfaces())
            {

                if (interfaceType == typeof(IList))
                    return true;
            }

            return false;
        }

        private Type GetEnumerableType(Type type)
        {
            foreach (Type intType in type.GetInterfaces())
            {
                if (intType.IsConstructedGenericType
                    && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return intType.GetGenericArguments()[0];
                }
            }
            return null;
        }

        public async override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var response = context.HttpContext.Response;

            Type type = context.Object.GetType();
            Type itemType = GetEnumerableType(type);

            StringWriter _stringWriter = new StringWriter();

            if (_options.UseSingleLineHeaderInCsv)
            {
                _stringWriter.WriteLine(
                    string.Join<string>(
                        _options.CsvDelimiter, itemType.GetProperties().Select(x => x.Name)
                    )
                );
            }

            var trimChars = _options.CsvDelimiter.ToCharArray();

            foreach (var obj in (IEnumerable<object>)context.Object)
            {

                var vals = obj.GetType().GetProperties().Select(
                    pi => new {
                        Value = pi.GetValue(obj, null)
                    }
                );

                string _valueLine = string.Empty;

                foreach (var val in vals)
                {

                    if (val.Value != null)
                    {
                        string _val = String.Empty;

                        if (val.Value is double)
                            _val = ((double)val.Value).ToString(System.Globalization.CultureInfo.InvariantCulture);
                        else if (val.Value is DateTime)
                            _val = ((DateTime)val.Value).ToString("yyyy-MM-dd");
                        else
                            _val = val.Value.ToString();

                        //Check if quotes needed
                        if (_val.Contains(",") || _val.Contains("\r") || _val.Contains("\n") || _val.Contains("\""))
                        {
                            //Double quote quotes
                            _val = _val.Replace("\"", "\"\"");

                            //Put value inside quotes
                            _val = string.Concat("\"", _val, "\"");
                        }

                        _valueLine = string.Concat(_valueLine, _val, _options.CsvDelimiter);
                    }
                    else
                    {
                        _valueLine = string.Concat(_valueLine, string.Empty, _options.CsvDelimiter);
                    }
                }

                _stringWriter.WriteLine(_valueLine.TrimEnd(trimChars));
            }

            var streamWriter = new StreamWriter(response.Body);
            await streamWriter.WriteAsync(_stringWriter.ToString());
            await streamWriter.FlushAsync();
        }
    }
}
