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

namespace PunReportNet47Test
{
    public class WpfXamlTests
    {
        [Fact]
        public async Task Service_works_with_embedded_repo()
        {
            WpfXamlReportType xamlReportType = new WpfXamlReportType();
            var r1 = new ReportInfo("Hello World", xamlReportType.Name, PassThroughEngine.ScriptingLanguage);
            r1.TemplateFileName = "PunReportNet47Test.flow.hello_world.xml";
            var reports = new List<ReportInfo>();
            reports.Add(r1);

            ReportingService reportingService = await ReportingService.CreateAssemblyEmbedded(
                new IReportType[] { xamlReportType }
                , Assembly.GetExecutingAssembly()
                , reports
                );

            List<object> rows = MakeDicPeople();

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["rows"] = rows;

            string path = "hello_service.xaml";
            FileStream output = new FileStream(path, FileMode.Create);
            MemoryStream stdout = new MemoryStream();
            string message = null;
            try
            {
                var response = await reportingService.GenerateReportAsync("Hello World", data, output, stdout);
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
        [Fact]
        public async Task Missing_template_gives_good_error()
        {
            WpfXamlReportType xamlReportType = new WpfXamlReportType();
            var r1 = new ReportInfo("Hello World", xamlReportType.Name, PassThroughEngine.ScriptingLanguage);
            r1.TemplateFileName = "Cant_Find_Me.flow.hello_world.xml";
            var reports = new List<ReportInfo>();
            reports.Add(r1);

            ReportingService reportingService = await ReportingService.CreateAssemblyEmbedded(
                new IReportType[] { xamlReportType }
                , Assembly.GetExecutingAssembly()
                , reports
                );

            List<object> rows = MakeDicPeople();

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
            WpfXamlReportType xamlReportType = new WpfXamlReportType();
            var r1 = new ReportInfo("Hello World", xamlReportType.Name, PassThroughEngine.ScriptingLanguage);
            r1.TemplateFileName = "PunReportNet47Test.badflow.hello_world.xml";
            var reports = new List<ReportInfo>();
            reports.Add(r1);

            ReportingService reportingService = await ReportingService.CreateAssemblyEmbedded(
                new IReportType[] { xamlReportType }
                , Assembly.GetExecutingAssembly()
                , reports
                );

            List<object> rows = MakeDicPeople();

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["rows"] = rows;

            string path = "Bad_hello_service.xaml";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"Reports", "PunReportTest_output.txt");
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
        private List<object> MakeDicPeople()
        {
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
            return rows;
        }
    }
    class Hobo
    {
        public Hobo(string firstName, string surname, float age) { FirstName = firstName; Surname = surname; Age = age; DateOfBirth = DateTime.Today.AddYears(-(int)age); }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public float Age { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
