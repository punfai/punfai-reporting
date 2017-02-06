using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;
using System.Text.RegularExpressions;
using Punfai.Report.Interfaces;
using System.Threading.Tasks;
using Punfai.Report.OfficeOpenXml.ReportTypes;

namespace Punfai.Report.OfficeOpenXml.Fillers
{
    public class PowerPointFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new[] { typeof(PowerPointReportType) }; } }

        public Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            Console.WriteLine("Filling a template with {1}", this.GetType().Name);
            return Task.FromResult<bool>(true);
        }
    }

}
