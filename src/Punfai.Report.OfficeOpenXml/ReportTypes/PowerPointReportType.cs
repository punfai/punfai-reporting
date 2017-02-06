using Punfai.Report.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;
using Punfai.Report.OfficeOpenXml.Fillers;
using Punfai.Report.OfficeOpenXml.Template;
using Punfai.Report.Template;

namespace Punfai.Report.OfficeOpenXml.ReportTypes
{
    public class PowerPointReportType : IReportType
    {
        public PowerPointReportType()
        {
            this.Filler = new PowerPointFiller();
        }

        public string Name { get { return "Microsoft PowerPoint"; } }
        public string DocumentType { get { return "pptx"; } }
        public IReportFiller Filler { get; private set; }
        public ITemplate CreateTemplate(byte[] templateBytes)
        {
            return new PlainTextTemplate(templateBytes);
        }
        public bool GetDefaultTemplate(out byte[] template)
        {
            template = null;
            return false;
        }
    }
}
