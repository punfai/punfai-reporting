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

namespace Punfai.Report.OfficeOpenXml.ReportTypes
{
    public class ExcelReportType : IReportType
    {
        private readonly IReportFiller filler;

        public ExcelReportType()
        {
            this.filler = new ExcelFiller();
        }
        public ExcelReportType(IReportFiller filler)
        {
            // An ExcelReportType needs a specialised dedicated filler, not any old text search and replace
            if (!filler.SupportedReports.Contains(this.GetType()))
                throw new ArgumentOutOfRangeException("filler");
            this.filler = filler;
        }

        public string Name { get { return "Microsoft Excel"; } }
        public string DocumentType { get { return "xlsx"; } }
        public IReportFiller Filler { get { return filler; } }
        public ITemplate CreateTemplate(byte[] templateBytes)
        {
            var template = new ExcelTemplate(templateBytes);
            template.LoadSections();
            return template;
        }
        public bool GetDefaultTemplate(out byte[] template)
        {
            template = null;
            return false;
        }
    }
}
