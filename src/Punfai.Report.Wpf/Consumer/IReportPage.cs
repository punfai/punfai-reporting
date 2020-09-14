namespace Punfai.Report.Wpf.Consumer
{
    public interface IReportPage
    {
        string PageTitle { get; }
        string[] Role { get; }
        int Order { get; }
        void Refresh();
    }

}
