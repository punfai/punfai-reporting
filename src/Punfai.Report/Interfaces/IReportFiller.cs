using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Punfai.Report
{
    /// <summary>
    /// Evolving onwards from IFormFiller
    /// Part of the Report Scripting Engine
    /// See MeshPrism.Report for reference UI implementaion
    /// </summary>
    public interface IReportFiller
    {
        Type[] SupportedReports { get; }
        /// <summary>
        /// It's not your job to close the output stream OK?!
        /// </summary>
        /// <param name="t"></param>
        /// <param name="stuffing"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output);
        string LastError { get; }
    }
}
