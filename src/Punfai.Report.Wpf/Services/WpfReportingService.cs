using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Punfai.Report.Interfaces;
using System.Threading.Tasks;

namespace Punfai.Report.Wpf
{
    /// <summary>
    /// Not sure why there is a IReportingService, it might be a good guide for tying everything together.
    /// But this WpfReportingService builds on Punfai.Report.ReportingService by giving some easy methods that automatically write
    /// to a smartly named file in the Output folder. Because the PCL doesn't do files (!?!)
    /// And it's up to the end user what they do with the generated output, 
    /// this is a simple example implementation, use it, or make a better one that suits.
    /// Why not inherit ReportingService? Get with the program dude, it's time to compose.
    /// </summary>
    public class WpfReportingService : IReportingService
    {
        private readonly IReportRepository reprepo;
        private readonly IReportingService baseService;

        public WpfReportingService(IReportRepository reprepo, IReportingService baseService)
        {
            this.reprepo = reprepo;
            this.baseService = baseService;
        }

        public async Task<Dictionary<string, dynamic>> GenerateReportAsync(ReportInfo report, string outputFolder, Stream stdout = null, bool closeStream = true)
        {
            var stuffing = await RunScriptAsync(report, stdout);
            await WriteReportsAsync(report, stuffing, outputFolder, stdout);
            return stuffing;
        }
        //imm: the immutable way ?
        //public async Task<Dictionary<string, dynamic>> WriteReportsAsync(ReportInfo report, Dictionary<string, dynamic> stuffing, string outputFolder, Stream stdout = null)
        /// <summary>
        /// handles single documents, multiple documents in a folder, or all that in many folders.
        /// The passed in stuffing is updated with actual paths used
        ///   folder: overwritten
        ///   file: unchanged
        ///   filePath: full path to the generated file
        /// The folder and file dictionaries and lists are converted to List[Dictionary[string,dynamic]]
        /// for simpler management if you need to read it and post-process the generated files.
        /// The deeper lists and dictionaries below the single document level are untouched.
        /// </summary>
        /// <param name="report"></param>
        /// <param name="stuffing"></param>
        /// <param name="outputFolder"></param>
        /// <param name="stdout"></param>
        /// <returns></returns>
        public async Task WriteReportsAsync(ReportInfo report, Dictionary<string, dynamic> stuffing, string outputFolder, Stream stdout = null)
        {
            if (stuffing.ContainsKey("folders"))
            {
                // recurse into MULTIPLE document generation, for MULTIPLE folders
                /*
                 *  [folders]
                 *      [dic]
                 *          [folder]
                 *          [files]
                 *      [dic]...
                 */
                //imm: Dictionary<string, dynamic> newResult = new Dictionary<string, dynamic>();
                List<Dictionary<string, dynamic>> newFoldersList = new List<Dictionary<string, dynamic>>();
                var folders = stuffing["folders"] as IEnumerable<object>;
                foreach (var folderDic in folders)
                {
                    var goodDic = getDictionary(folderDic);
                    await WriteReportsAsync(report, goodDic, outputFolder, stdout);
                    newFoldersList.Add(goodDic);
                }
                stuffing["folders"] = newFoldersList;
                //imm: newResult["folders"] = newFoldersList;
            }
            // convention: if there is a ["folder"] of string, and a ["files"] of IEnumerable<IDictionary<?,?>> then generate many files in to folder
            else if (stuffing.ContainsKey("folder") && stuffing.ContainsKey("files"))
            {
                // MULTIPLE document generation for a single script run
                IReportType rt = GetReportType(report.ReportType);
                byte[] templateBytes = await reprepo.GetTemplateAsync(report.ID);
                if (templateBytes == null) throw new Exception("Can't generate a report with no template.");
                var t = rt.CreateTemplate(templateBytes);

                List<Dictionary<string, dynamic>> newFilesList = new List<Dictionary<string, dynamic>>();
                string folder = safeFolder((string)stuffing["folder"]);
                var files = stuffing["files"] as IEnumerable<object>; // the list of single document context dictionaries
                foreach (var f1 in files)
                {
                    var dic = getDictionary(f1);
                    string preferredFilename = null;
                    // convention: ["file"] is the desired file name (required)
                    if (dic.ContainsKey("file")) preferredFilename = dic["file"] as string;
                    if (preferredFilename == null) throw new Exception("[file] not set");
                    dic["filePath"] = await writeReportAsync(t, rt, dic, preferredFilename, outputFolder, folder);
                    newFilesList.Add(dic);
                }
                stuffing["folder"] = folder; // safe folder
                stuffing["files"] = newFilesList; // standardised list of dic
            }
            else
            {
                // SINGLE document generation for a single script run
                string preferredFilename = null;
                if (stuffing.ContainsKey("file")) preferredFilename = stuffing["file"] as string;
                // allow auto name if just one document
                IReportType rt = GetReportType(report.ReportType);
                byte[] templateBytes = await reprepo.GetTemplateAsync(report.ID);
                if (templateBytes == null) throw new Exception("Can't generate a report with no template.");
                var t = rt.CreateTemplate(templateBytes);
                if (preferredFilename == null) preferredFilename = report.Name + "." + rt.DocumentType;
                stuffing["filePath"] = await writeReportAsync(t, rt, stuffing, preferredFilename, outputFolder);
            }
        }

        private async Task<string> writeReportAsync(ITemplate t, IReportType rt, Dictionary<string, dynamic> stuffing, string preferredFilename, string outputFolder, string subfolder = null)
        {
            if (string.IsNullOrWhiteSpace(preferredFilename)) throw new ArgumentException("give me a valid preferredFilename");
            // find a valid output file path
            string outputPath = PrepareOutputFilePath(preferredFilename, outputFolder, subfolder);
            using (var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                bool ok = await FillReportAsync(t, rt, stuffing, stream);
                stream.Flush();
            }
            return outputPath;
        }
        /// <summary>
        /// We could be dealing with a few different dics.
        /// Python dic [object, object]
        /// clr Dictionary<string, object>
        /// </summary>
        /// <param name="dic">some foreign dic</param>
        /// <returns>A known dynamic dic keyed with strings</returns>
        private Dictionary<string, dynamic> getDictionary(object dic)
        {
            var f1 = dic as Dictionary<string, object>;
            if (f1 != null) return f1;
            var f2 = dic as IDictionary<string, object>;
            if (f2 != null)
            {
                var d = new Dictionary<string, dynamic>();
                foreach (var pair in f2)
                    d.Add(pair.Key, pair.Value);
                return d;
            }
            var f3 = dic as IDictionary<object, object>;
            if (f3 != null)
            {
                var d = new Dictionary<string, dynamic>();
                foreach (var pair in f3)
                    d.Add(pair.Key.ToString(), pair.Value);
                return d;
            }
            throw new Exception("not a known dictionary");
        }
        /// <summary>
        /// Returns a unique filename "outputFolder\subfolder\preferredFilename_N.ext"
        /// where N is a suffix appended to ensure existing files are not overwritten.
        /// </summary>
        /// <param name="preferredFilename"></param>
        /// <param name="outputFolder"></param>
        /// <param name="subfolder"></param>
        /// <returns></returns>
        public static string PrepareOutputFilePath(string preferredFilename, string outputFolder, string subfolder = null)
        {
            string ParentDir = outputFolder;
            //else ParentDir = "%Desktop%";
            if (ParentDir.Contains("%Desktop%"))
                ParentDir = ParentDir.Replace("%Desktop%", System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            string TargetDir;
            if (subfolder != null) {
                TargetDir = Path.Combine(ParentDir, safeFolder(subfolder));
            }
            else TargetDir = ParentDir;
            DirectoryInfo directoryInfo = new DirectoryInfo(ParentDir);
            if (!directoryInfo.Exists)
                directoryInfo.Create();
            DirectoryInfo directoryInfo1 = new DirectoryInfo(TargetDir);
            if (!directoryInfo1.Exists)
                directoryInfo1.Create();

            string outFileBase = Path.Combine(TargetDir, safeFile(preferredFilename));
            if (outFileBase.EndsWith(".")) outFileBase = outFileBase + "1";
            int dotpos = outFileBase.LastIndexOf(".");
            string ext = null;
            if (dotpos > 1 && dotpos > outFileBase.LastIndexOf(Path.DirectorySeparatorChar))
            {
                ext = outFileBase.Substring(dotpos + 1);
                outFileBase = outFileBase.Substring(0, dotpos);
            }
            string outFile = outFileBase + "." + ext;
            int i = 1;
            while (System.IO.File.Exists(outFile))
            {
                outFile = outFileBase + "_" + i.ToString() + "." + ext;
                i++;
            }
            return outFile;
        }
        /// <summary>
        /// one level only, no path separators allowed here
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static string safeFolder(string folder)
        {
            char[] charsbad = Path.GetInvalidPathChars();
            var safename = new StringBuilder(folder);
            foreach (var bad in charsbad) safename.Replace(bad, '_');
            safename.Replace(Path.DirectorySeparatorChar, '_');
            safename.Replace(Path.AltDirectorySeparatorChar, '_');
            return safename.ToString();
        }
        private static string safeFile(string file)
        {
            // want a safe file name
            char[] charsbad = Path.GetInvalidFileNameChars();
            var safename = new StringBuilder(file);
            foreach (var bad in charsbad) safename.Replace(bad, '_');
            return safename.ToString();
        }

        #region call the base service
        public IEnumerable<IReportType> AvailableReportTypes { get { return baseService.AvailableReportTypes; } }

        public IReportType GetReportType(string reportTypeName)
        {
            return baseService.GetReportType(reportTypeName);
        }

        public Task<string> GenerateReportAsync(ReportInfo report, Stream output, Stream stdout = null, bool closeStream = true)
        {
            return baseService.GenerateReportAsync(report, output, stdout, closeStream);
        }
        public Task<Dictionary<string, dynamic>> RunScriptAsync(string scriptLanguage, IEnumerable<InputParameter> parameters, string script, Stream stdout = null)
        {
            return baseService.RunScriptAsync(scriptLanguage, parameters, script, stdout);
        }

        public Task<Dictionary<string, dynamic>> RunScriptAsync(ReportInfo report, Stream stdout = null)
        {
            return baseService.RunScriptAsync(report, stdout);
        }

        public Task<bool> FillReportAsync(ITemplate t, IReportType rt, Dictionary<string, dynamic> stuffing, Stream output)
        {
            return baseService.FillReportAsync(t, rt, stuffing, output);
        }

        public Task<string> GenerateReportAsync(string reportName, IDictionary<string, object> inputParams, Stream output, Stream stdout = null, bool closeStream = true)
        {
            return baseService.GenerateReportAsync(reportName, inputParams, output, stdout, closeStream);
        }

        public Task<string> GenerateReportAsync(string reportName, IEnumerable<(string, object)> inputParams, Stream output, Stream stdout = null, bool closeStream = true)
        {
            return baseService.GenerateReportAsync(reportName, inputParams, output, stdout, closeStream);
        }
        #endregion

    }
}
