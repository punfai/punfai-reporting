using Punfai.Report.ReportTypes;
using Punfai.Report.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Punfai.Report.Fillers
{
    public class XmlFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new[] { typeof(XmlReportType) }; } }
        public string LastError { get; private set; }

        public async Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            // TODO: make this more asyncy
            XmlWriter writer = XmlWriter.Create(output, new XmlWriterSettings() { Encoding = new UTF8Encoding(false), Indent = true, Async = true });
            // should only be one section
            foreach (var section in t.SectionNames)
            {
                XDocument doc;
                try
                {
                    var txt = t.GetSectionText(section);
                    doc = XDocument.Parse(txt);
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    return false;
                }
                foreach (KeyValuePair<string, dynamic> pair in stuffing)
                {
                    XmlTemplateTool.ReplaceKey(doc.Root, pair.Key, pair.Value);
                }
                doc.WriteTo(writer);
            }
            await writer.FlushAsync();
            LastError = null;
            return true;
        }
    }
}
