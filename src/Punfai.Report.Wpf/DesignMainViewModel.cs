using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

using Prism.Mvvm;
using Prism.Commands;
using Punfai.Report.Interfaces;


namespace Punfai.Report.Wpf
{
    public class DesignMainViewModel : BindableBase
    {
        private readonly IReportRepository reprepo;
        private readonly IReportingService reportingService;
        private readonly IReportDesignerService designerService;
        public enum ScreenStates { HomeState, SelectReportTypeState, SelectDesignerState, EditWithDesignerState, DesignerState }

        public DesignMainViewModel()
        {
            Message = "Message goes here";
            ScreenState = ScreenStates.HomeState;
            Reports = new ObservableCollection<ReportInfo>();
            var arg1 = new ReportInfo.Builder(1, "OpenOffice Document")
                .Author("Jim")
                .Dependencies("RadEnterprise")
                .Description("Lists the top ranking products for the year")
                .Name("Top Products")
                .Parameters("startDate:2012-01-01;endDate:2012-12-31")
                .Script("screen.output('hello world')")
                .Build();
            Reports.Add(arg1);
            Reports.Add(arg1.Copy());
        }

        public DesignMainViewModel(IReportingService reportingService, IReportDesignerService designerService, IReportRepository reprepo)
        {
            this.reportingService = reportingService;
            this.designerService = designerService;
            this.reprepo = reprepo;
            CreateCommand = new DelegateCommand(ExecuteCreate);
            CreateWithCommand = new DelegateCommand<object>(ExecuteCreateWith);
            EditCommand = new DelegateCommand(ExecuteEdit, CanExecuteEdit);
            DeleteCommand = new DelegateCommand(ExecuteDelete, CanExecuteDelete);
            BackCommand = new DelegateCommand(ExecuteBack);
            ReportTypesMessage = "Installed Report Types";
            ScreenState = ScreenStates.HomeState;
            ReportTypesCV = CollectionViewSource.GetDefaultView(reportingService.AvailableReportTypes);
            DesignersCV = CollectionViewSource.GetDefaultView(designerService.AvailableReportDesigners);
            ReportTypesCV.MoveCurrentTo(null);
            DesignersCV.MoveCurrentTo(null);
            ReportTypesCV.CurrentChanged += ReportTypesCV_CurrentChanged;
        }

        void ReportTypesCV_CurrentChanged(object sender, EventArgs e)
        {
            if (ScreenState == ScreenStates.SelectReportTypeState || ScreenState == ScreenStates.SelectDesignerState)
            {
                if (ReportTypesCV.CurrentItem != null)
                    ExecuteCreate();
            }
        }

        #region properties

        public string Message { get; private set; }
        public string ReportTypesMessage { get; private set; }
        public IEnumerable<IReportDesigner> PossibleDesigners { get; private set; }
        public ICollectionView ReportTypesCV { get; private set; }
        public ICollectionView DesignersCV { get; private set; }
        public IReportDesigner ActiveDesigner { get; private set; }
        public ScreenStates ScreenState { get; private set; }
        public ObservableCollection<ReportInfo> Reports { get; private set; }

        private ReportInfo reportToEdit;

        #endregion

        #region commands

        public DelegateCommand CreateCommand { get; private set; }
        public DelegateCommand<object> CreateWithCommand { get; private set; }
        public DelegateCommand EditCommand { get; private set; }
        public DelegateCommand DeleteCommand { get; private set; }
        public DelegateCommand BackCommand { get; private set; }

        private void ExecuteCreate()
        {
            reportToEdit = null;
            var metaF = ReportTypesCV.CurrentItem as IReportType;
            if (metaF == null)
            {
                ReportTypesMessage = "Select a report type...";
                ScreenState = ScreenStates.SelectReportTypeState;
                raise();
            }
            else
            {
                ReportTypesMessage = "Installed Report Types";
                var possibleD = designerService.AvailableReportDesigners.Where(d => d.DocumentType == "*" || d.DocumentType == metaF.DocumentType);

                if (possibleD.Count() != 1)
                {
                    PossibleDesigners = possibleD;
                    ScreenState = ScreenStates.SelectDesignerState;
                    raise();
                }
                else
                {
                    var metaD = possibleD.First();
                    ExecuteCreateWith(metaD);
                    // calls raise()
                }
            }
        }
        private bool CanExecuteCreate()
        {
            return true;
        }
        private async void ExecuteCreateWith(object arg)
        {
            if (reportToEdit != null)
            {
                //do an edit with this designer, not a create
                EditWith(arg);
                return;
            }
            // get and set up the right filler and designer
            var metaF = ReportTypesCV.CurrentItem as IReportType;
            var metaD = arg as IReportDesigner;
            var designer = designerService.GetDesigner(metaD.Name); // refactored the meta out of here, a bit unnecessary, but if you ever switch back...
            PossibleDesigners = null;
            DesignersCV.MoveCurrentTo(metaD);
            // create report instance and load with designer
            ReportInfo rep = await reprepo.CreateNewAsync(metaF.Name, "New report");
            Reports.Insert(0, rep);
            designer.Load(rep);
            ActiveDesigner = designer;

            ScreenState = ScreenStates.DesignerState;
            raise();
        }
        private void ExecuteEdit()
        {
            var cv = CollectionViewSource.GetDefaultView(Reports);
            var r = cv.CurrentItem as ReportInfo;
            if (r == null) return;
            reportToEdit = r;
            //News.AddMessage("current item: {0}, {1}", r.Name, r.ReportType);
            // get and set up the right filler and designer
            var metaF = reportingService.AvailableReportTypes.Where(f => f.Name == r.ReportType).FirstOrDefault();
            if (metaF == null)
            {
                Message = "Problem: Report Type ({0}) is not installed.";
                raise();
                return;
            }
            var possibleD = designerService.AvailableReportDesigners.Where(d => d.DocumentType == "*" || d.DocumentType == metaF.DocumentType);
            if (possibleD.Count() != 1)
            {
                PossibleDesigners = possibleD;
                ScreenState = ScreenStates.EditWithDesignerState;
                raise();
            }
            else
            {
                var metaD = possibleD.First();
                EditWith(metaD);
                // calls raise()
            }
        }
        private async void EditWith(object arg)
        {
            var metaD = arg as IReportDesigner;
            var designer = designerService.GetDesigner(metaD.Name);
            PossibleDesigners = null;
            DesignersCV.MoveCurrentTo(metaD);
            // we don't have the script or template yet. get the script now.
            reportToEdit.Script = await reprepo.GetScriptAsync(reportToEdit.ID);
            designer.Load(reportToEdit);
            ActiveDesigner = designer;
            ScreenState = ScreenStates.DesignerState;
            raise();
            reportToEdit = null;
        }
        private bool CanExecuteEdit()
        {
            return CanExecuteDelete();
        }
        private void ExecuteDelete()
        {
            var cv = CollectionViewSource.GetDefaultView(Reports);
            var r = cv.CurrentItem as ReportInfo;
            if (r == null) throw new Exception("No report selected");
            //reprepo.Delete(r.ID.ToString(), MeshReportSchema.MReport, MeshReportSchema.DB)
            Reports.Remove(r);
            cv.MoveCurrentTo(null);
            raise();
        }
        private bool CanExecuteDelete()
        {
            if (Reports == null) return false;
            var cv = CollectionViewSource.GetDefaultView(Reports);
            if (cv == null) return false;
            return cv.CurrentItem != null;
        }
        private async void ExecuteBack()
        {
            await reprepo.SaveAsync(ActiveDesigner.ReportDefinition);
            await reprepo.SaveScriptAsync(ActiveDesigner.ReportDefinition.ID, ActiveDesigner.ReportDefinition.Script);

            int index = Reports.IndexOf(ActiveDesigner.ReportDefinition);
            if (index != -1)
            {
                Reports.RemoveAt(index);
                Reports.Insert(index, ActiveDesigner.ReportDefinition);
            }

            ActiveDesigner = null;
            ScreenState = ScreenStates.HomeState;
            ReportTypesCV.MoveCurrentTo(null);
            OnPropertyChanged(() => Reports);
            raise();
            //loadReports();
        }

        #endregion

        public void OnNavigatedTo()
        {
            loadReports();
        }

        #region methods

        private async void loadReports()
        {
            if (Reports != null)
            {
                var cv0 = CollectionViewSource.GetDefaultView(Reports);
                cv0.CurrentChanged -= cv_CurrentChanged;
            }
            Reports = null;
            OnPropertyChanged(() => Reports);
            var reports = await reprepo.GetAllAsync();
            Reports = new ObservableCollection<ReportInfo>(reports);
            var cv = CollectionViewSource.GetDefaultView(Reports);
            cv.CurrentChanged += cv_CurrentChanged;
            OnPropertyChanged(() => Reports);
        }

        void cv_CurrentChanged(object sender, EventArgs e)
        {
            EditCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }

        private void raise()
        {
            OnPropertyChanged(() => Message);
            OnPropertyChanged(() => ReportTypesMessage);
            OnPropertyChanged(() => PossibleDesigners);
            OnPropertyChanged(() => ActiveDesigner);
            OnPropertyChanged(() => ScreenState);
            EditCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }

        #endregion

    }

}
