using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Punfai.Report.Fillers;
using Punfai.Report.ReportTypes;
using Punfai.Report.Template;
using System.Dynamic;
using System.IO;
using System.Reflection;
using Punfai.Report;
using Punfai.Report.Wpf.ReportTypes;
using Punfai.Report.OfficeOpenXml.ReportTypes;

namespace PunReportNet47Test
{
    public class ExcelTests
    {
        [Fact]
        public async Task Excel_filler_enumerates_and_fills_all_the_types()
        {
            Punfai.Report.OfficeOpenXml.Fillers.ExcelEnumerableFiller filler = new Punfai.Report.OfficeOpenXml.Fillers.ExcelEnumerableFiller();
            Dictionary<string, object> stuffing = new Dictionary<string, object>();
            List<object> rows = new List<object>();
            List<string> header = new List<string>();
            header.AddRange(new string[] {
                "Date"
                ,"Id"
                ,"Name"
                ,"Start"
                ,"Finish"
                ,"Lunch"
                ,"Hours"
                ,"Activity"
                ,"Contractor"
                ,"Station"
            });
            rows.Add(header);
            for (int i = 0; i < 3; i++)
            {
                object[] row = new object[header.Count];
                row[0] = new DateTime(2019,7,1);
                row[1] = 18281;
                row[2] = "Biil Bob";
                row[3] = new DateTime(2019,7,1,6,30,7);
                row[4] = new DateTime(2019, 7, 1, 16, 1, 49);
                row[5] = 0.5;
                row[6] = 3.4;
                row[7] = "stuff";
                row[8] = "Hard Grunt";
                row[9] = "BillsPC";
                rows.Add(row);
            }
            stuffing.Add("rows", rows);
            string path = "hello_filler_excellist.xlsx";
            FileStream output = new FileStream(path, FileMode.Create);
            var ok = await filler.FillAsync(null, stuffing, output);
            output.Flush();
            output.Close();
            Assert.True(ok);
        }
        [Fact]
        public async Task Service_works_with_embedded_repo()
        {
            ExcelListReportType excelListReportType = new ExcelListReportType();
            var r1 = new ReportInfo("Hello World", excelListReportType.Name, PassThroughEngine.ScriptingLanguage);
            r1.TemplateFileName = null;
            var reports = new List<ReportInfo>();
            reports.Add(r1);
            ReportingService reportingService = await ReportingService.CreateAssemblyEmbedded(
                new IReportType[] { excelListReportType }
                , Assembly.GetExecutingAssembly()
                , reports
                );

            Dictionary<string, object> stuffing = new Dictionary<string, object>();
            List<object> rows = new List<object>();
            List<string> header = new List<string>();
            header.AddRange(new string[] {
                "Date"
                ,"Id"
                ,"Name"
                ,"Start"
                ,"Finish"
                ,"Lunch"
                ,"Hours"
                ,"Activity"
                ,"Contractor"
                ,"Station"
            });
            rows.Add(header);
            for (int i = 0; i < 3; i++)
            {
                object[] row = new object[header.Count];
                row[0] = new DateTime(2019, 7, 1);
                row[1] = 18281;
                row[2] = "Biil Bob";
                row[3] = new DateTime(2019, 7, 1, 6, 30, 7);
                row[4] = new DateTime(2019, 7, 1, 16, 1, 49);
                row[5] = 0.5;
                row[6] = 3.4;
                row[7] = "stuff";
                row[8] = "Hard Grunt";
                row[9] = "BillsPC";
                rows.Add(row);
            }
            stuffing.Add("rows", rows);
            string path = "hello_service_excellist.xlsx";
            FileStream output = new FileStream(path, FileMode.Create);
            MemoryStream stdout = new MemoryStream();
            string message = null;
            try
            {
                var response = await reportingService.GenerateReportAsync("Hello World", stuffing, output, stdout);
                stdout.Position = 0;
                StreamReader r = new StreamReader(stdout);
                message = r.ReadToEnd();
            }
            catch (Exception ex)
            {
                Assert.True(false);
                return;
            }
            output.Close();
            Assert.True(File.Exists(path));
            var lines = File.ReadLines(path);
            Assert.True(lines.Count() > 0);
            var line = File.ReadLines(path).First();
            //Assert.StartsWith("<?xml version=", line);
        }
    }
}
