using System.Collections.Generic;
using System.Linq;

namespace Punfai.Report.Wpf.Services
{
    public class ReportDesignerService : IReportDesignerService
    {
        private readonly List<IReportDesigner> designers = new List<IReportDesigner>();
        public ReportDesignerService(List<IReportDesigner> designers)
        {
            this.designers = designers;
        }

        public IEnumerable<IReportDesigner> AvailableReportDesigners
        {
            get
            {
                return designers;
            }
        }

        public IReportDesigner GetDesigner(string reportTypeName)
        {
            var designer = designers.FirstOrDefault(item => item.Name == reportTypeName) as IReportDesigner;
            return designer;
        }
    }
}
