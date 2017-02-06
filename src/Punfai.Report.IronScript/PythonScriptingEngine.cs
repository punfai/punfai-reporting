using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Punfai.Report.Interfaces;
using IronPython;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Threading.Tasks;

namespace Punfai.Report.IronScript
{
    public class PythonScriptingEngine : IReportScriptingEngine
    {
        private readonly ScriptEngine engine;
        private ScriptScope scope;

        public PythonScriptingEngine()
        {
            engine = Python.CreateEngine();
            engine.Runtime.LoadAssembly(typeof(System.Dynamic.ExpandoObject).Assembly);
            // an extra 8mb if you want to include pythin Lib folder. Until then, use clr.
            //engine.SetSearchPaths(new string[] { "C:\\Program Files (x86)\\IronPython 2.7\\Lib" });
        }

        public string ScriptLanguage { get { return "IronPython"; } }

        public Task<Dictionary<string, dynamic>> RunScriptAsync(IEnumerable<InputParameter> parameters, string script, Dictionary<string, dynamic> resources, Stream stdout = null)
        {
            Task<Dictionary<string, dynamic>> t = Task.Run<Dictionary<string, dynamic>>(() =>
            {
                Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
                if (string.IsNullOrWhiteSpace(script)) return data;

                //TextWriter writer = new StreamWriter();
                if (stdout != null)
                {
                    engine.Runtime.IO.SetOutput(stdout, System.Text.Encoding.UTF8);
                }

                scope = engine.Runtime.CreateScope();
                foreach (var pair in resources)
                {
                    scope.SetVariable(pair.Key, pair.Value);
                }
                scope.SetVariable("data", data);
                foreach (var p in parameters.ToList())
                {
                    if ("pump,data".Split(',').Contains(p.Name))
                        throw new Exception("Illegal parameter name '" + p.Name + "'");
                    scope.SetVariable(p.Name, p.Value);
                }

                try
                {
                    engine.Execute(script, scope);
                }
                catch (Exception ex)
                {
                    if (stdout != null)
                    {
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ex.Message);
                        stdout.Write(bytes, 0, bytes.Length);
                    }
                }
                return data;
            });
            return t;
        }

        internal class nulllogger
        {
            public void print(object message)
            {
            }
        }

        // DO NOT DELETE THIS CODE UNLESS WE NO LONGER REQUIRE ASSEMBLY A!!!
        private void DummyFunctionToMakeSureReferencesGetCopiedProperly_DO_NOT_DELETE_THIS_CODE()
        {
            // Assembly A is used by this file, and that assembly depends on assembly B,
            // but this project does not have any code that explicitly references assembly B. Therefore, when another project references
            // this project, this project's assembly and the assembly A get copied to the project's bin directory, but not
            // assembly B. So in order to get the required assembly B copied over, we add some dummy code here (that never
            // gets called) that references assembly B; this will flag VS/MSBuild to copy the required assembly B over as well.
            var dummyType1 = typeof(IronPython.Modules.ComplexMath);
            //var dummyType2 = typeof(IronPython.SQLite.PythonSQLite);
            //var dummyType3 = typeof(IronPython.Modules.Wpf);
        }
    }
}
