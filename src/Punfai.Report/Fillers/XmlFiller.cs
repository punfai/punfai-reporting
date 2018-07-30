using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Punfai.Report.Utils;
using System.Threading.Tasks;
using Punfai.Report.ReportTypes;

namespace Punfai.Report.Fillers
{
    public class XmlFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new[] { typeof(XmlReportType) }; } }
        public string LastError { get; private set; }

        public async Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            // TODO: make this more asyncy
            XmlWriter writer = XmlWriter.Create(output, new XmlWriterSettings() { Encoding = UTF8Encoding.UTF8, Indent = true, Async = true });
            // should only be one section
            foreach (var section in t.SectionNames)
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Parse(t.GetSectionText(section));
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    continue;
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
