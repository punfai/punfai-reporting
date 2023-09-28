using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Punfai.Report.Utils;

/// <summary>
/// Please explain
/// </summary>
public class FieldFormatOptions
{
    public string FieldName { get; set; }
    public string DataType { get; set; }
    public bool Required { get; set; }
    public string Format { get; set; }
    public char BlankFiller { get; set; }
    public int? FixedLength { get; set; }
    public bool AlignLeft { get; set; }

    #region perform formatting
    public string ApplyFormat(string svalue, StringBuilder errors)
    {
        string formatted;
        switch (DataType)
        {
            case "s": formatted = FormatString(svalue, errors); break;
            case "a": formatted = FormatAlpha(svalue, errors); break;
            case "an": formatted = FormatAlphaNum(svalue, errors); break;
            case "n": formatted = FormatNum(svalue, errors); break;
            case "dt": formatted = FormatDate(svalue, errors); break;
            default: formatted = svalue; break;
        }
        return ApplyFixedLength(formatted, errors);
    }
    public string FormatString(string svalue, StringBuilder errors)
    {
        return svalue;
    }
    public string FormatAlpha(string svalue, StringBuilder errors)
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
    public string FormatAlphaNum(string svalue, StringBuilder errors)
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
    public string FormatNum(string svalue, StringBuilder errors)
    {
        StringBuilder s = new StringBuilder();
        decimal dval;
        if (!decimal.TryParse(svalue, out dval))
        {
            dval = 0;
            errors.AppendLine($"Not a number: '{svalue}'");
        }
        if (!string.IsNullOrEmpty(Format))
            return dval.ToString(Format);
        else
            return dval.ToString();
    }
    public string FormatDate(string svalue, StringBuilder errors)
    {
        DateTime d;
        if (DateTime.TryParse(svalue, out d))
        {
            if (!string.IsNullOrEmpty(Format))
                return d.ToString(Format);
            else
                return d.ToString("yyyy-MM-dd");
        }
        return string.Empty;
    }

    public string ApplyFixedLength(string svalue, StringBuilder errors)
    {
        if (FixedLength.HasValue)
        {
            if (svalue.Length > FixedLength.Value)
            {
                errors.AppendLine(string.Format("Truncating {0} fixed length {1} value too long {2}", FieldName, FixedLength.Value, svalue));
                return svalue.Substring(0, FixedLength.Value);
            }
            else if (svalue.Length == FixedLength)
            {
                return svalue;
            }
            StringBuilder s = new StringBuilder();
            bool right = !AlignLeft;
            if (right)
            {
                // right aligned
                while (s.Length + svalue.Length < FixedLength.Value)
                    s.Append(BlankFiller);
                if (!string.IsNullOrEmpty(svalue))
                    s.Append(svalue);
            }
            else
            {
                // left aligned
                if (!string.IsNullOrEmpty(svalue))
                    s.Append(svalue);
                while (s.Length < FixedLength.Value)
                    s.Append(BlankFiller);
            }
            return s.ToString();
        }
        else
        {
            return svalue;
        }
    }
    #endregion

    public static FieldFormatOptions ParseFormatting(string format, string fieldName)
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
}
