using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Prism.Mvvm;
using Prism.Commands;

using Punfai.Report;
using Punfai.Report.Interfaces;
using Punfai.Report.Wpf.Services;

namespace Punfai.Report.Wpf.Designers
{
    public class DefaultDesignerViewModel : BindableBase, IReportDesigner
    {
        private readonly IReportRepository reprepo;
        private readonly WpfReportingService reportingService;
        public enum TemplateStates { HaveTemplateState, NoTemplateState }

        public DefaultDesignerViewModel()
        {
            ReportDefinition = new ReportInfo.Builder(1, "OpenOffice Document")
                .Author("Jim")
                .Dependencies("RadEnterprise")
                .Description("Lists the top ranking products for the year")
                .Name("Top Products")
                .Parameters("startDate:2012-01-01;endDate:2012-12-31")
                .Script("print 'hello world'")
                .Build();
            OutputEntries = new ObservableCollection<OutputEntry>();
            output("output goes here");
            LanguagesList = new List<string>() { "IronPython", "IronRuby" };
            TemplateMessage = "No template selected";
            ValidText = true;
        }

        public DefaultDesignerViewModel(IReportRepository rrep, WpfReportingService reportingService)
        {
            this.reprepo = rrep;
            this.reportingService = reportingService;
            GenerateCommand = new DelegateCommand(ExecuteGenerate);
            RunCommand = new DelegateCommand(ExecuteRun);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave);
            ExportTemplateCommand = new DelegateCommand<string>(ExecuteExportTemplate);
            LoadTemplateCommand = new DelegateCommand<string>(ExecuteLoadTemplate);
            DefaultTemplateCommand = new DelegateCommand(ExecuteDefaultTemplate);
            OutputEntries = new ObservableCollection<OutputEntry>();

            LanguagesList = new List<string>() { "IronPython", "IronRuby" }; // TODO: get them from manifest or something

            TemplateMessage = "No template selected";
            ValidText = true;
        }

        #region properties

        public string OutputFolder { get { return "%Desktop%\\Reports"; } }
        public string Name { get { return Constants.DefaultConsumerReportPageTitle; } }
        public string DocumentType { get { return "*"; } }
        public ReportInfo originalInfo { get; private set; }
        public ReportInfo ReportDefinition { get; private set; }
        public ObservableCollection<OutputEntry> OutputEntries { get; set; }
        public string Message { get; private set; }
        public string TemplateMessage { get; private set; }
        public int Progress { get; set; }
        public List<string> LanguagesList { get; private set; }
        public TemplateStates TemplateState { get { return Template == null ? TemplateStates.NoTemplateState : TemplateStates.HaveTemplateState; } }
        public ITemplate Template { get; private set; }
        public CollectionView SectionsCV { get; private set; }
        public IEnumerable<string> SectionNames { get { if (Template != null) return Template.SectionNames; else return null; } }
        private string _templateText;
        public string TemplateText 
        {
            get
            {
                if (Template == null || currentSection() == null) return "";
                if (ValidText) _templateText = Template.GetSectionText(currentSection());
                return _templateText;
            }
            set
            {
                if (Template != null && currentSection() != null)
                {
                    _templateText = value;
                    ValidText = Template.SetSectionText(currentSection(), value);
                    RaisePropertyChanged(nameof(ValidText));
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public bool ValidText { get; private set; }

        /**
         *  ReportArgs Properties 
         **/
        public string ReportName { get { return ReportDefinition.Name; } set { ReportDefinition.Name = value; raise(); } }
        public string Description { get { return ReportDefinition.Description; } set { ReportDefinition.Description = value; raise(); } }
        public ObservableCollection<InputParameter> Parameters { get; private set; }
        public string ScriptingLanguage { get { return ReportDefinition.ScriptingLanguage; } set { ReportDefinition.ScriptingLanguage = value; raise(); } }
        public string Script { get { return ReportDefinition.Script; } set { ReportDefinition.Script = value; raise(); } }
        public string Dependencies { get { return ReportDefinition.Dependencies; } set { ReportDefinition.Dependencies = value; raise(); } }
        public string Author { get { return ReportDefinition.Author; } set { ReportDefinition.Author = value; raise(); } }
        public string TemplateFileName { get { return ReportDefinition.TemplateFileName; } set { ReportDefinition.TemplateFileName = value; raise(); } }

        #endregion

        #region commands

        public DelegateCommand GenerateCommand { get; private set; }
        public DelegateCommand RunCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand BackCommand { get; private set; }
        public DelegateCommand<string> ExportTemplateCommand { get; private set; }
        public DelegateCommand<string> LoadTemplateCommand { get; private set; }
        public DelegateCommand DefaultTemplateCommand { get; private set; }

        private async void ExecuteGenerate()
        {
            clearOut();
            output("--------generating--------");
            if (Script.Trim() == string.Empty)
            {
                output("Give me some code!");
                return;
            }
            Message = "Running";
            RaisePropertyChanged(nameof(Message));

            foreach (var p in ReportDefinition.Parameters)
                p.Value = p.TestValue;

            try
            {
                MemoryStream stdout = new MemoryStream();
                var stuffing = await reportingService.RunScriptAsync(ReportDefinition, stdout);
                await reportingService.WriteReportsAsync(ReportDefinition, stuffing, OutputFolder, stdout);
                //await reportingService.GenerateReportAsync(ReportDefinition, resources, OutputFolder, stdout);
                stdout.Flush();
                writestdout(stdout);
                output("---------done----------");
                writeGeneratedPaths(stuffing);
            }
            catch (Exception ex)
            {
                output("---------error----------");
                output(ex.Message);
            }
        }
        private async void ExecuteRun()
        {
            clearOut();
            if (string.IsNullOrWhiteSpace(Script))
            {
                output("Give me some code!");
                return;
            }
            Message = "Running";
            RaisePropertyChanged(nameof(Message));
            output("--------starting--------");

            foreach (var p in ReportDefinition.Parameters)
                p.Value = p.TestValue;

            Dictionary<string, dynamic> data = null;
            try
            {
                MemoryStream stdout = new MemoryStream();
                data = await reportingService.RunScriptAsync(ReportDefinition.ScriptingLanguage, ReportDefinition.Parameters, Script, stdout);
                stdout.Flush();
                writestdout(stdout);
            }
            catch (Exception ex)
            {
                output(ex.Message);
            }
            if (data != null)
            {
                output("-----------------------");
                output("data variable contents:");
                foreach (KeyValuePair<string, dynamic> pair in data)
                {
                    output(string.Format("[{0}] = {1}", pair.Key, pair.Value));
                }
            }
        }

        private async void ExecuteSave()
        {
            //Message = "Saving";
            //OnPropertyChanged("Message");

            await reprepo.SaveAsync(this.ReportDefinition);
            await reprepo.SaveScriptAsync(this.ReportDefinition.ID, this.Script);
            if (Template != null)
            {
                await reprepo.SaveTemplateAsync(this.ReportDefinition.ID, Template.GetTemplateBytes());
                Template.AcceptChanges();
            }

            originalInfo = ReportDefinition.Copy();
            raise();
        }
        private bool CanExecuteSave()
        {
            return originalInfo != null && !originalInfo.Equals(ReportDefinition) || (Template != null && Template.IsChanged);
        }
        private void ExecuteBack()
        {
            ExecuteSave();
            if (Back != null)
                Back(this, EventArgs.Empty);
        }
        private void ExecuteLoadTemplate(string filepath)
        {
            if (filepath == null || filepath == string.Empty) return;
            FileInfo info = new FileInfo(filepath);
            if (!info.Exists)
                throw new Exception("What? File does not exist");
            readTemplate(info);
        }
        private void ExecuteExportTemplate(string filepath)
        {
            if (filepath == null || filepath == string.Empty) return;
            exportTemplate(filepath);
        }
        private async void ExecuteDefaultTemplate()
        {
            byte[] buf;
            IReportType rt = reportingService.GetReportType(ReportDefinition.ReportType);
            if (rt.GetDefaultTemplate(out buf))
            {
                TemplateMessage = "Loading template...";
                RaisePropertyChanged(nameof(TemplateMessage));
                await reprepo.SaveTemplateAsync(ReportDefinition.ID, buf);
                Template = rt.CreateTemplate(buf);
                ReportDefinition.TemplateFileName = "DefaultTemplate";
                if (SectionsCV != null) SectionsCV.CurrentChanged -= SectionsCV_CurrentChanged;
                SectionsCV = new ListCollectionView(new List<string>(Template.SectionNames));
                SectionsCV.CurrentChanged += SectionsCV_CurrentChanged;
                raise();
            }
            else
            {
                TemplateMessage = "Sorry no default template defined for " + rt.Name + ". Best to design a template manually then load it in.";
            }
        }

        private async void readTemplate(FileInfo info)
        {
            TemplateMessage = "Loading template...";
            RaisePropertyChanged(nameof(TemplateMessage));
            FileStream stream;
            try { stream = info.OpenRead(); }
            catch (Exception) 
            {
                TemplateMessage = "Error: file in use";
                RaisePropertyChanged(nameof(TemplateMessage));
                return; 
            }
            byte[] buf = new byte[stream.Length];
            int x = await stream.ReadAsync(buf, 0, buf.Length);
            await reprepo.SaveTemplateAsync(ReportDefinition.ID, buf);
            IReportType rt = reportingService.GetReportType(ReportDefinition.ReportType);
            Template = rt.CreateTemplate(buf);
            ReportDefinition.TemplateFileName = Path.GetFileName(info.FullName);
            if (SectionsCV != null) SectionsCV.CurrentChanged -= SectionsCV_CurrentChanged;
            SectionsCV = new ListCollectionView(new List<string>(Template.SectionNames));
            SectionsCV.CurrentChanged += SectionsCV_CurrentChanged;
            raise();
        }
        private async void exportTemplate(string filepath)
        {
            try
            {
                using (FileStream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write))
                {
                    //IReportType rt = reportingService.GetReportType(ReportDefinition.ReportType);
                    byte[] buf = Template.GetTemplateBytes();
                    await stream.WriteAsync(buf, 0, buf.Length);
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region methods

        public void output(string s)
        {
            OutputEntries.Add(new OutputEntry() { Message = s });
        }
        public bool softspace;
        public void write(string s)
        {
            OutputEntries.Add(new OutputEntry() { Message = s });
        }
        private void writestdout(Stream stream)
        {
            stream.Flush();
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            string line;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                output(line);
            }
        }
        private void writeGeneratedPaths(Dictionary<string, dynamic> stuffing)
        {
            if (stuffing.ContainsKey("folders"))
            {
                foreach (var folder in stuffing["folders"])
                {
                    writeGeneratedPaths(folder);
                }
            }
            else if (stuffing.ContainsKey("folder") && stuffing.ContainsKey("files"))
            {
                output(string.Format("output to folder: {0}", stuffing["folder"]));
                foreach (var file in stuffing["files"])
                {
                    output(string.Format("   {0}", file["filePath"]));
                }
            }
        }

        private void raise()
        {
            RaisePropertyChanged(nameof(ReportName));
            RaisePropertyChanged(nameof(ReportDefinition));
            RaisePropertyChanged(nameof(Description));
            RaisePropertyChanged(nameof(ScriptingLanguage));
            RaisePropertyChanged(nameof(Parameters));
            RaisePropertyChanged(nameof(Dependencies));
            RaisePropertyChanged(nameof(Author));
            RaisePropertyChanged(nameof(TemplateFileName));
            RaisePropertyChanged(nameof(TemplateState));
            RaisePropertyChanged(nameof(Script));
            RaisePropertyChanged(nameof(SectionsCV));
            RaisePropertyChanged(nameof(TemplateText));

            SaveCommand.RaiseCanExecuteChanged();
        }

        private void clearOut()
        {
            OutputEntries.Clear();
        }

        private string currentSection()
        {
            return SectionsCV.CurrentItem as string;
        }

        #endregion

        #region IReportDesigner Members

        public async void Load(ReportInfo specifiedReport)
        {
            clearOut();
            // track changes
            this.originalInfo = specifiedReport.Copy();
            this.ReportDefinition = specifiedReport;
            // parameters
            if (Parameters != null) Parameters.CollectionChanged -= paramsCollection_CollectionChanged;
            Parameters = ReportDefinition.Parameters;
            Parameters.CollectionChanged += paramsCollection_CollectionChanged;
            // template
            if (SectionsCV != null) SectionsCV.CurrentChanged -= SectionsCV_CurrentChanged;
            byte[] buf = await reprepo.GetTemplateAsync(ReportDefinition.ID);
            if (buf == null)
            {
                Template = null;
                TemplateMessage = string.Format("No template selected");
                SectionsCV = null;
            }
            else
            {
                IReportType rt = reportingService.GetReportType(ReportDefinition.ReportType);
                Template = rt.CreateTemplate(buf);
                SectionsCV = new CollectionView(Template.SectionNames);
                SectionsCV.CurrentChanged += SectionsCV_CurrentChanged;
            }
            // notify
            raise();
        }

        void SectionsCV_CurrentChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(TemplateText));
        }

        void paramsCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveCommand.RaiseCanExecuteChanged();
        }

        public event EventHandler Back;

        #endregion
    }

}
