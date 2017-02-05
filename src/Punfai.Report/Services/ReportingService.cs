using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Punfai.Report.Interfaces;
using System.Threading.Tasks;

namespace Punfai.Report
{
    public class ReportingService : IReportingService
    {
        private readonly IReportRepository reprepo;
        private readonly List<IReportType> reportTypes;
        private readonly List<IReportScriptingEngine> scriptingEngines;
        private readonly Dictionary<string, dynamic> resources;

        public ReportingService(IReportRepository reprepo, List<IReportType> rtypes, List<IReportScriptingEngine> scriptingEngines, Dictionary<string, dynamic> resources)
        {
            this.reprepo = reprepo;
            this.reportTypes = rtypes;
            this.scriptingEngines = scriptingEngines;
            this.resources = resources;
        }

        public IEnumerable<IReportType> AvailableReportTypes { get { return reportTypes; } }

        public IReportType GetReportType(string reportTypeName)
        {
            var rt = reportTypes.FirstOrDefault(item => item.Name == reportTypeName);
            return rt;
        }

        public async Task<string> GenerateReportAsync(ReportInfo report, Stream output, Stream stdout = null)
        {
            if (output == null) throw new ArgumentNullException("output");
            bool ok;
            string outputPath = "OK.";

		    var stuffing = await RunScriptAsync(report, stdout);


            byte[] templateBytes = await reprepo.GetTemplateAsync(report.ID);
            if (templateBytes == null) throw new Exception("Can't generate a report with no template.");

            var rt = GetReportType(report.ReportType);
            var t = rt.CreateTemplate(templateBytes);



            ok = await FillReportAsync(t, rt, stuffing, output);

            if (ok)
                return outputPath;
            else
                return "Generation failed.";
        }
        public Task<Dictionary<string, dynamic>> RunScriptAsync(string scriptLanguage, IEnumerable<InputParameter> parameters, string script, Stream stdout = null)
        {
            var e = this.scriptingEngines.FirstOrDefault(a => a.ScriptLanguage == scriptLanguage);
            if (e == null) return null; // or throw
            return e.RunScriptAsync(parameters, script, resources, stdout);
        }
        public async Task<Dictionary<string, dynamic>> RunScriptAsync(ReportInfo report, Stream stdout = null)
        {
            string script = report.Script;
            if (string.IsNullOrEmpty(script)) script = await reprepo.GetScriptAsync(report.ID);
            if (string.IsNullOrEmpty(script)) throw new Exception("Can't generate a report with no script.");

            var e = this.scriptingEngines.FirstOrDefault(a => a.ScriptLanguage == report.ScriptingLanguage);
            if (e == null) return null; // or throw
            return await e.RunScriptAsync(report.Parameters, script, resources, stdout);
        }

        internal class nulllogger
        {
            public void print(object message)
            {
            }
        }
        public async Task<bool> FillReportAsync(ITemplate t, IReportType rt, Dictionary<string, dynamic> stuffing, Stream output)
        {
            if (t == null) throw new ArgumentNullException("t");
            if (output == null) throw new ArgumentNullException("output");

            bool ok = await rt.Filler.FillAsync(t, stuffing, output);
            return ok;
        }
    }
}
