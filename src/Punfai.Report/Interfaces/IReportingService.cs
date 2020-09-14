using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Punfai.Report
{
    public interface IReportingService
    {
        IEnumerable<IReportType> AvailableReportTypes { get; }

        #region out with the old
        IReportType GetReportType(string reportTypeName);
        Task<Dictionary<string, dynamic>> RunScriptAsync(string scriptLanguage, IEnumerable<InputParameter> parameters, string script, Stream stdout = null);

        // better
        Task<Dictionary<string, dynamic>> RunScriptAsync(ReportInfo report, Stream stdout = null);
        Task<bool> FillReportAsync(ITemplate t, IReportType rt, Dictionary<string, dynamic> stuffing, Stream output);

        // do everything in one call
        Task<string> GenerateReportAsync(ReportInfo report, Stream output, Stream stdout = null, bool closeStream = true);
        #endregion

        // even better
        Task<string> GenerateReportAsync(string reportName, IDictionary<string, object> inputParams, Stream output, Stream stdout = null, bool closeStream = true);
        Task<string> GenerateReportAsync(string reportName, IEnumerable<(string, object)> inputParams, Stream output, Stream stdout = null, bool closeStream = true);

    }
}
