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

namespace Punfai.Report.OfficeOpenXml.Fillers
{
    public class WordFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new[] { typeof(WordReportType) }; } }

        public string LastError { get; private set; }

        public Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            using (Stream docstream = new MemoryStream())
            {
                byte[] templateBytes = t.GetTemplateBytes();
                docstream.Write(templateBytes, 0, templateBytes.Length);
                docstream.Flush();
                docstream.Position = 0;
                using (WordprocessingDocument doc = WordprocessingDocument.Open(docstream, true))
                {
                    // main document part
                    if (doc.MainDocumentPart != null) doPart(doc.MainDocumentPart, stuffing);

                    var headers = doc.MainDocumentPart.HeaderParts.ToList();
                    headers.ForEach(p =>
                    {
                        doPart(p, stuffing);
                    });
                    var footers = doc.MainDocumentPart.FooterParts.ToList();
                    footers.ForEach(p =>
                    {
                        doPart(p, stuffing);
                    });
                }
                docstream.Position = 0;
                XmlTemplateTool.CopyStream(docstream, output);
            }
            return Task.FromResult<bool>(true);
        }
        private void doPart(OpenXmlPart part, IDictionary<string, dynamic> stuffing)
        {
            try
            {
                XDocument xdoc1 = part.GetXDocument();
                foreach (KeyValuePair<string, dynamic> pair in stuffing)
                    XmlTemplateTool.ReplaceKey(xdoc1.Root, pair.Key, pair.Value);
                part.PutXDocument(xdoc1);
            }
            catch (Exception ex) { Console.WriteLine("XmlFiller.Fill", "bad excel part", ex); }
        }
    }

}
