using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace Punfai.Report.Ibex.Netcore
{
    /// <summary>
    /// You could make this more interesting than a PlainTextTemplate if you wanted
    /// </summary>
    public class XslFoTemplate : ITemplate
    {
        private List<string> sectionNames;
        private string bodySection;
        private bool loaded = false;
        public XslFoTemplate(byte[] templateBytes)
        {
            if (templateBytes == null) bodySection = "";
            else bodySection = System.Text.UTF8Encoding.UTF8.GetString(templateBytes, 0, templateBytes.Length);
            sectionNames = new List<string>(new[] { "Body" });
            IsChanged = false;
        }

        #region properties
        public IEnumerable<string> SectionNames { get { return sectionNames; } }
        public bool IsChanged
        {
            get;
            private set;
        }
        #endregion

        #region public methods
        public void AcceptChanges()
        {
            IsChanged = false;
        }

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
        #endregion
    }
}
