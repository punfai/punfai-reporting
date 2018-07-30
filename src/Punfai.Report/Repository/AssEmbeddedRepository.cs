using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Punfai.Report
{
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

        public Task<IEnumerable<ReportInfo>> GetAllAsync(int pageIndex = 0, int pageSize = 10)
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
            var script = r.Script;
            //var resourcePath = r.Script;
            //var reader = new StreamReader(assembly.GetManifestResourceStream(resourcePath), UTF8Encoding.UTF8);
            //var script = await reader.ReadToEndAsync();
            return script;
        }

        public Task<byte[]> GetTemplateAsync(int id)
        {
            var r = reports.FirstOrDefault(a => a.ID == id);
            if (r == null) throw new Exception($"Report not found, id={id}");
            var resourcePath = r.TemplateFileName;
            Stream templateStream;
            try
            {
                templateStream = assembly.GetManifestResourceStream(resourcePath);
            }
            catch (Exception)
            {
                throw new Exception($"embedded template resource not found {resourcePath}");
            }
            var reader = new BinaryReader(templateStream, UTF8Encoding.UTF8);
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
