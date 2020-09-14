using Punfai.Report.ReportTypes;
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
    public class TextFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new[] { typeof(TextReportType) }; } }
        public string LastError { get; private set; }

        public async Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            StringBuilder errors = new StringBuilder();
            var blockkeys = stuffing.Keys.Where(a => a.StartsWith("rows")).OrderBy(a => a).ToArray();
            if (blockkeys.Count() == 0)
            {
                //A text file data['rows'] to be set to an enumerable list of rows
                return false;
            }
            // read any settings passed in
            var settings = readSettings(stuffing);
            // 1
            // 2. the template has N rows (after removing #comments)
            //      row 1: block 1 to be the template matched with blockkey 1 IList contents
            //      row 2: block 2 to be the template matched with blockkey 2 IList contents
            //      row N: ...
            //    read IList<IDictionary<string, object>> stored in stuffing["rows"]
            UTF8Encoding utf8NoPreamble = new UTF8Encoding(false);
            UTF8Encoding utf8WithPreamble = new UTF8Encoding(true);
            Encoding utf8 = settings.BOMPreamble ? utf8WithPreamble : utf8NoPreamble;
            StreamWriter writer = new StreamWriter(output, utf8);
            foreach (var section in t.SectionNames)
            {
                string fulltemplate = t.GetSectionText(section).Trim();
                if (string.IsNullOrEmpty(fulltemplate))
                {
                    // 1. 
                    dynamic rows = stuffing.Values.FirstOrDefault(a => a is IList<object>);
                    if (rows == null) continue;
                    foreach (var row in rows)
                    {
                        var list = row as IList<object>;
                        if (list == null)
                        {
                            // row is some kind of object. Pretend the coder gave us a string representaion of the whole row.
                            await writer.WriteAsync(row.ToString());
                        }
                        else
                        {
                            // row is a list of items, concat them
                            StringBuilder s = new StringBuilder();
                            foreach (var item in row)
                            {
                                addField(item, s);
                            }
                            await writer.WriteAsync(s.ToString());
                        }
                        if (settings.NewLine != string.Empty)
                            await writer.WriteAsync(settings.NewLine);
                    }
                }
                else
                {
                    // 2. 
                    Debug.WriteLine("TextFiller.Fill", "custom text template");
                    string[] lines = fulltemplate.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> linesClean = new List<string>();
                    foreach (var line in lines)
                        if (!line.StartsWith("#") && !line.StartsWith("//") && !line.StartsWith("--"))
                            linesClean.Add(line);

                    if (linesClean.Count != blockkeys.Count())
                    {
                        Debug.WriteLine("template line count does not match data row block count");
                        return false;
                    }
                    // loop over blocks
                    for (int i = 0; i < linesClean.Count; i++)
                    {
                        dynamic rows = stuffing[blockkeys[i]];
                        foreach (dynamic row in rows)
                        {
                            StringBuilder srow = new StringBuilder(linesClean[i]);
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
                            await writer.WriteAsync(srow.ToString());
                            if (settings.NewLine != string.Empty)
                                await writer.WriteAsync(settings.NewLine);
                        }
                    }
                }

            }
            // the caller owns the stream, they have to close it.
            // so don't go using() or closing the streamwriter
            await writer.FlushAsync();
            LastError = errors.ToString();
            return true;
        }
        private TextSettings readSettings(IDictionary<string, object> stuffing)
        {
            string newLine;
            if (stuffing.ContainsKey("newLine"))
            {
                var a = stuffing["newLine"] as string;
                if (a == null) newLine = string.Empty;
                else newLine = a;
            }
            else
                newLine = "\r\n";
            bool bomPreamble;
            if (stuffing.ContainsKey("bom"))
            {
                var a = stuffing["bom"] as bool?;
                if (a.HasValue) bomPreamble = a.Value;
                else bomPreamble = false;
            }
            else
                bomPreamble = false;

            var settings = new TextSettings()
            {
                NewLine = newLine,
                BOMPreamble = bomPreamble
            };
            return settings;
        }
        private void addField(object oval, StringBuilder s)
        {
            if (oval is string)
                addString((string)oval, s);
            else
                addObject(oval, s);
        }
        private void addObject(object oval, StringBuilder s)
        {
            string sval;
            if (oval == null) sval = "";
            else sval = oval.ToString();
            s.Append(sval);
        }
        private void addString(string sval, StringBuilder s)
        {
            s.Append(sval);
        }
        private class TextSettings
        {
            public string NewLine { get; set; }
            public bool BOMPreamble { get; set; }
        }
    }
}
