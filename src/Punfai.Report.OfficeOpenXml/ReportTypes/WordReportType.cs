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
    public class WordReportType : IReportType
    {
        public WordReportType()
        {
            this.Filler = new WordFiller();
        }

        public string Name { get { return "Microsoft Word"; } }
        public string DocumentType { get { return "docx"; } }
        public IReportFiller Filler { get; private set; }
        public ITemplate CreateTemplate(byte[] templateBytes)
        {
            var template = new WordTemplate(templateBytes);
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
