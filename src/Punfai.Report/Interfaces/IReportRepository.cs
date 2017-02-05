using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Punfai.Report
{
    public interface IReportRepository
    {
        /// <summary>
        /// Gets all reports in the system, does not select large fields: Script, Template
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ReportInfo>> GetAllAsync(int pageIndex = 0, int pageSize = 10);
        Task<ReportInfo> CreateNewAsync(string reportType, string name);
        Task<ReportInfo> GetAsync(int id);
        Task<ReportInfo> GetByNameAsync(string name);
        Task<string> GetScriptAsync(int id);
        Task<byte[]> GetTemplateAsync(int reportInfoID);
        Task<bool> SaveAsync(ReportInfo r);
        Task<bool> SaveScriptAsync(int id, string script);
        Task<bool> SaveTemplateAsync(int id, byte[] template);
    }
}
