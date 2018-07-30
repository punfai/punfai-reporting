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

namespace PunReportTests
{
    public class CoreFillerTests
    {
        [Fact]
        public async Task Can_fill_a_plain_text_template_one_block()
        {
            PlainTextTemplate t = new PlainTextTemplate(createOneBlockPlainTextTemplateBytes());
            List<Hobo> hobos = new List<Hobo>();
            hobos.Add(new Hobo("Joey SlowMo", 17.55f));
            hobos.Add(new Hobo("Harry M.Hottogo", 32f));
            hobos.Add(new Hobo("P. Jr Moggato", 24.858358f));
            List<object> rows = new List<object>(
            hobos.Select(a =>
            {
                dynamic row = new ExpandoObject();
                row.FirstName = a.FirstName;
                row.Age = a.Age.ToString("0");
                row.DateOfBirth = a.DateOfBirth;
                return row;
            }));
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["rows"] = rows;
            data["newLine"] = "\r\n";
            data["bom"] = false;
            string path = "PunReportTest_output.txt";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"Reports", "PunReportTest_output.txt");
            FileStream output = new FileStream(path, FileMode.Create);
            TextFiller filler = new TextFiller();
            var filled = await filler.FillAsync(t, data, output);
            output.Flush();
            output.Dispose();
        }
        [Fact]
        public async Task Can_fill_a_plain_text_template_multi_blocks()
        {
            PlainTextTemplate t = new PlainTextTemplate(createMultiBlockPlainTextTemplateBytes());
            List<object> nofillRow = new List<object>(new object[] { new Dictionary<string, object>() });
            List<Nogo> nogos = new List<Nogo>();
            nogos.Add(new Nogo("15 Sunry Paradismal Arches", "QLD"));
            List<object> rowsA = new List<object>(
            nogos.Select(a =>
            {
                dynamic row = new ExpandoObject();
                row.Street = a.Street;
                row.State = a.State;
                return row;
            }));
            List<Hobo> hobos = new List<Hobo>();
            hobos.Add(new Hobo("Joey SlowMo", 17.55f));
            hobos.Add(new Hobo("Harry M.Hottogo", 32f));
            hobos.Add(new Hobo("P. Jr Moggato", 24.858358f));
            List<object> rowsC = new List<object>(
            hobos.Select(a =>
            {
                dynamic row = new ExpandoObject();
                row.FirstName = a.FirstName;
                row.Age = a.Age.ToString("0");
                row.DateOfBirth = a.DateOfBirth;
                return row;
            }));
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["rowsA"] = rowsA;
            data["rowsB"] = nofillRow;
            data["rowsC"] = rowsC;
            data["rowsD"] = nofillRow;
            data["newLine"] = "\r\n";
            data["bom"] = false;
            string path = "PunReportTest_output.txt";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Reports", "PunReportTest_output.txt");
            FileStream output = new FileStream(path, FileMode.Create);
            TextFiller filler = new TextFiller();
            var filled = await filler.FillAsync(t, data, output);
            output.Flush();
            output.Dispose();
        }

        private byte[] createOneBlockPlainTextTemplateBytes()
        {
            string block = "Name: {{FirstName:a,m,20}};Age: {{Age:n,o[0],5}}; DateOfBirth: {{DateOfBirth:dt[dd-MM-yy],o[0],8}};";
            UTF8Encoding utf8NoPreamble = new UTF8Encoding(false);
            return utf8NoPreamble.GetBytes(block);
        }
        private byte[] createMultiBlockPlainTextTemplateBytes()
        {
            StringBuilder blocks = new StringBuilder();
            blocks.AppendLine("Before the hobos we have an important row about nogos: Street: {{Street: an,m}}; State: {{State: a,m,3}};");
            blocks.AppendLine("|Name                |Age  |DoB     |");
            blocks.AppendLine("|{{FirstName:a,m,20}}|{{Age:n,o[0],5}}|{{DateOfBirth:dt[dd-MM-yy],o[0],8}}|");
            blocks.AppendLine("Unadulterated footer {{}} {{oohhh}} and Ahhhhggghh!");
            UTF8Encoding utf8NoPreamble = new UTF8Encoding(false);
            return utf8NoPreamble.GetBytes(blocks.ToString());
        }

    }
    class Hobo
    {
        public Hobo(string firstName, float age) { FirstName = firstName; Age = age; DateOfBirth = DateTime.Today.AddYears(-(int)age); }
        public string FirstName { get; set; }
        public float Age { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
    class Nogo
    {
        public Nogo(string street, string state) { Street = street; State = state; }
        public string Street { get; set; }
        public string State { get; set; }
    }
}
