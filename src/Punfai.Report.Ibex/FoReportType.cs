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

namespace Punfai.Report.Ibex
{
    public class FoReportType : IReportType
    {
        public FoReportType()
        {
            this.Filler = new FoFiller();
        }

        public string Name { get { return "Ibex FO PDF"; } }
        public string DocumentType { get { return "pdf"; } }
        public IReportFiller Filler { get; private set; }
        public ITemplate CreateTemplate(byte[] templateBytes)
        {
            return new PlainTextTemplate(templateBytes);
        }
        public bool GetDefaultTemplate(out byte[] template)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine("<fo>");
            s.AppendLine("<body>");
            s.AppendLine("<pre>data['Title'] = 'Hello world'</pre>");
            s.AppendLine("<h1>{{Title}}</h1>");
            s.AppendLine("<pre>data['somelist'] = listofpeople</pre>");
            s.AppendLine("<p mesh-repeat=\"person in somelist\">{{person.FirstName}} {{person.Surname}}</p>");
            s.AppendLine("</body>");
            s.AppendLine("</html>");
            char[] chars = new char[s.Length];
            s.CopyTo(0, chars, 0, s.Length);
            template = UTF8Encoding.UTF8.GetBytes(chars);
            return true;
        }
    }
}
