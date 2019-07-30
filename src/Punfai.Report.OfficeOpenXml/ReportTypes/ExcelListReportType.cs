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
    public class ExcelListReportType : IReportType
    {
        private readonly IReportFiller filler;

        public ExcelListReportType()
        {
            this.filler = new ExcelEnumerableFiller();
        }
        public ExcelListReportType(IReportFiller filler)
        {
            // An ExcelReportType needs a specialised dedicated filler, not any old text search and replace
            if (!filler.SupportedReports.Contains(this.GetType()))
                throw new ArgumentOutOfRangeException("filler");
            this.filler = filler;
        }

        public string Name { get { return "Excel List"; } }
        public string DocumentType { get { return "xlsx"; } }
        public IReportFiller Filler { get { return filler; } }
        public ITemplate CreateTemplate(byte[] templateBytes)
        {
            return null;
        }
        public bool GetDefaultTemplate(out byte[] template)
        {
            template = null;
            return false;
        }
    }
}
