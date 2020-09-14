using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Punfai.Report
{
    public class ReportInfo
    {
        public ReportInfo(string name, string reportType, string scriptingLanguage)
        {
            this.Name = name;
            this.ReportType = reportType;
            this.ScriptingLanguage = scriptingLanguage;
            ID = -1;
            Parameters = new ObservableCollection<InputParameter>();
        }
        private ReportInfo()
        {
            Parameters = new ObservableCollection<InputParameter>();
        }
        public static ReportInfo CreateDefault(string reportType)
        {
            return new Builder(-1, reportType).Build();
        }

        public int ID { get; protected set; }
        public string ReportType { get; protected set; }
        public Guid Uid { get; protected set; }
        public string Code { get; protected set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ObservableCollection<InputParameter> Parameters { get; protected set; }
        public string ScriptingLanguage { get; set; }
        public string Script { get; set; }
        public string Dependencies { get; set; }
        public string Author { get; set; }
        public string Data { get; set; }
        public string TemplateFileName { get; set; }


        public class Builder
        {
            ReportInfo args = new ReportInfo();
            public Builder()
            {
                args.ID = -1;
                args.ScriptingLanguage = "IronPython";
            }
            public Builder(int id, string reportType)
            {
                args.ID = id;
                args.ReportType = reportType;
                args.ScriptingLanguage = "IronPython";
            }
            public Builder Uid(Guid uid)
            {
                args.Uid = uid;
                return this;
            }
            public Builder Code(string code)
            {
                args.Code = code;
                return this;
            }
            public Builder Name(string name)
            {
                args.Name = name;
                return this;
            }
            public Builder Description(string description)
            {
                args.Description = description;
                return this;
            }
            public Builder Parameters(string serialisedParameters)
            {
                args.processParameters(serialisedParameters);
                return this;
            }
            public Builder Parameters(ObservableCollection<InputParameter> parameters)
            {
                args.Parameters = parameters;
                return this;
            }
            public Builder ScriptingLanguage(string language)
            {
                args.ScriptingLanguage = language;
                return this;
            }
            public Builder Script(string script)
            {
                args.Script = script;
                return this;
            }
            public Builder Dependencies(string dependencies)
            {
                args.Dependencies = dependencies;
                return this;
            }
            public Builder Author(string author)
            {
                args.Author = author;
                return this;
            }
            public Builder Data(string data)
            {
                args.Data = data;
                return this;
            }
            public Builder TemplateFileName(string templateFileName)
            {
                args.TemplateFileName = templateFileName;
                return this;
            }
            public ReportInfo Build()
            {
                //if (args.Script == null) throw new Exception("no ??? specified");
                var ret = args;
                args = null;
                return ret;
            }
        }

        #region methods
        public void SetId(int id)
        {
            this.ID = id;
        }
        private void processParameters(string parameters)
        {
            ObservableCollection<InputParameter> dic = new ObservableCollection<InputParameter>();
            if (parameters != null)
            {
                string[] arr1 = parameters.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in arr1)
                {
                    string[] kv = s.Split(':');
                    if (kv.Length > 0)
                    {
                        InputParameter p = new InputParameter() { Name = kv[0] };
                        if (kv.Length > 1) p.TestValue = kv[1];
                        if (kv.Length > 2) p.DataType = kv[2];
                        dic.Add(p);
                    }
                }
            }
            this.Parameters = dic;
        }
        public string SerialiseParameters()
        {
            string blah = "";
            if (Parameters != null) blah = string.Join(";", this.Parameters.Select(p => p.Name + ":" + p.TestValue));
            var entries = this.Parameters.Select(p => p.Name + ":" + p.TestValue);
            if (Parameters != null) blah = string.Join(";", entries);
            return blah;
            //return Parameters == null ? "" : string.Join(";", this.Parameters.Select(p => p.Name + ":" + p.TestValue));
        }
        public ReportInfo Copy()
        {
            return new Builder(this.ID, this.ReportType)
                .Name(Name)
                .Description(Description)
                .Parameters(SerialiseParameters())
                .ScriptingLanguage(ScriptingLanguage)
                .Script(Script)
                .Dependencies(Dependencies)
                .Author(Author)
                .TemplateFileName(TemplateFileName)
                .Data(Data)
                .Build();
        }
        public override bool Equals(object obj)
        {
            if (obj is ReportInfo)
            {
                var r = obj as ReportInfo;
                return (r.Author == Author
                    && r.Data == Data
                    && r.Dependencies == Dependencies
                    && r.Description == Description
                    && r.ID == ID
                    && r.Name == Name
                    && r.SerialiseParameters() == SerialiseParameters()
                    && r.ReportType == ReportType
                    && r.Script == Script
                    && r.TemplateFileName == TemplateFileName
                    && r.ScriptingLanguage == ScriptingLanguage);
            }
            else
                return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        public bool SetInputParameter(string name, object value)
        {
            var p = Parameters.FirstOrDefault(a => a.Name == name);
            if (p == null)
            {
                Parameters.Add(new InputParameter() { Name = name, Value = value });
            }
            else
            {
                p.Value = value;
            }
            return true;
        }
        #endregion
    }
}
