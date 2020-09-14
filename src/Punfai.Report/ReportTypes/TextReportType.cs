using Punfai.Report.Fillers;
using Punfai.Report.Template;
using System.Text;

namespace Punfai.Report.ReportTypes
{
    public class TextReportType : IReportType
    {
        public TextReportType()
        {
            this.Filler = new TextFiller();
        }

        public string Name { get { return "Text"; } } // keep it the same as the metadata
        public string DocumentType { get { return "text"; } } // keep it the same as the metadata
        public IReportFiller Filler { get; private set; }
        public ITemplate CreateTemplate(byte[] templateBytes)
        {
            return new PlainTextTemplate(templateBytes);
        }
        public bool GetDefaultTemplate(out byte[] template)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine("# Plain text template");
            s.AppendLine("# input: data['rows'] = IEnumerable<dic>");
            s.AppendLine(@"#      : data['newline'] = '' or \n or \r\n or not given to default to \r\n. empty string is no new line.");
            s.AppendLine("#{{Name:a,m,20}}  a:alpha an:alphanum dt[format]:date[dotnetDateFormat] n:num ar:alphaRightAligned nl:numLeftAligned , m:mandatory o:optional , field length");
            char[] chars = new char[s.Length];
            s.CopyTo(0, chars, 0, s.Length);
            template = UTF8Encoding.UTF8.GetBytes(chars);
            return true;
        }
    }
}
