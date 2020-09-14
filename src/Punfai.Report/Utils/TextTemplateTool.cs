using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Punfai.Report.Utils
{
    public class TextTemplateTool
    {
        static TextTemplateTool()
        {
        }
        #region public static methods
        public static void ReplaceKey(StringBuilder s, string key, string svalue, StringBuilder errors)
        {
            string placeHolderFormatted = "{{" + key + ":";
            string placeHolder = "{{" + key + "}}";
            if (svalue == null) svalue = "";
            string ss = s.ToString();
            int index = ss.IndexOf(placeHolderFormatted);
            while (index > -1)
            {
                // find format and full custom placeholder
                int endIndex = ss.IndexOf("}}", index);
                string customPlaceHolder = ss.Substring(index, endIndex - index + 2);
                int formatStart = customPlaceHolder.IndexOf(':') + 1;
                int formatLength = customPlaceHolder.Length - formatStart - 2;
                string format = customPlaceHolder.Substring(formatStart, formatLength);
                var options = parseFormatting(format, key);
                if (string.IsNullOrWhiteSpace(svalue) && options.Required)
                    errors.AppendLine(string.Format("Required field {0} not specified", key));
                string formattedString = applyFormat(svalue, options, errors);
                s.Replace(customPlaceHolder, formattedString);
                ss = s.ToString();
                index = ss.IndexOf(placeHolderFormatted);
            }
            // unformatted placeholder replacement
            s.Replace(placeHolder, svalue);
        }
        public static void ReplaceKeys(StringBuilder s, IDictionary<string, object> dic, StringBuilder errors, string prefix = null)
        {
            foreach (var pair in dic)
            {
                string key;
                if (!string.IsNullOrEmpty(prefix)) key = string.Format("{0}.{1}", prefix, pair.Key);
                else key = pair.Key;
                if (pair.Value == null)
                    ReplaceKey(s, key, "", errors);
                else
                    ReplaceKey(s, key, pair.Value.ToString(), errors);
            }
        }
        public static void ReplaceKeys(StringBuilder s, IDictionary<object, object> dic, StringBuilder errors, string prefix = null)
        {
            foreach (var pair in dic)
            {
                string key;
                if (!string.IsNullOrEmpty(prefix)) key = string.Format("{0}.{1}", prefix, pair.Key);
                else key = pair.Key.ToString();
                if (pair.Value == null)
                    ReplaceKey(s, key, "", errors);
                else
                    ReplaceKey(s, key, pair.Value.ToString(), errors);
            }
        }
        #endregion

        #region perform formatting
        private static string applyFormat(string svalue, FieldFormatOptions options, StringBuilder errors)
        {
            string formatted;
            switch (options.DataType)
            {
                case "s": formatted = formatString(svalue, options, errors); break;
                case "a": formatted = formatAlpha(svalue, options, errors); break;
                case "an": formatted = formatAlphaNum(svalue, options, errors); break;
                case "n": formatted = formatNum(svalue, options, errors); break;
                case "dt": formatted = formatDate(svalue, options, errors); break;
                default: formatted = svalue; break;
            }
            return applyFixedLength(formatted, options, errors);
        }
        private static string formatString(string svalue, FieldFormatOptions options, StringBuilder errors)
        {
            return svalue;
        }
        private static string formatAlpha(string svalue, FieldFormatOptions options, StringBuilder errors)
        {
            StringBuilder s = new StringBuilder();
            foreach (char c in svalue)
            {
                if (char.IsLetter(c))
                    s.Append(c);
                else
                    s.Append(' ');
            }
            return s.ToString().Trim();
        }
        private static string formatAlphaNum(string svalue, FieldFormatOptions options, StringBuilder errors)
        {
            StringBuilder s = new StringBuilder();
            foreach (char c in svalue)
            {
                if (char.IsLetterOrDigit(c))
                    s.Append(c);
                else
                    s.Append(' ');
            }
            return s.ToString().Trim();
        }
        private static string formatNum(string svalue, FieldFormatOptions options, StringBuilder errors)
        {
            StringBuilder s = new StringBuilder();
            decimal dval;
            if (!decimal.TryParse(svalue, out dval))
            {
                dval = 0;
                errors.AppendLine($"Not a number: '{svalue}'");
            }
            if (!string.IsNullOrEmpty(options.Format))
                return dval.ToString(options.Format);
            else
                return dval.ToString();
        }
        private static string formatDate(string svalue, FieldFormatOptions options, StringBuilder errors)
        {
            DateTime d;
            if (DateTime.TryParse(svalue, out d))
            {
                if (!string.IsNullOrEmpty(options.Format))
                    return d.ToString(options.Format);
                else
                    return d.ToString("yyyy-MM-dd");
            }
            return string.Empty;
        }

        private static string applyFixedLength(string svalue, FieldFormatOptions options, StringBuilder errors)
        {
            if (options.FixedLength.HasValue)
            {
                if (svalue.Length > options.FixedLength.Value)
                {
                    errors.AppendLine(string.Format("Truncating {0} fixed length {1} value too long {2}", options.FieldName, options.FixedLength.Value, svalue));
                    return svalue.Substring(0, options.FixedLength.Value);
                }
                else if (svalue.Length == options.FixedLength)
                {
                    return svalue;
                }
                StringBuilder s = new StringBuilder();
                bool right = !options.AlignLeft;
                if (right)
                {
                    // right aligned
                    while (s.Length + svalue.Length < options.FixedLength.Value)
                        s.Append(options.BlankFiller);
                    if (!string.IsNullOrEmpty(svalue))
                        s.Append(svalue);
                }
                else
                {
                    // left aligned
                    if (!string.IsNullOrEmpty(svalue))
                        s.Append(svalue);
                    while (s.Length < options.FixedLength.Value)
                        s.Append(options.BlankFiller);
                }
                return s.ToString();
            }
            else
            {
                return svalue;
            }
        }
        #endregion

        #region format setup
        private class FieldFormatOptions
        {
            public string FieldName { get; set; }
            public string DataType { get; set; }
            public bool Required { get; set; }
            public string Format { get; set; }
            public char BlankFiller { get; set; }
            public int? FixedLength { get; set; }
            public bool AlignLeft { get; set; }
        }
        private static FieldFormatOptions parseFormatting(string format, string fieldName)
        {
            var formats0 = format.Trim().Split(new[] { ',' }, 3);
            var formats = formats0.Select(a => a.Trim()).ToArray();
            string dataType;
            string dotnetFormatString;
            bool alignLeft = true;
            if (formats.Length > 0)
            {
                if (formats[0].Contains("["))
                {
                    int f1 = formats[0].IndexOf('[');
                    int f2 = formats[0].IndexOf(']', f1);
                    if (f2 == -1) f2 = formats[0].Length - 1;
                    dataType = formats[0].Substring(0, f1);
                    dotnetFormatString = formats[0].Substring(f1 + 1, f2 - f1 - 1);
                }
                else
                {
                    dataType = formats[0];
                    dotnetFormatString = null;
                }
                // set alignment default
                alignLeft = !dataType.StartsWith("n");
                // if alignment specified
                var lastChar = dataType[dataType.Length - 1];
                if (lastChar == 'r' || lastChar == 'l')
                {
                    if (lastChar == 'r') alignLeft = false;
                    else if (lastChar == 'l') alignLeft = true;
                    // remove the alignment option from the data type
                    dataType = dataType.Substring(0, dataType.Length - 1);
                }
            }
            else
            {
                dataType = "s";
                dotnetFormatString = null;
            }
            bool required;
            char blankFiller = dataType == "n" || dataType == "dt" ? '0' : ' ';
            if (formats.Length > 1)
            {
                required = formats[1] == "m";
                int blankf1 = formats[1].IndexOf('[');
                if (blankf1 > 0)
                {
                    blankFiller = formats[1][blankf1 + 1];
                }
            }
            else
            {
                required = false;
            }
            int? fixedLength;
            if (formats.Length > 2)
            {
                int fx;
                if (int.TryParse(formats[2], out fx))
                    fixedLength = fx;
                else
                    fixedLength = null;
            }
            else
            {
                fixedLength = null;
            }

            var options = new FieldFormatOptions()
            {
                FieldName = fieldName,
                DataType = dataType,
                Format = dotnetFormatString,
                Required = required,
                BlankFiller = blankFiller,
                FixedLength = fixedLength,
                AlignLeft = alignLeft
            };
            return options;
        }
        #endregion
    }
}
