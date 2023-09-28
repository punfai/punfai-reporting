using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Punfai.Report
{
    /// <summary>
    /// Skip the database store and embed templates and scripts in the assembly.
    /// Initialise it with SaveAsync which caches the reports in an instance variable.
    /// TemplateFileName is the path of the embedded template resource.
    /// Script is passed in as a property on the report, and often null when ScriptingEngine == PassThroughEngine.
    /// If script is null and not using passthroughengine, then look for an embedded script. Follow the convention.
    /// Script resource = TemplateFileName minus the extension plus .script.
    /// So make sure TemplateFileName has an extension like .xml|.csv|.txt
    /// If template and script is null I think it should auto serialise your data to a csv if reportType=csv
    /// </summary>
    public class AssEmbeddedRepository : IReportRepository
    {
        private readonly Assembly assembly;
        private List<ReportInfo> reports;
        private List<IReportType> reportTypes;
        public AssEmbeddedRepository(Assembly assembly, IEnumerable<IReportType> reportTypes)
        {
            this.reports = new List<ReportInfo>();
            this.reportTypes = new List<IReportType>(reportTypes);
            this.assembly = assembly;
        }

        public Task<ReportInfo> CreateNewAsync(string reportType, string name)
        {
            int i = reports.Count + 1;
            var r = new ReportInfo.Builder(1, reportType)
                .Name(name)
                .Build();
            this.reports.Add(r);
            return Task.FromResult(r);
        }

        public Task<IEnumerable<ReportInfo>> GetAllAsync(int pageIndex = 0, int pageSize = 0)
        {
            return Task.FromResult((IEnumerable<ReportInfo>)reports.ToArray());
        }

        public Task<ReportInfo> GetAsync(int id)
        {
            return Task.FromResult(reports.FirstOrDefault(a => a.ID == id));
        }

        public Task<ReportInfo> GetByNameAsync(string name)
        {
            return Task.FromResult(reports.FirstOrDefault(a => a.Name == name));
        }
        public async Task<string> GetScriptAsync(int id)
        {
            var r = reports.FirstOrDefault(a => a.ID == id);
            if (r == null) throw new Exception($"Report not found, id={id}");
            string script;
            if (r.Script == null && r.ScriptingLanguage != PassThroughEngine.ScriptingLanguage && r.TemplateFileName != null)
            {
                // this is the convention
                var resourcePath = string.Concat(r.TemplateFileName.AsSpan(0, r.TemplateFileName.LastIndexOf('.')), ".script");
                if (resourcePath == null)
                    return null;
                Stream scriptStream;
                try
                {
                    scriptStream = assembly.GetManifestResourceStream(resourcePath);
                    if (scriptStream == null) throw new Exception();
                }
                catch (Exception)
                {
                    throw new Exception($"embedded template resource not found {resourcePath}");
                }
                var reader = new StreamReader(scriptStream, new UTF8Encoding(false));
                script = await reader.ReadToEndAsync();
            }
            else
                script = r.Script;
            return script;
        }

        public Task<byte[]> GetTemplateAsync(int id)
        {
            var r = reports.FirstOrDefault(a => a.ID == id);
            if (r == null) throw new Exception($"Report not found, id={id}");
            var resourcePath = r.TemplateFileName;
            if (resourcePath == null)
            {
                return Task.FromResult(new byte[] { });
                //var rtype = reportTypes.First(a => a.Name == r.ReportType);
                //if (rtype.GetDefaultTemplate(out byte[] defaultTemplate))
                //    return Task.FromResult(defaultTemplate);
            }
            Stream templateStream;
            try
            {
                templateStream = assembly.GetManifestResourceStream(resourcePath);
                if (templateStream == null) throw new Exception();
            }
            catch (Exception)
            {
                return Task.FromResult(new byte[] { }); // could be a csv that doesn't want a template.
                //throw new Exception($"embedded template resource not found {resourcePath}");
            }
            var reader = new BinaryReader(templateStream, new UTF8Encoding(false));
            var template = reader.ReadBytes((int)reader.BaseStream.Length);
            return Task.FromResult(template);
        }

        public Task<bool> SaveAsync(ReportInfo r)
        {
            if (reports.Any(a => a.Name == r.Name))
            {
                int i = reports.FindIndex(a => a.Name == r.Name);
                reports.RemoveAt(i);
            }
            reports.Add(r);
            r.SetId(reports.Count);
            return Task.FromResult(true);
        }

        public Task<bool> SaveScriptAsync(int id, string script)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveTemplateAsync(int id, byte[] template)
        {
            throw new NotImplementedException();
        }

    }
}
