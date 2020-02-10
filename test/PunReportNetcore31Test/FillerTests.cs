using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Dynamic;
using System.IO;
using System.Reflection;
using Punfai.Report.Ibex;

namespace PunReportNetcore31Test
{
    public class FillerTests
    {
        private const string ibexRuntimeKey = "BC5E5D350DBD58E469843A5587AEC0D60AU5BQUV0AXYIOFEZBRG1G==";
        [Fact]
        public async Task Can_get_some_default_fo()
        {
            IbexFoReportType report = new IbexFoReportType(ibexRuntimeKey);
            byte[] templateBytes;
            report.GetDefaultTemplate(out templateBytes);

            string path = "PunReportTest_ibex_out.fo";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"Reports", "PunReportTest_output.txt");
            FileStream output = new FileStream(path, FileMode.Create);
            BinaryWriter w = new BinaryWriter(output);
            w.Write(templateBytes);
            w.Flush();
            w.Close();
            var line = File.ReadLines(path).First();
            Assert.StartsWith("<?xml version=", line);
        }
        [Fact]
        public async Task Can_get_some_test_fo()
        {
            IbexFoReportType report = new IbexFoReportType(ibexRuntimeKey);
            byte[] template;
            Assembly _assembly;
            try
            {
                _assembly = Assembly.GetExecutingAssembly();
                var reader = new BinaryReader(_assembly.GetManifestResourceStream("PunReportNetcore31Test.hello_world.fo"), UTF8Encoding.UTF8);
                template = reader.ReadBytes((int)reader.BaseStream.Length);
            }
            catch
            {
                //logger.Error("Error accessing resources!");
                template = UTF8Encoding.UTF8.GetBytes("template not found");
            }
            //var t = report.CreateTemplate(template);

            string path = "PunReportTest_ibex_hello_world_out.fo";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"Reports", "PunReportTest_output.txt");
            FileStream output = new FileStream(path, FileMode.Create);
            BinaryWriter w = new BinaryWriter(output);
            w.Write(template);
            w.Flush();
            w.Close();
            var line = File.ReadLines(path).First();
            Assert.StartsWith("<?xml version", line);
        }
        [Fact]
        public async Task Can_make_a_hello_world_pdf()
        {
            IbexFoReportType report = new IbexFoReportType(ibexRuntimeKey);
            byte[] template;
            Assembly _assembly;
            try
            {
                _assembly = Assembly.GetExecutingAssembly();
                var reader = new BinaryReader(_assembly.GetManifestResourceStream("PunReportNetcore31Test.hello_world.fo"), UTF8Encoding.UTF8);
                template = reader.ReadBytes((int)reader.BaseStream.Length);
            }
            catch
            {
                //logger.Error("Error accessing resources!");
                template = UTF8Encoding.UTF8.GetBytes("template not found");
            }
            var t = report.CreateTemplate(template);
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

            string path = "PunReportTest_hello_world.pdf";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"Reports", "PunReportTest_output.txt");
            FileStream output = new FileStream(path, FileMode.Create);
            var filled = await report.Filler.FillAsync(t, data, output);
            Assert.True(filled);
        }
        [Fact]
        public async Task Can_use_a_fo_to_make_a_pdf()
        {
            IbexFoReportType report = new IbexFoReportType(ibexRuntimeKey);
            byte[] templateBytes;
            report.GetDefaultTemplate(out templateBytes);
            var t = report.CreateTemplate(templateBytes);

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
            //data["newLine"] = "\r\n";
            //data["bom"] = false;

            string path = "PunReportTest_ibex_out.pdf";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"Reports", "PunReportTest_output.txt");
            FileStream output = new FileStream(path, FileMode.Create);
            var filled = await report.Filler.FillAsync(t, data, output);
            //output.Flush();
            //output.Dispose();
            Assert.True(filled);
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
