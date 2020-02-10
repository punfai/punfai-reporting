using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Dynamic;
using System.IO;
using System.Reflection;
using Punfai.Report;
using Punfai.Report.Ibex.Netcore;

namespace PunReportNetcore31Test
{
    public class ServiceTests
    {
        private const string ibexRuntimeKey = "BC5E5D350DBD58E469843A5587AEC0D60AU5BQUV0AXYIOFEZBRG1G==";
        [Fact]
        public async Task Service_works_with_embedded_repo()
        {
            IbexFoReportType foReportType = new IbexFoReportType(ibexRuntimeKey);
            var r1 = new ReportInfo("Hello World", foReportType.Name, PassThroughEngine.ScriptingLanguage);
            r1.TemplateFileName = "PunReportNetcore31Test.hello_world.fo";
            var reports = new List<ReportInfo>();
            reports.Add(r1);

            ReportingService reportingService = await ReportingService.CreateAssemblyEmbedded(
                new IReportType[] { foReportType }
                , Assembly.GetExecutingAssembly()
                , reports
                );

            List<Hobo> hobos = new List<Hobo>();
            hobos.Add(new Hobo("Joey", "SlowMo", 17.55f));
            hobos.Add(new Hobo("Harry M.", "Hottogo", 32f));
            hobos.Add(new Hobo("P. Jr", "Moggato", 24.858358f));
            List<object> rows = new List<object>(
            hobos.Select(a =>
            {
                dynamic row = new ExpandoObject();
                row.FirstName = a.FirstName;
                row.Surname = a.Surname;
                row.Age = a.Age;
                row.DateOfBirth = a.DateOfBirth;
                return row;
            }));

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["rows"] = rows;

            string path = "hello_service.pdf";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"Reports", "PunReportTest_output.txt");
            FileStream output = new FileStream(path, FileMode.Create);
            var pdffile = await reportingService.GenerateReportAsync("Hello World", data, output);
            Assert.True(File.Exists(path));
        }
        [Fact]
        public async Task Missing_template_gives_good_error()
        {
            IbexFoReportType foReportType = new IbexFoReportType(ibexRuntimeKey);
            var r1 = new ReportInfo("Hello World", foReportType.Name, PassThroughEngine.ScriptingLanguage);
            r1.TemplateFileName = "Cant_Find_Me.hello_world.fo";
            var reports = new List<ReportInfo>();
            reports.Add(r1);

            ReportingService reportingService = await ReportingService.CreateAssemblyEmbedded(
                new IReportType[] { foReportType }
                , Assembly.GetExecutingAssembly()
                , reports
                );

            List<Hobo> hobos = new List<Hobo>();
            hobos.Add(new Hobo("Joey", "SlowMo", 17.55f));
            hobos.Add(new Hobo("Harry M.", "Hottogo", 32f));
            hobos.Add(new Hobo("P. Jr", "Moggato", 24.858358f));
            List<object> rows = new List<object>(
            hobos.Select(a =>
            {
                dynamic row = new ExpandoObject();
                row.FirstName = a.FirstName;
                row.Surname = a.Surname;
                row.Age = a.Age;
                row.DateOfBirth = a.DateOfBirth;
                return row;
            }));

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["rows"] = rows;

            string path = "Missing_hellooo_service.pdf";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"Reports", "PunReportTest_output.txt");
            FileStream output = new FileStream(path, FileMode.Create);
            try
            {
                var pdffile = await reportingService.GenerateReportAsync("Hello World", data, output);
            }
            catch (Exception ex)
            {
                Assert.True(true);
            }
            finally { output.Close(); }
        }
        [Fact]
        public async Task Bad_template_gives_good_error()
        {
            IbexFoReportType foReportType = new IbexFoReportType(ibexRuntimeKey);
            var r1 = new ReportInfo("Hello World", foReportType.Name, PassThroughEngine.ScriptingLanguage);
            r1.TemplateFileName = "IbexReportTests.bad.hello_world.fo";
            var reports = new List<ReportInfo>();
            reports.Add(r1);

            ReportingService reportingService = await ReportingService.CreateAssemblyEmbedded(
                new IReportType[] { foReportType }
                , Assembly.GetExecutingAssembly()
                , reports
                );

            List<Hobo> hobos = new List<Hobo>();
            hobos.Add(new Hobo("Joey", "SlowMo", 17.55f));
            hobos.Add(new Hobo("Harry M.", "Hottogo", 32f));
            hobos.Add(new Hobo("P. Jr", "Moggato", 24.858358f));
            List<object> rows = new List<object>(
            hobos.Select(a =>
            {
                dynamic row = new ExpandoObject();
                row.FirstName = a.FirstName;
                row.Surname = a.Surname;
                row.Age = a.Age;
                row.DateOfBirth = a.DateOfBirth;
                return row;
            }));

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["rows"] = rows;

            string path = "Bad_hello_service.pdf";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"Reports", "PunReportTest_output.txt");
            FileStream output = new FileStream(path, FileMode.Create);
            MemoryStream stdout = new MemoryStream();
            string message = null;
            try
            {
                var pdffile = await reportingService.GenerateReportAsync("Hello World", data, output, stdout);
                stdout.Position = 0;
                StreamReader r = new StreamReader(stdout);
                message = r.ReadToEnd();
            }
            catch (Exception ex)
            {
                Assert.True(true);
                return;
            }
            Assert.True(message != null && message.Length > 0);
        }
    }
}
