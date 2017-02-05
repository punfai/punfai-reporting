using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Punfai.Report
{
    public interface IReportingService
    {
        IEnumerable<IReportType> AvailableReportTypes { get; }
        IReportType GetReportType(string reportTypeName);
        Task<string> GenerateReportAsync(ReportInfo report, Stream output = null, Stream stdout = null);
        Task<Dictionary<string, dynamic>> RunScriptAsync(string scriptLanguage, IEnumerable<InputParameter> parameters, string script, Stream stdout = null);

        // better
        Task<Dictionary<string, dynamic>> RunScriptAsync(ReportInfo report, Stream stdout = null);
        Task<bool> FillReportAsync(ITemplate t, IReportType rt, Dictionary<string, dynamic> stuffing, Stream output);
    }
}
