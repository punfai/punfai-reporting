using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Punfai.Report.Utils;
using System.Threading.Tasks;

namespace Punfai.Report.Ibex.Netcore
{
    /// <summary>
    /// Based on / exactly like regular XmlFiller
    /// </summary>
    public class FoFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new Type[] { }; } }

        public string LastError { get; private set; }

        public async Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            // TODO: make this more asyncy
            XmlWriter fowriter = XmlWriter.Create(output, new XmlWriterSettings() { Encoding = new UTF8Encoding(false), Indent = true, Async = true });
            // should only be one section
            bool ok = false;
            foreach (var section in t.SectionNames)
            {
                XDocument doc;
                try
                {
                    string xmltext = t.GetSectionText(section);
                    //writeCharCodes(xmltext, 5);
                    if ((int)xmltext[0] == 65279)
                        doc = XDocument.Parse(xmltext.Substring(1));
                    else
                        doc = XDocument.Parse(xmltext);
                }
                catch (Exception ex)
                {
                    ok = false;
                    LastError = ex.Message;
                    continue;
                }
                foreach (KeyValuePair<string, dynamic> pair in stuffing)
                {
                    XmlTemplateTool.ReplaceKey(doc.Root, pair.Key, pair.Value);
                }
                // doc is now a filled out FO
                doc.WriteTo(fowriter);
                ok = true;
            }
            await fowriter.FlushAsync();
            return ok;
        }
        private void writeCharCodes(string s, int numChars)
        {
            System.Diagnostics.Debug.Write(s.Substring(0, numChars));
            for (int i = 0; i < numChars; i++)
            {
                System.Diagnostics.Debug.Write('|');
                System.Diagnostics.Debug.Write((int)s[i]);
            }
            System.Diagnostics.Debug.WriteLine('|');
        }
    }

}
