using Punfai.Report.Template;
using System.IO;
using System.Reflection;
using System.Text;

namespace Punfai.Report.Ibex
{
    public class IbexXslReportType : IReportType
    {
        public IbexXslReportType()
        {
            var foFiller = new XslFoFiller();
            this.Filler = new IbexFiller(foFiller);
        }

        public string Name { get { return "Ibex XslFo PDF"; } }
        public string DocumentType { get { return "pdf"; } }
        public IReportFiller Filler { get; private set; }
        public ITemplate CreateTemplate(byte[] templateBytes)
        {
            //return new PlainTextTemplate(templateBytes);
            return new XslFoTemplate(templateBytes);
        }
        public bool GetDefaultTemplate(out byte[] template)
        {
            Assembly _assembly;
            //Stream _imageStream;
            try
            {
                _assembly = Assembly.GetExecutingAssembly();
                //_imageStream = _assembly.GetManifestResourceStream("Punfai.Report.Ibex.jpg");
                //var _textStreamReader = new StreamReader(_assembly.GetManifestResourceStream("Punfai.Report.Ibex.template.fo"), UTF8Encoding.UTF8);
                //var s = _textStreamReader.ReadToEnd();
                //template = UTF8Encoding.UTF8.GetBytes(s);
                var reader = new BinaryReader(_assembly.GetManifestResourceStream("Punfai.Report.Ibex.template.xslt"), UTF8Encoding.UTF8);
                template = reader.ReadBytes((int)reader.BaseStream.Length);
            }
            catch
            {
                //logger.Error("Error accessing resources!");
                template = UTF8Encoding.UTF8.GetBytes("template not found");
            }
            return true;
        }
    }
}
