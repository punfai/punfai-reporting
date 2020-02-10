using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace Punfai.Report.Ibex.Netcore
{
    public class XslFoFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new Type[] { }; } }
        public string LastError { get; private set; }

        private XslCompiledTransform transform;
        private XsltArgumentList xal;
		private Encoding encoding;

		public XslFoFiller()
		{
            transform = new XslCompiledTransform();
            xal = new XsltArgumentList();
        }

        public async Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            // TODO: make this more asyncy
            XmlWriter writer = XmlWriter.Create(output, new XmlWriterSettings() { Encoding = UTF8Encoding.UTF8, Indent = true, Async = true });
            // should only be one section
            foreach (var section in t.SectionNames)
            {
            }
            await writer.FlushAsync();
            return true;
        }
    }

}
