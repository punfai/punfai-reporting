using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using System.Text.RegularExpressions;
using Punfai.Report.Interfaces;
using Punfai.Report.Utils;
using Punfai.Report.OfficeOpenXml.Utils;
using System.Threading.Tasks;
using Punfai.Report.OfficeOpenXml.ReportTypes;

namespace Punfai.Report.OfficeOpenXml.Fillers
{
    public class ExcelFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new[] { typeof(ExcelReportType) }; } }

        public Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            using (Stream docstream = new MemoryStream())
            {
                byte[] templateBytes = t.GetTemplateBytes();
                docstream.Write(templateBytes, 0, templateBytes.Length);
                docstream.Flush();
                docstream.Position = 0;
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(docstream, true))
                {
                    // shared string part
                    if (doc.WorkbookPart.SharedStringTablePart != null) doPart(doc.WorkbookPart.SharedStringTablePart, stuffing);
                    // some other part
                    if (doc.WorkbookPart.WorkbookStylesPart != null) doPart(doc.WorkbookPart.WorkbookStylesPart, stuffing);
                    // some other part
                    if (doc.WorkbookPart.CalculationChainPart != null) doPart(doc.WorkbookPart.CalculationChainPart, stuffing);

                    var worksheets = doc.WorkbookPart.WorksheetParts.ToList();
                    worksheets.ForEach(w =>
                    {
                        doPart(w, stuffing);
                    });
                }
                docstream.Position = 0;
                XmlTemplateTool.CopyStream(docstream, output);
            }
            return Task.FromResult<bool>(true);
        }
        private void doPart(OpenXmlPart part, IDictionary<string, dynamic> stuffing)
        {
            try
            {
                XDocument xdoc1 = part.GetXDocument();
                foreach (KeyValuePair<string, dynamic> pair in stuffing)
                    XmlTemplateTool.ReplaceKey(xdoc1.Root, pair.Key, pair.Value);
                part.PutXDocument(xdoc1);
            }
            catch (Exception ex) 
            { 
                //System.Diagnostics.Trace.TraceError("XmlFiller.Fill bad excel part {0}", ex); 
            }
        }
    }

    #region helper
    public class ExcelTemplateTool
    {
        public static void AddSomeRows(string filePath)
        {
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filePath, true))
            {
                WorksheetPart worksheet = doc.WorkbookPart.WorksheetParts.First();
                SheetData sheetdata = worksheet.Worksheet.GetFirstChild<SheetData>();
                Row contentRow = new Row();
                int index = 40;
                contentRow.RowIndex = (UInt32)index;
                Cell c = new Cell();
                c.CellReference = "A" + index;
                CellValue cv = new CellValue();
                cv.Text = "4";
                c.Append(cv);
                contentRow.AppendChild(c);
                contentRow.AppendChild(CreateTextCell("B", "programmed cell", index));
                sheetdata.AppendChild(contentRow);
            }
        }
        private static Cell CreateTextCell(string header, string text, int index)
        {
            //Create new inline string cell
            Cell c = new Cell();
            c.DataType = CellValues.InlineString;
            c.CellReference = header + index;
            //Add text to text cell
            InlineString inlineString = new InlineString();
            Text t = new Text();
            t.Text = text;
            inlineString.AppendChild(t);
            c.AppendChild(inlineString);
            return c;
        }
        public static void OpenAndAddToSpreadsheetStream(Stream stream)
        {
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(stream, true))
            {
                // Add a WorksheetPart.
                WorksheetPart newWorksheetPart = spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
                newWorksheetPart.Worksheet = new Worksheet(new SheetData());

                // Create Sheets object.
                Sheets sheets = spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                string relationshipId = spreadSheet.WorkbookPart.GetIdOfPart(newWorksheetPart);

                // Create a unique ID for the new worksheet.
                uint sheetId = 1;
                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                // Give the new worksheet a name.
                string sheetName = "mySheet" + sheetId;

                // Append the new worksheet and associate it with the workbook.
                Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
                sheets.Append(sheet);
            }
            // Caller must close the stream.
        }
        // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
        // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
        public static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }
        public static SharedStringTablePart GetSharedStringTablePart(SpreadsheetDocument doc)
        {
            // Get the SharedStringTablePart. If it does not exist, create a new one.
            SharedStringTablePart shareStringPart;
            if (doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
            {
                shareStringPart = doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
            }
            else
            {
                shareStringPart = doc.WorkbookPart.AddNewPart<SharedStringTablePart>();
            }
            return shareStringPart;
        }
    }
    #endregion
}
