using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Punfai.Report.Utils;
using System.Threading.Tasks;
using xmlpdf.logging;
using xslfo;

namespace Punfai.Report.Ibex
{
    public class IbexFiller : IReportFiller
    {
        public const string LIC_XMLPDFLicense = "Punfai.Report.Ibex.xmlpdf.lic";
        private readonly string ibexRuntimeKey;

        IReportFiller foFiller;
        public IbexFiller(IReportFiller foFiller, string ibexRuntimeKey)
        {
            this.foFiller = foFiller;
            this.ibexRuntimeKey = ibexRuntimeKey;
        }
        public Type[] SupportedReports { get { return new[] { typeof(IbexFoReportType), typeof(IbexXslReportType) }; } }

        public string LastError { get; private set; }

        public async Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            MemoryStream fostream = new MemoryStream();
            // the inner filler produces the full FO document (using xslt or some cool templating engine)
            // we just generate a pdf with Ibex and write it to the output stream
            var ok = await foFiller.FillAsync(t, stuffing, fostream);
            if (!ok)
            {
                LastError = foFiller.LastError;
                return false;
            }
            //xmlpdf.licensing.Generator.LicenseFileLocation = LIC_XMLPDFLicense;
            //xmlpdf.licensing.Generator.LicenseFileLocation = licPath;
            xmlpdf.licensing.Generator.setRuntimeKey(ibexRuntimeKey);
            FODocument doc = new FODocument();
            string appPath = Directory.GetCurrentDirectory() + Path.PathSeparator;
            doc.setBaseURI(appPath);
            MemoryStream memstream = new MemoryStream();
            StreamReader r = new StreamReader(memstream);
            var logger = xmlpdf.logging.Logger.getLogger();
            logger.setLevel(Level.WARNING);
            logger.clearHandlers();
            StreamHandler h = new StreamHandler(memstream);
            logger.addHandler(h);
            try
            {
                fostream.Position = 0;
                doc.generate(fostream, output);
            }
            catch (Exception ex)
            {
                //logger.Error(this.GetType().FullName + ".Transform", "problem generating pdf", ex);
                LastError = ex.Message;
                return false;
            }
            finally
            {
                fostream.Close();
                output.Close();
            }
            memstream.Position = 0;
            var message = r.ReadToEnd();
            logger.clearHandlers();
            if (message != null && message.Length > 0)
            {
                LastError = message;
                return false;
            }
            return true;
        }
    }

}
