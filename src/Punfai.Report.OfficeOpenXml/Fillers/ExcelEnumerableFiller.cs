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
using DocumentFormat.OpenXml;

namespace Punfai.Report.OfficeOpenXml.Fillers
{
    public class ExcelEnumerableFiller : IReportFiller
    {
        public Type[] SupportedReports { get { return new[] { typeof(ExcelReportType) }; } }

        public async Task<bool> FillAsync(ITemplate t, IDictionary<string, dynamic> stuffing, Stream output)
        {
            dynamic rows = stuffing.Values.FirstOrDefault(a => a is IEnumerable<object>);
            if (rows == null) return false;
            //using (Stream docstream = new MemoryStream())
            using (SpreadsheetDocument doc = SpreadsheetDocument.Create(output, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document.
                WorkbookPart workbookpart = doc.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                #region styles
                // Add styles
                var stylesPart = workbookpart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = new Stylesheet();

                // number formats, date is a formatted number
                //var numFmt = @"<numFmts count=""1""><numFmt numFmtId=""166"" formatCode=""[$-F800]dddd\,\ mmmm\ dd\,\ yyyy""/></numFmts>";
                var numFmtDate = new NumberingFormat();
                numFmtDate.NumberFormatId = 166;
                numFmtDate.FormatCode = @"[$-F800]dddd\,\ mmmm\ dd\,\ yyyy";
                stylesPart.Stylesheet.NumberingFormats = new NumberingFormats();
                stylesPart.Stylesheet.NumberingFormats.Count = 1;
                stylesPart.Stylesheet.NumberingFormats.AppendChild(numFmtDate);

                // blank font list
                stylesPart.Stylesheet.Fonts = new Fonts();
                stylesPart.Stylesheet.Fonts.Count = 1;
                stylesPart.Stylesheet.Fonts.AppendChild(new Font());

                // create fills
                stylesPart.Stylesheet.Fills = new Fills();

                // create a solid red fill
                var solidRed = new PatternFill() { PatternType = PatternValues.Solid };
                solidRed.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("FFFF0000") }; // red fill
                solidRed.BackgroundColor = new BackgroundColor { Indexed = 64 };

                stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // required, reserved by Excel
                stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // required, reserved by Excel
                stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = solidRed });
                stylesPart.Stylesheet.Fills.Count = 3;

                // blank border list
                stylesPart.Stylesheet.Borders = new Borders();
                stylesPart.Stylesheet.Borders.Count = 1;
                stylesPart.Stylesheet.Borders.AppendChild(new Border());

                // blank cell format list
                stylesPart.Stylesheet.CellStyleFormats = new CellStyleFormats();
                stylesPart.Stylesheet.CellStyleFormats.Count = 1;
                stylesPart.Stylesheet.CellStyleFormats.AppendChild(new CellFormat());

                // cell format list
                stylesPart.Stylesheet.CellFormats = new CellFormats();
                // empty one for index 0, seems to be required
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat());
                // cell format index 1 applies the date format
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 0, BorderId = 0, FillId = 0, NumberFormatId = 166, ApplyNumberFormat = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center });
                //stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 0, BorderId = 0, FillId = 2, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center });
                stylesPart.Stylesheet.CellFormats.Count = 2;

                //stylesPart.Stylesheet.Save();
                #endregion

                // Add a WorksheetPart to the WorkbookPart.
                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Add Sheets to the Workbook.
                Sheets sheets = doc.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                // Append a new worksheet and associate it with the workbook. 
                // Workbook -> Sheets -> Sheet 
                Sheet sheet = new Sheet()
                {
                    Id = doc.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Sheet1"
                };
                sheets.Append(sheet);

                // Get the SharedStringTablePart. If it does not exist, create a new one.
                SharedStringTablePart sharedStringPart = ExcelTemplateTool.GetSharedStringTablePart(doc);
                
                // don't let it confuse you
                // doc -> WorkbookPart -> WorksheetPart -> WorkSheet -> SheetData
                SheetData sheetdata = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                foreach (var datarow in rows)
                {
                    IEnumerable<object> cells = datarow as IEnumerable<object>;
                    if (cells == null) continue;
                    Row contentRow = new Row();
                    //int index = 10;
                    //contentRow.RowIndex = (UInt32)index;
                    foreach (var cellObj in cells)
                    {
                        Cell c;
                        if (cellObj == null)
                            c = CreateEmptyCell();
                        else if (cellObj is string)
                            c = CreateSharedStringCell((string)cellObj, sharedStringPart);
                        else if (cellObj is DateTime)
                            c = CreateDateTimeCell((DateTime)cellObj);
                        else if (cellObj.GetType().GetTypeInfo().IsValueType)
                            c = CreateValueCell(cellObj);
                        else
                            c = CreateInlineStringCell(cellObj.ToString());
                        contentRow.AppendChild(c);
                    }
                    sheetdata.AppendChild(contentRow);
                }

                workbookpart.Workbook.Save();
            }
            return true;
        }
        private static Cell CreateEmptyCell()
        {
            Cell c = new Cell();
            return c;
        }
        private static Cell CreateInlineStringCell(string text)
        {
            Cell c = new Cell();
            c.DataType = CellValues.InlineString;
            InlineString inlineString = new InlineString();
            Text t = new Text();
            t.Text = text;
            inlineString.AppendChild(t);
            c.AppendChild(inlineString);
            return c;
        }
        private static Cell CreateSharedStringCell(string text, SharedStringTablePart part)
        {
            int index = ExcelTemplateTool.InsertSharedStringItem(text, part);
            Cell cell = new Cell();
            cell.CellValue = new CellValue(index.ToString());
            cell.DataType = CellValues.SharedString;
            return cell;
        }
        private static Cell CreateValueCell(object obj)
        {
            Cell cell = new Cell();
            CellValue cv = new CellValue();
            cv.Text = obj.ToString();
            cell.Append(cv);
            return cell;
        }
        private static Cell CreateDateTimeCell(DateTime d)
        {
            Cell cell = new Cell();
            CellValue cv = new CellValue();
            cv.Text = ToOADate(d).ToString();
            cell.StyleIndex = 1; // date style
            cell.Append(cv);
            return cell;
        }

        private static double ToOADate(DateTime d)
        {
            var span = d - new DateTime(1899, 11, 30);
            return span.TotalDays;
        }
    }

}
