using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Punfai.Report
{
    public class PassThroughEngine : IReportScriptingEngine
    {
        public const string ScriptingLanguage = "PassThrough";
        public string ScriptLanguage => ScriptingLanguage;

        public Task<Dictionary<string, dynamic>> RunScriptAsync(IEnumerable<InputParameter> parameters, string script, Dictionary<string, dynamic> resources, Stream stdout = null)
        {
            Dictionary<string, dynamic> dic = new Dictionary<string, object>();
            foreach (var p in parameters)
            {
                dic.Add(p.Name, p.Value);
            }
            return Task.FromResult(dic);
        }
    }
}
