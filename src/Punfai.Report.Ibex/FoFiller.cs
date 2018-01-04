using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using System.Text.RegularExpressions;
using Punfai.Report.Interfaces;
using Punfai.Report.OfficeOpenXml.Utils;
using Punfai.Report.Utils;
using System.Threading.Tasks;
using Punfai.Report.OfficeOpenXml.ReportTypes;

namespace Punfai.Report.Ibex
{
    public class FoFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new[] { typeof(FoReportType) }; } }

        public Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            // TODO: make this more asyncy
            XmlWriter writer = XmlWriter.Create(output, new XmlWriterSettings() { Encoding = UTF8Encoding.UTF8, Indent = true, Async = true });
            // should only be one section
            foreach (var section in t.SectionNames)
            {
                XDocument doc;
                try { doc = XDocument.Parse(t.GetSectionText(section)); }
                catch (Exception) { continue; }
                foreach (KeyValuePair<string, dynamic> pair in stuffing)
                {
                    XmlTemplateTool.ReplaceKey(doc.Root, pair.Key, pair.Value);
                }
                // doc is now a filled out FO
            }
            await writer.FlushAsync();
            return true;
        }
    }

}
