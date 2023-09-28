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
                var options = FieldFormatOptions.ParseFormatting(format, key);
                if (string.IsNullOrWhiteSpace(svalue) && options.Required)
                    errors.AppendLine(string.Format("Required field {0} not specified", key));
                string formattedString = options.ApplyFormat(svalue, errors);
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


    }
}
