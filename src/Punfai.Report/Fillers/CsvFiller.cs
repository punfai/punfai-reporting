﻿using Punfai.Report.ReportTypes;
using Punfai.Report.Template;
using Punfai.Report.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Punfai.Report.Fillers
{
    public class CsvFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new[] { typeof(CsvReportType) }; } }
        public string LastError { get; private set; }

        public async Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            StringBuilder errors = new StringBuilder();
            if (!stuffing.ContainsKey("rows"))
            {
                //A CSV report needs data['rows'] to be set to an enumerable list of rows
                return false;
            }
            // read any settings passed in
            bool quoteStrings;
            readSettings(stuffing, out quoteStrings);
            // does it even need a template? no not really.
            // 1. the template is null or blank
            // find the first IList<IList<object>> and iterate
            // 2. the template has one or two rows (after removing #comments)
            //      row 1: header (optional)
            //      row 2 (or 1 if no header): row template
            //    read IList<IDictionary<string, object>> stored in stuffing["rows"]
            if (t == null) t = new PlainTextTemplate(new byte[] { });
            StreamWriter writer = new StreamWriter(output, new UTF8Encoding(false));
            foreach (var section in t.SectionNames)
            {
                string fulltemplate = t.GetSectionText(section).Trim();
                if (string.IsNullOrEmpty(fulltemplate))
                {
                    // 1. 
                    dynamic rows = stuffing["rows"];
                    if (rows == null) rows = stuffing.Values.FirstOrDefault(); // out with it?
                    //dynamic rows = stuffing.Values.FirstOrDefault(a => a is IList<object>);
                    if (rows == null) continue;
                    try
                    {
                        foreach (dynamic row in rows)
                        {
                            // row is a list of items, CS them!
                            StringBuilder s = new StringBuilder();
                            foreach (var item in row)
                            {
                                addField(item, s, quoteStrings);
                            }
                            await writer.WriteLineAsync(s.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("untemplated csv error. is there an enumerable entry in the dictionary?");
                        Debug.WriteLine(ex.Message);
                        continue;
                    }
                }
                else
                {
                    // 2. 
                    Debug.WriteLine("CsvFiller.Fill", "custom csv template");
                    string[] lines = fulltemplate.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> linesClean = new List<string>();
                    foreach (var line in lines)
                        if (!line.StartsWith("#") && !line.StartsWith("//") && !line.StartsWith("--"))
                            linesClean.Add(line.TrimEnd());
                    string headerTemplate = null;
                    string rowTemplate = null;
                    if (linesClean.Count == 1) rowTemplate = linesClean[0];
                    else
                    {
                        headerTemplate = linesClean[0];
                        rowTemplate = linesClean[1];
                    }
                    dynamic rows = stuffing["rows"];
                    if (headerTemplate != null)
                    {
                        await writer.WriteLineAsync(headerTemplate);
                    }
                    foreach (dynamic row in rows)
                    {
                        StringBuilder srow = new StringBuilder(rowTemplate);
                        IDictionary<string, object> dic = row as IDictionary<string, object>;
                        if (dic != null)
                        {
                            TextTemplateTool.ReplaceKeys(srow, dic, errors);
                        }
                        else
                        {
                            IDictionary<object, object> pdic = row as IDictionary<object, object>;
                            if (pdic != null)
                            {
                                TextTemplateTool.ReplaceKeys(srow, pdic, errors);
                            }
                            else
                            {
                                Debug.WriteLine("row is neither IDictionary<string,object> or IDictionary<object,object>");
                                continue;
                            }
                        }
                        await writer.WriteLineAsync(srow.ToString());
                    }
                }

            }
            // the caller owns the stream, they have to close it.
            // so don't go using() or closing the streamwriter - why the hell not? Because they probably writing to a memorystream and closing it dissappears it.
            await writer.FlushAsync();
            LastError = errors.ToString();
            return true;
        }
        private void readSettings(IDictionary<string, object> stuffing, out bool quoteStrings)
        {
            quoteStrings = true;
            if (stuffing.ContainsKey("quoteStrings"))
            {
                var a = stuffing["quoteStrings"] as bool?;
                if (a.HasValue) quoteStrings = a.Value;
            }
        }
        private void addField(object oval, StringBuilder s, bool quotes)
        {
            if (oval is string)
                addString((string)oval, s, quotes);
            else
                addObject(oval, s);
        }
        private void addObject(object oval, StringBuilder s)
        {
            string sval;
            if (oval == null) sval = "";
            else sval = oval.ToString();
            s.Append(sval.Replace(",", "").Replace("$", "")); // switch stripping? useful for string numbers {number:c2}
            s.Append(',');
        }
        private void addString(string sval, StringBuilder s, bool quotes)
        {
            if (quotes) s.Append('"');
            s.Append(sval.Replace(",","").Replace("$","")); // switch stripping? useful for string numbers {number:c2}
            if (quotes) s.Append('"');
            s.Append(',');
        }
        private string pad(object oval, int len, int inull)
        {
            string sval;
            int ival;
            if (oval == null || oval.ToString() == String.Empty) ival = inull;
            else if (oval is int) ival = (int)oval;
            else if (oval is long) ival = Convert.ToInt32(oval);
            else if (!int.TryParse(oval.ToString(), out ival)) ival = inull;
            string sformat = "";
            for (int i = 0; i < len; i++)
                sformat += "0";
            sval = ival.ToString(sformat);
            return sval;
        }
    }
}
