using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Punfai.Report.Wpf.Consumer
{
    public class RegularConsumerViewModel : BindableBase, IReportPage
    {
        private readonly IReportRepository reprepo;
        private readonly WpfReportingService reportingService;
        private readonly Dictionary<string, dynamic> reportResources;
        private List<ReportInfo> allReports;

        public RegularConsumerViewModel()
        {
            ReportMessage = "Generating report";
            ReportProgress = 30;
        }

        public RegularConsumerViewModel(IReportRepository reportrepo, WpfReportingService reportingService, Dictionary<string, dynamic> reportResources)
        {
            this.reprepo = reportrepo;
            this.reportingService = reportingService;
            this.reportResources = reportResources;
            GenerateCommand = new DelegateCommand(ExecuteGenerate, CanExecuteGenerate);

            ReportMessage = getDefaultMessage();

            OutputFolder = "%Desktop%\\Reports";
        }

        #region properties
        private bool isWorking = false;
        public string OutputFolder { get; set; }
        public int ReportProgress { get; private set; }
        public string ReportMessage { get; private set; }
        public ListCollectionView Reports { get; private set; }

        public string PageTitle
        {
            get { return "Semi-automatic reports"; }
        }

        public string[] Role
        {
            get { return new[] { "" }; }
        }

        public int Order
        {
            get { return 100; }
        }

        #endregion

        #region commands
        public DelegateCommand GenerateCommand { get; private set; }

        private async void ExecuteGenerate()
        {
            string folder = OutputFolder;
            var report = Reports.CurrentItem as ReportInfo;
            if (report == null)
            {
                ReportMessage = "No report selected ";
                RaisePropertyChanged(nameof(ReportMessage));
                return;
            }
            isWorking = true;
            GenerateCommand.RaiseCanExecuteChanged();

            ReportMessage = "Generating... ";
            RaisePropertyChanged(nameof(ReportMessage));
            ReportProgress = 30;
            RaisePropertyChanged(nameof(ReportProgress));
            foreach (var p in report.Parameters)
            {
                Debug.WriteLine(string.Format("parameter {0}: :{1}", p.Name, p.Value));
                report.SetInputParameter(p.Name, p.Value);
            }
            string filename = string.Format("{0}.xlsx", report.Name);
            await doReport(report, folder, filename);
            ReportMessage = "Report generated";// +message;
            ReportProgress = 100;
            RaisePropertyChanged(nameof(ReportMessage));
            RaisePropertyChanged(nameof(ReportProgress));
            isWorking = false;
            GenerateCommand.RaiseCanExecuteChanged();
        }

        public bool CanExecuteGenerate()
        {
            if (Reports == null) return false;
            var report = Reports.CurrentItem as ReportInfo;
            return !isWorking && report != null;
        }
        #endregion

        #region methods
        void Reports_CurrentChanged(object sender, EventArgs e)
        {
            var report = Reports.CurrentItem as ReportInfo;
            if (report == null) return;
        }

        private Task doReport(ReportInfo report, string outputfolder, string filename)
        {
            return reportingService.GenerateReportAsync(report, outputfolder);
        }

        public async void Refresh()
        {
            var reports = await this.reprepo.GetAllAsync();
            //foreach (var r in reports)
            //{
            //    Console.WriteLine(r.Name);
            //}
            allReports = new List<ReportInfo>(reports);
            Reports = new ListCollectionView(allReports);
            Reports.CurrentChanged += Reports_CurrentChanged;
            RaisePropertyChanged(nameof(Reports));
            RaisePropertyChanged(nameof(ReportMessage));
            GenerateCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region other
        public string getDefaultMessage()
        {
            //return "Output folder: " + PrefillFolder + "\\Reports";
            return "Output folder: \\Reports";
        }

        void statsService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "IsReady" && statsService.IsReady)
            //{
            //    ModelFrag results = (ModelFrag)statsService.Results;
            //    //dialogService.ShowTaskDialog("General Statistics", "", News.BreakingNews);
            //}
            //else if (e.PropertyName == "IsWorking")
            //{
            //    GenerateCommand.RaiseCanExecuteChanged();
            //}
            //else if (e.PropertyName == "Progress")
            //{
            //    ReportProgress = statsService.Progress;
            //    ReportMessage = statsService.ProgressState;
            //    OnPropertyChanged("ReportProgress");
            //    OnPropertyChanged("ReportMessage");
            //}
        }
        #endregion

    }
}
