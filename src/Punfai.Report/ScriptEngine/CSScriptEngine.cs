﻿using CSScriptLib;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Punfai.Report
{
    public interface IRunScriptAsync
    {
        Task ProcessDataAsync(Dictionary<string, dynamic> data, Dictionary<string, dynamic> resources);
    }
    public class CSScriptEngine : IReportScriptingEngine
    {
        public const string ScriptingLanguage = "C#Script";
        public string ScriptLanguage => ScriptingLanguage;

        public async Task<Dictionary<string, dynamic>> RunScriptAsync(IEnumerable<InputParameter> parameters, string script, Dictionary<string, dynamic> resources, Stream stdout = null)
        {
            Dictionary<string, dynamic> data = new Dictionary<string, object>();
            foreach (var p in parameters)
            {
                data.Add(p.Name, p.Value ?? p.TestValue);
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using System.Linq;");
            // split the script and strip out the usings
            string[] lines = script.Split('\n');
            int? iLastUsing = null;
            for (int i = 0; i < lines.Length; i++)
                if (lines[i].StartsWith("using "))
                {
                    iLastUsing = i;
                    sb.AppendLine(lines[i].TrimEnd());
                }


            // end use case might want resources ready to go as vars
            // but until then we:
            // var payslips = (IEnumerable<PayslipDisplayVM>)resources["payslips"];
            // var contractors = (IEnumerable<Contractor>)resources["contractors"];
            // ... but how do we get these into resources?
            // !!! dynamic is not allow to do linq !!!

            sb.AppendLine("public async Task ProcessDataAsync(Dictionary<string, dynamic> data, Dictionary<string, dynamic> resources)");
            sb.AppendLine("{");
            //foreach (var pair in resources)
            //{
            //    sb.Append("\tdynamic ");
            //    sb.Append(pair.Key);
            //    sb.Append(" = resources[\"").Append(pair.Key).AppendLine("\"];");
            //}
            //foreach (var pair in parameters)
            //{
            //    sb.Append("\tdynamic ");
            //    sb.Append(pair.Name);
            //    sb.Append(" = data[\"").Append(pair.Name).AppendLine("\"];");
            //}
            for (int i = (iLastUsing + 1) ?? 0; i < lines.Length; i++)
                sb.AppendLine(lines[i].TrimEnd());
            sb.AppendLine("}");
            string fullScript = sb.ToString();
            IRunScriptAsync runner = CSScript.Evaluator.LoadMethod<IRunScriptAsync>(fullScript);
            await runner.ProcessDataAsync(data, resources);
            return data;
        }
    }
}