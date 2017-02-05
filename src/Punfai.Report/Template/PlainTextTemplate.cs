using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Punfai.Report.Interfaces;

namespace Punfai.Report.Template
{
    /// <summary>
    /// The default ITemplate for a plain text document (utf8?) with a single section (Body)
    /// </summary>
    public class PlainTextTemplate : ITemplate
    {
        private string bodySection;
        public PlainTextTemplate(byte[] templateBytes)
        {
            if (templateBytes == null) bodySection = "";
            else bodySection = System.Text.UTF8Encoding.UTF8.GetString(templateBytes, 0, templateBytes.Length);
            SectionNames = new[] { "Body" };
            IsChanged = false;
        }
        public IEnumerable<string> SectionNames { get; private set; }
        public byte[] GetTemplateBytes()
        {
            byte[] buf = System.Text.Encoding.UTF8.GetBytes(bodySection);
            return buf;
        }
        public string GetSectionText(string sectionName)
        {
            if (sectionName != "Body") throw new Exception("The only section we've got is called 'Body' not '" + sectionName + "'");
            if (bodySection == null) throw new Exception("No template has been loaded. Call LoadTemplate first.");
            return bodySection;
        }
        public bool SetSectionText(string sectionName, string text)
        {
            if (sectionName != "Body") throw new Exception("The only section we've got is called 'Body' not '" + sectionName + "'");
            if (text != bodySection) IsChanged = true;
            bodySection = text;
            return true;
        }

        public void AcceptChanges()
        {
            IsChanged = false;
        }

        public bool IsChanged
        {
            get;
            private set;
        }
    }
}
