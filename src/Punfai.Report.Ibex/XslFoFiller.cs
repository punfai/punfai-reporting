using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace Punfai.Report.Ibex
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
            //xal.AddExtensionObject("http://mesh/xsltools", new Mesh.Reporting.XSLTools());
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

        #region private stuff
        /*

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
        private void Load(Stream xslStream, Assembly resourceAssembly = null)
        {
            XPathDocument xpdxsl = new XPathDocument(xslStream);
            XsltSettings settings = new XsltSettings();
            XmlResolver resolver = null;
            //if (resourceAssembly != null) resolver = new XmlResourceStreamResolver(resourceAssembly);
            try
            {
                transform.Load(xpdxsl, settings, resolver);
            }
            catch (XsltCompileException xex) { News.AddError(this.GetType().FullName + ".Transform", null, xex); }
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
        */
        #endregion
    }

}
