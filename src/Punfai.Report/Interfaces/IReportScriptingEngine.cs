using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Punfai.Report
{
    public interface IReportScriptingEngine
    {
        string ScriptLanguage { get; }
        Task<Dictionary<string, dynamic>> RunScriptAsync(IEnumerable<InputParameter> parameters, string script, Dictionary<string, dynamic> resources, Stream stdout = null);
    }
}
