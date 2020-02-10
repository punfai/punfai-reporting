﻿using Punfai.Report.Template;
using System.IO;
using System.Reflection;
using System.Text;

namespace Punfai.Report.Ibex.Netcore
{
    public class IbexFoReportType : IReportType
    {
        public IbexFoReportType(string ibexRuntimeKey)
        {
            var foFiller = new FoFiller();
            this.Filler = new IbexFiller(foFiller, ibexRuntimeKey);
        }

        public string Name { get { return "Ibex FoFiller PDF"; } }
        public string DocumentType { get { return "pdf"; } }
        public IReportFiller Filler { get; private set; }
        public ITemplate CreateTemplate(byte[] templateBytes)
        {
            return new PlainTextTemplate(templateBytes);
        }
        public bool GetDefaultTemplate(out byte[] template)
        {
            Assembly _assembly;
            try
            {
                _assembly = Assembly.GetExecutingAssembly();
                var reader = new BinaryReader(_assembly.GetManifestResourceStream("Punfai.Report.Ibex.Netcore.template.fo"), UTF8Encoding.UTF8);
                template = reader.ReadBytes((int)reader.BaseStream.Length);
            }
            catch
            {
                template = UTF8Encoding.UTF8.GetBytes("template not found");
            }
            return true;
        }
    }
}
