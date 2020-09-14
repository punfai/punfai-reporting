using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Punfai.Report.Template
{
    /// <summary>
    /// Not actually used yet
    /// </summary>
    public class XmlTemplate : ITemplate
    {
        private string bodySection;
        public XmlTemplate(byte[] templateBytes)
        {
            if (templateBytes == null) bodySection = "";
            else
            {
                MemoryStream xmlStream = new MemoryStream(templateBytes);
                var doc = XDocument.Load(xmlStream);
                bodySection = doc.Root.ToString();
            }
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
