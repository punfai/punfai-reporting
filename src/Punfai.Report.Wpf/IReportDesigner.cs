using Punfai.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Punfai.Report.Wpf
{
    public interface IReportDesigner
    {
        string Name { get; }
        /// <summary>
        /// '*' if it supports all types, or a comma separated list: 'xml,html,txt'
        /// </summary>
        string DocumentType { get; }
        void Load(ReportInfo specifiedReport);
        ReportInfo ReportDefinition { get; }
        event EventHandler Back;
    }

    public interface IReportDesignerService
    {
        IEnumerable<IReportDesigner> AvailableReportDesigners { get; }
        IReportDesigner GetDesigner(string designerName);
    }
}
