using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Punfai.Report.Interfaces;
using System.Threading.Tasks;
using System.Reflection;
using System.Dynamic;

namespace Punfai.Report
{
    public class ReportingService : IReportingService
    {
        private readonly IReportRepository reprepo;
        private readonly IEnumerable<IReportType> reportTypes;
        private readonly IEnumerable<IReportScriptingEngine> scriptingEngines;
        private readonly Dictionary<string, dynamic> resources;

        public ReportingService(IReportRepository reprepo, IEnumerable<IReportType> rtypes, IEnumerable<IReportScriptingEngine> scriptingEngines, Dictionary<string, dynamic> resources)
        {
            this.reprepo = reprepo;
            this.reportTypes = rtypes;
            this.scriptingEngines = scriptingEngines;
            this.resources = resources;
        }

        public IEnumerable<IReportType> AvailableReportTypes { get { return reportTypes; } }

        #region out with the old
        public IReportType GetReportType(string reportTypeName)
        {
            var rt = reportTypes.FirstOrDefault(item => item.Name == reportTypeName);
            return rt;
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
            //if (string.IsNullOrEmpty(script)) throw new Exception("Can't generate a report with no script.");

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
        #endregion

        #region do everything: generate
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
        public async Task<string> GenerateReportAsync(string reportName, IDictionary<string,object> inputParams, Stream output, Stream stdout = null)
        {
            if (output == null) throw new ArgumentNullException("output");
            bool ok;
            string outputPath = "OK.";

            ReportInfo reportOrig = await reprepo.GetByNameAsync(reportName);
            var report = reportOrig.Copy();
            foreach (var pair in inputParams)
                report.SetInputParameter(pair.Key, pair.Value);

            var stuffing = await RunScriptAsync(report, stdout);

            byte[] templateBytes = await reprepo.GetTemplateAsync(report.ID);
            if (templateBytes == null) throw new Exception("Can't generate a report with no template.");

            var rt = GetReportType(report.ReportType);
            var t = rt.CreateTemplate(templateBytes);

            ok = await FillReportAsync(t, rt, stuffing, output);

            if (ok)
                return outputPath;
            else
            {
                if (stdout != null)
                {
                    StreamWriter w = new StreamWriter(stdout);
                    await w.WriteAsync(rt.Filler.LastError);
                    await w.FlushAsync();
                }
                return $"Generation failed. {rt.Filler.LastError}";
            }
        }

        public Task<string> GenerateReportAsync(string reportName, IEnumerable<(string, object)> inputParams, Stream output, Stream stdout = null)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            foreach (var pair in inputParams)
            {
                dic[pair.Item1] = pair.Item2;
            }
            return GenerateReportAsync(reportName, dic, output, stdout);
        }

        #endregion

        #region static helpers
        public static async Task<ReportingService> CreateAssemblyEmbedded(
            IEnumerable<IReportType> reportTypes
            ,Assembly assembly
            ,IEnumerable<ReportInfo> reports)
        {
            AssEmbeddedRepository reprepo = new AssEmbeddedRepository(assembly, reportTypes);
            foreach (var r in reports)
            {
                await reprepo.SaveAsync(r);
            }
            var scriptingEngines = new IReportScriptingEngine[] { new PassThroughEngine() };
            Dictionary<string, object> resources = new Dictionary<string, object>();
            ReportingService reportingService = new ReportingService(reprepo, reportTypes, scriptingEngines, resources);
            return reportingService;
        }
        public static IEnumerable<IReportType> GetReportTypes()
        {
            var rtypes = new IReportType[] {
                new Punfai.Report.ReportTypes.CsvReportType()
                ,new Punfai.Report.ReportTypes.TextReportType()
                ,new Punfai.Report.ReportTypes.XmlReportType()
            };
            return rtypes;
        }
        #endregion
    }
}
