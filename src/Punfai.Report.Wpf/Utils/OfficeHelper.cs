//using Microsoft.Office.Interop.PowerPoint;
//using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Punfai.Report.Wpf.Utils
{
    public class OfficeHelper
    {
        //public static void ConvertPPTXtoPDF(string source, string target)
        //{
        //    ConvertPPTXtoFixed(source, target, PpFixedFormatType.ppFixedFormatTypePDF);
        //}
        //public static void ConvertPPTXtoXPS(string source, string target)
        //{
        //    ConvertPPTXtoFixed(source, target, PpFixedFormatType.ppFixedFormatTypeXPS);
        //}
        //public static void ConvertWordToXPS(List<string> files)
        //{
        //    ConvertWordToFixed(files, WdExportFormat.wdExportFormatXPS);
        //}
        //public static void ConvertWordToPDF(List<string> files)
        //{
        //    ConvertWordToFixed(files, WdExportFormat.wdExportFormatPDF);
        //}
        ///// <summary>
        ///// Convert pptx to PDF/XPS (install office PDF/XPS addin)
        ///// </summary>
        //private static void ConvertPPTXtoFixed(string source, string target, PpFixedFormatType fixedFormatType)
        //{
        //    Microsoft.Office.Interop.PowerPoint.Application ppApp = new Microsoft.Office.Interop.PowerPoint.Application();
        //    Microsoft.Office.Interop.PowerPoint.Presentation presentation = ppApp.Presentations.Open(source, Microsoft.Office.Core.MsoTriState.msoTrue, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse);
        //    presentation.ExportAsFixedFormat(target,
        //        fixedFormatType,
        //        PpFixedFormatIntent.ppFixedFormatIntentPrint,
        //        Microsoft.Office.Core.MsoTriState.msoFalse,
        //        PpPrintHandoutOrder.ppPrintHandoutHorizontalFirst,
        //        PpPrintOutputType.ppPrintOutputSlides,
        //        Microsoft.Office.Core.MsoTriState.msoFalse,
        //        null,
        //        PpPrintRangeType.ppPrintAll,
        //        "",
        //        false,
        //        false,
        //        false,
        //        true,
        //        true,
        //        System.Reflection.Missing.Value);
        //    presentation.Close();
        //    presentation = null;
        //    ppApp = null;
        //    GC.Collect();
        //}
        //private static bool ConvertWordToFixed(List<string> files, WdExportFormat fixedFormatType)
        //{
        //    Microsoft.Office.Interop.Word.Application wordApplication;
        //    try
        //    {
        //        wordApplication = new Microsoft.Office.Interop.Word.Application();
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }

        //    Document wordDocument = null;
        //    object paramMissing = Type.Missing;
        //    WdExportFormat paramExportFormat = fixedFormatType;
        //    bool paramOpenAfterExport = false;
        //    WdExportOptimizeFor paramExportOptimizeFor =
        //        WdExportOptimizeFor.wdExportOptimizeForPrint;
        //    WdExportRange paramExportRange = WdExportRange.wdExportAllDocument;
        //    int paramStartPage = 0;
        //    int paramEndPage = 0;
        //    WdExportItem paramExportItem = WdExportItem.wdExportDocumentContent;
        //    bool paramIncludeDocProps = true;
        //    bool paramKeepIRM = true;
        //    WdExportCreateBookmarks paramCreateBookmarks =
        //        WdExportCreateBookmarks.wdExportCreateWordBookmarks;
        //    bool paramDocStructureTags = true;
        //    bool paramBitmapMissingFonts = true;
        //    bool paramUseISO19005_1 = false;
        //    string targetExt = fixedFormatType == WdExportFormat.wdExportFormatPDF ? ".pdf" : ".xps";
        //    foreach (string file in files)
        //    {
        //        object paramSourceDocPath = file;
        //        string paramExportFilePath = file.Substring(0, file.LastIndexOf('.')) + targetExt;
        //        try
        //        {
        //            // Open the source document.
        //            wordDocument = wordApplication.Documents.Open(
        //                ref paramSourceDocPath);
        //            /*, ref paramMissing, ref paramMissing,
        //                ref paramMissing, ref paramMissing, ref paramMissing,
        //                ref paramMissing, ref paramMissing, ref paramMissing,
        //                ref paramMissing, ref paramMissing, ref paramMissing,
        //                ref paramMissing, ref paramMissing, ref paramMissing,
        //                ref paramMissing);*/

        //            // Export it in the specified format.
        //            if (wordDocument != null)
        //                wordDocument.ExportAsFixedFormat(paramExportFilePath,
        //                    paramExportFormat, paramOpenAfterExport,
        //                    paramExportOptimizeFor, paramExportRange, paramStartPage,
        //                    paramEndPage, paramExportItem, paramIncludeDocProps,
        //                    paramKeepIRM, paramCreateBookmarks, paramDocStructureTags,
        //                    paramBitmapMissingFonts, paramUseISO19005_1,
        //                    ref paramMissing);
        //        }
        //        catch (Exception ex)
        //        {
        //            return false;
        //        }
        //        finally
        //        {
        //            // Close and release the Document object.
        //            if (wordDocument != null)
        //            {
        //                wordDocument.Close(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges, ref paramMissing, ref paramMissing);
        //                //wordDocument.Close(ref paramMissing, ref paramMissing, ref paramMissing);
        //                wordDocument = null;
        //            }
        //        }
        //    }
        //    // Quit Word and release the ApplicationClass object.
        //    if (wordApplication != null)
        //    {
        //        wordApplication.Quit(ref paramMissing, ref paramMissing,
        //            ref paramMissing);
        //        wordApplication = null;
        //    }

        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //    return true;
        //}
    }

}
