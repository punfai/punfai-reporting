using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

using System.Text;
using System.IO;
using Punfai.Report.Wpf;

using Prism.Mvvm;
using Prism.Commands;

namespace Punfai.Report.Wpf.Consumer
{
    public class ReportViewModel : BindableBase
    {
        public ReportViewModel()
        {
            this.Pages = new List<IReportPage>();
            var page = new RegularConsumerViewModel();
            Pages.Add(page);
        }
        //private readonly IEnumerable<Lazy<IReportPage, IReportPageMetadata>> reports;
        public ReportViewModel(List<IReportPage> reportPages)
        {
            this.Pages = reportPages;

            // for maximum efficiency, don't load the View, load a special IReportPossibilityInfo that has a Lazy<IReportPage> 
            // in the same way that IConfigScreen works. But in the mean time, load them all.
            //int reportCount = importedReports.Count();
            //this.reports = importedReports.Where(a => pump.User.IsInAnyRole(a.Metadata.Role)).OrderBy(b => b.Metadata.Order);

            //if (reports.Count() > 1)
            //{
            //    // not -> (don't) show the regular report page if the app supplies custom reporting pages
            //    // the default consumer report page is visible only to PrimaryRoles.SuperUser and ReportingRoles.ReportConsumer
            //    // so if you don't want it, don't use the ReportConsumerRole. Because it's cool, the SuperUser won't mind having access to it.
            //    //reports = reports.Where(a => a.Metadata.PageTitle != Constants.DefaultConsumerReportPageTitle);
            //}
            //lazyPages = new List<LazyPageViewModel>();
            //foreach (var r in reports)
            //    lazyPages.Add(new LazyPageViewModel(r.Metadata.PageTitle, r));
            //ReportTitles = new ListCollectionView(lazyPages);
            //ReportTitles.CurrentChanged += ReportTitles_CurrentChanged;
            //ReportTitles.MoveCurrentToFirst();

        }

        public List<IReportPage> Pages { get; private set; }

        #region methods
        //void ReportTitles_CurrentChanged(object sender, EventArgs e)
        //{
        //    var lazyPage = ReportTitles.CurrentItem as LazyPageViewModel;
        //    if (!lazyPage.HasLoaded)
        //    {
        //        lazyPage.LoadCommand.Execute();
        //    }
        //    else
        //    {
        //        lazyPage.PageView.Refresh();
        //    }
        //}

        //List<LazyPageViewModel> lazyPages;
        //public ICollectionView ReportTitles { get; private set; }

        //public IReportPage SelectReportPage(string pageTitle)
        //{
        //    var reportpage = reports.FirstOrDefault(page => page.Metadata.PageTitle == pageTitle);
        //    if (reportpage == null) return null;
        //    return reportpage.Value;
        //}
        #endregion


        public void RefreshPages()
        {
            Pages.ForEach(page => page.Refresh());
        }

    }
    public class LazyPageViewModel : BindableBase
    {
        private readonly Lazy<IReportPage> lazyPage;
        public LazyPageViewModel(string pageTitle, Lazy<IReportPage> lazyPage)
        {
            this.lazyPage = lazyPage;
            this.PageTitle = pageTitle;
            HasLoaded = false;
            PageView = null;
            LoadCommand = new DelegateCommand(ExecuteLoad);
        }
        public string PageTitle { get; private set; }
        public IReportPage PageView { get; private set; }
        public bool HasLoaded { get; private set; }

        public DelegateCommand LoadCommand { get; private set; }

        private void ExecuteLoad()
        {
            PageView = lazyPage.Value;
            HasLoaded = true;
            RaisePropertyChanged(nameof(HasLoaded));
            RaisePropertyChanged(nameof(PageView));
        }

    }
}
