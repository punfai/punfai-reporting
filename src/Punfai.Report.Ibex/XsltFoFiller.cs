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
    public class XsltFoFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new[] { typeof(XsltFoReportType) }; } }

        public const string LIC_XMLPDFLicense = "Mesh.GUI.WinForms.Library.xmlpdf.lic";
        private XslCompiledTransform transform;
        private XsltArgumentList xal;
		private Encoding encoding;
        private string licPath;

		public XsltFoFiller()
		{
            transform = new XslCompiledTransform();
            xal = new XsltArgumentList();
			//xal.AddExtensionObject("http://mesh/xsltools", new Mesh.Reporting.XSLTools());
			pdfFilePath = "";
            licPath = Path.Combine(pdfFilePath);
        }
		public string PageWidth
		{
			set
			{
				xslfo.UserAgent.PageWidth = value;
			}
		}
		public string PageHeight
		{
			set 
			{
				xslfo.UserAgent.PageHeight = value;
			}
		}

		public XsltArgumentList Arguments
		{
			get { return xal; }
			set { xal = value; }
		}
		/// <summary>
		/// Defaults to UTF-8
		/// </summary>
		public Encoding Encoding
		{
			get { return encoding; }
			set { encoding = value; }
		}
        public void Load(Stream xslStream, Assembly resourceAssembly = null)
        {
            XPathDocument xpdxsl = new XPathDocument(xslStream);
            XsltSettings settings = new XsltSettings();
            XmlResolver resolver = null;
            if (resourceAssembly != null) resolver = new XmlResourceStreamResolver(resourceAssembly);
            try
            {
                transform.Load(xpdxsl, settings, resolver);
            }
            catch (System.Xml.Xsl.XsltCompileException xex) { News.AddError(this.GetType().FullName + ".Transform", null, xex); }
            catch (Exception ex) { News.AddError(this.GetType().FullName + ".Transform", "problem generating pdf", ex); }
        }
        public bool Transform(Stream xmlStream, string outputFilePath)
        {
            this.pdfFilePath = outputFilePath;
            try
            {
                News.AddMessage("Checking existence of output pdf target: " + pdfFilePath);
                if (File.Exists(pdfFilePath)) File.Delete(pdfFilePath);
            }
            catch (Exception)
            {
                throw new Exception("Report failed! \r\n Is the old file open? \r\n Please close it!");
            }
            FileStream pdfStream = File.Create(pdfFilePath);
            return Transform(xmlStream, pdfStream);
        }
        public bool Transform(Stream xmlStream, Stream outputFileStream)
		{
			xmlStream.Position = 0;
			xmlpdf.licensing.Generator.LicenseFileLocation = licPath;
			FODocument doc = new FODocument();
            string appPath = Directory.GetCurrentDirectory() + Path.PathSeparator;
            doc.setBaseURI(appPath);
			Encoding enc = encoding;
			if (encoding == null) enc = System.Text.Encoding.UTF8;
			MemoryStream foStream = new MemoryStream();
            XmlWriter xwriter = XmlWriter.Create(foStream, transform.OutputSettings);
            XmlReader xreader = new XmlTextReader(xmlStream);
            try
			{
				transform.Transform(xreader, xal, xwriter);
				xwriter.Flush();
				foStream.Position = 0;
				doc.generate(foStream, outputFileStream);
			}
			catch (System.Xml.Xsl.XsltCompileException xex) { News.AddError(this.GetType().FullName + ".Transform", null,  xex); return false; }
			catch (Exception ex) { News.AddError(this.GetType().FullName + ".Transform", "problem generating pdf", ex); return false; }
			finally
			{
				xmlStream.Close();
				outputFileStream.Close();
				xwriter.Close();
                xreader.Close();
			}
			return true;
		}

	}
    public class XsltFoFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new[] { typeof(XsltFoReportType) }; } }

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
