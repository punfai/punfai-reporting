﻿using Punfai.Report.Fillers;
using Punfai.Report.Template;
using System.Text;

namespace Punfai.Report.ReportTypes
{
    public class XmlReportType : IReportType
    {
        public XmlReportType()
        {
            this.Filler = new XmlFiller();
        }

        public string Name { get { return "Xml"; } }
        public string DocumentType { get { return "xml"; } }
        public IReportFiller Filler { get; private set; }
        public ITemplate CreateTemplate(byte[] templateBytes)
        {
            return new PlainTextTemplate(templateBytes);
        }
        public bool GetDefaultTemplate(out byte[] template)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine("<html>");
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
