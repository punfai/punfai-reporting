using Punfai.Report.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;
using Punfai.Report.Fillers;
using Punfai.Report.Template;

namespace Punfai.Report.ReportTypes
{
    public class CsvReportType : IReportType
    {
        public CsvReportType()
        {
            this.Filler = new CsvFiller();
        }

        public string Name { get { return "Csv"; } } // keep it the same as the metadata
        public string DocumentType { get { return "csv"; } } // keep it the same as the metadata
        public IReportFiller Filler { get; private set; }
        public ITemplate CreateTemplate(byte[] templateBytes)
        {
            return new PlainTextTemplate(templateBytes);
        }
        public bool GetDefaultTemplate(out byte[] template)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine("# There are two ways to create a csv:");
            s.AppendLine("# 1. No template - delete everything in this template, add an IList<IList<object>> to the data dictionary using any key = 'rows'");
            s.AppendLine("#    data['rows'] = somelist (IList<object>)");
            s.AppendLine("# 2. Using a template with placeholders.");
            s.AppendLine("#    data['rows'] = somedic (IList of IDictionary<string,object> or IDictionary<object,object>)");
            s.AppendLine("#    in other words a list of dynamic ExpandoObjects or a list of python dictionaries");
            s.AppendLine("{{ID}}, \"{{FirstName}}\", \"{{Surname}}\", \"{{DateOfBirth}}\"");
            char[] chars = new char[s.Length];
            s.CopyTo(0, chars, 0, s.Length);
            template = UTF8Encoding.UTF8.GetBytes(chars);
            return true;
        }
    }
}
