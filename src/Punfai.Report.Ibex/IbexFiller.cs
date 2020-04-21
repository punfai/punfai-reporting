using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
#if NETCOREAPP
using xmlpdf = ibex4;
using ibex4;
using ibex4.logging;
#else
using xmlpdf = ibex4;
using ibex4;
using ibex4.logging;
#endif

namespace Punfai.Report.Ibex
{
    public class IbexFiller : IReportFiller
    {
        private static string ibexRuntimeKey;
        public static void SetIbexRuntimeKey(string key)
        {
            ibexRuntimeKey = key;
        }

        IReportFiller foFiller;
        public IbexFiller(IReportFiller foFiller)
        {
            this.foFiller = foFiller;
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
            xmlpdf.licensing.Generator.setRuntimeKey(ibexRuntimeKey);
            FODocument doc = new FODocument();
            string appPath = Directory.GetCurrentDirectory() + Path.PathSeparator;
            doc.setBaseURI_XML(appPath);
            doc.setBaseURI_XSL(appPath);
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
