using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Punfai.Report.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using DocumentFormat.OpenXml.Spreadsheet;
using Punfai.Report.OfficeOpenXml.Fillers;
using System.Xml.Linq;
using Punfai.Report.OfficeOpenXml.Utils;

namespace Punfai.Report.OfficeOpenXml.Template
{
    /// <summary>
    /// The default ITemplate for a plain text document (utf8?) with a single section (Body)
    /// </summary>
    public class ExcelTemplate : ITemplate
    {
        private List<string> sectionNames;
        private byte[] templateBytes;
        private Dictionary<string, string> sectionText;
        private bool loaded = false;
        private bool bytesInSync = false;
        private const string sharedStringsSectionName = "SharedStrings";
        private const string workbookStyleSectionName = "WorkbookStyles";
        private const string calculationSectionName = "CalculationChain";
        private const string workbookSectionName = "WorkbookPart";

        public ExcelTemplate(byte[] templateBytes)
        {
            if (templateBytes == null) throw new ArgumentException("templateBytes");
            this.templateBytes = templateBytes;
            bytesInSync = true;
            sectionNames = new List<string>();
            sectionText = new Dictionary<string, string>();
            sectionNames.Add("Worksheet1");
            IsChanged = false;
        }
        public IEnumerable<string> SectionNames { get { return sectionNames; } }
        public byte[] GetTemplateBytes()
        {
            if (bytesInSync) return templateBytes;
            throw new Exception("bytes not in sync");
        }
        public string GetSectionText(string sectionName)
        {
            LoadSections();
            if (!SectionNames.Contains(sectionName)) throw new Exception("No section called '" + sectionName + "'");
            if (string.IsNullOrWhiteSpace(sectionText[sectionName])) return "";
            XDocument xdoc = XDocument.Parse(sectionText[sectionName]);
            if (xdoc.Declaration == null) return xdoc.ToString();
            else return xdoc.Declaration.ToString() + "\r\n" + xdoc.ToString();
        }
        public bool SetSectionText(string sectionName, string text)
        {
            LoadSections();
            if (!SectionNames.Contains(sectionName)) throw new Exception("No section called '" + sectionName + "'");
            if (text != sectionText[sectionName]) IsChanged = true;
            sectionText[sectionName] = text;
            bytesInSync = false;
            if (string.IsNullOrWhiteSpace(text))
                deleteSection(sectionName);
            else
                setSectionBytes(sectionName);
            return true;
        }

        private void setSectionBytes(string sectionName)
        {
            using (Stream docstream = new MemoryStream())
            {
                docstream.Write(templateBytes, 0, templateBytes.Length);
                docstream.Position = 0;
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(docstream, true))
                {
                    if (sectionName == sharedStringsSectionName)
                        setPartXml(sectionText[sectionName], doc.WorkbookPart.SharedStringTablePart);
                    else if (sectionName == workbookStyleSectionName)
                        setPartXml(sectionText[sectionName], doc.WorkbookPart.WorkbookStylesPart);
                    else if (sectionName == calculationSectionName)
                        setPartXml(sectionText[sectionName], doc.WorkbookPart.CalculationChainPart);
                    else if (sectionName == workbookSectionName)
                        setPartXml(sectionText[sectionName], doc.WorkbookPart);
                    else
                    {
                        var sheetId = doc.WorkbookPart.Workbook.Descendants<Sheet>().Where(sheet => sheet.Name == sectionName).Select(s => s.Id).FirstOrDefault();
                        if (sheetId == null) throw new ArgumentOutOfRangeException(sectionName);
                        var w = doc.WorkbookPart.GetPartById(sheetId.Value);
                        if (w == null) throw new ArgumentOutOfRangeException(sectionName);
                        setPartXml(sectionText[sectionName], w);
                    }
                }

                docstream.Position = 0;
                byte[] newbuf = new byte[docstream.Length];
                docstream.Read(newbuf, 0, (int)docstream.Length);
                templateBytes = newbuf;
                bytesInSync = true;
            }
        }
        private void deleteSection(string sectionName)
        {
            using (Stream docstream = new MemoryStream())
            {
                docstream.Write(templateBytes, 0, templateBytes.Length);
                docstream.Position = 0;
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(docstream, true))
                {
                    if (sectionName == sharedStringsSectionName)
                        doc.WorkbookPart.DeletePart(doc.WorkbookPart.SharedStringTablePart);
                    else if (sectionName == workbookStyleSectionName)
                        doc.WorkbookPart.DeletePart(doc.WorkbookPart.WorkbookStylesPart);
                    else if (sectionName == calculationSectionName)
                        doc.WorkbookPart.DeletePart(doc.WorkbookPart.CalculationChainPart);
                    //else if (sectionName == workbookSectionName)
                    else
                    {
                        var sheetId = doc.WorkbookPart.Workbook.Descendants<Sheet>().Where(sheet => sheet.Name == sectionName).Select(s => s.Id).FirstOrDefault();
                        if (sheetId == null) throw new ArgumentOutOfRangeException(sectionName);
                        var w = doc.WorkbookPart.GetPartById(sheetId.Value);
                        if (w == null) throw new ArgumentOutOfRangeException(sectionName);
                        doc.WorkbookPart.DeletePart(w);
                    }
                }

                docstream.Position = 0;
                byte[] newbuf = new byte[docstream.Length];
                docstream.Read(newbuf, 0, (int)docstream.Length);
                templateBytes = newbuf;
                bytesInSync = true;
            }
        }
        private void setPartXml(string sxml, OpenXmlPart part)
        {
            try
            {
                XDocument xdoc = XDocument.Parse(sxml);
                part.PutXDocument(xdoc);
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Trace.TraceError("setPartXml bad part xml {0}", ex);
            }
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
        /// <summary>
        /// Scans the template for worksheets and creates a section for each one.
        /// Updates the SectionNames array to reflect the available worksheets.
        /// </summary>
        public void LoadSections()
        {
            if (loaded) return;
            sectionNames.Clear();
            using (Stream docstream = new MemoryStream(templateBytes, false))
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(docstream, false))
                {
                    var worksheets = doc.WorkbookPart.WorksheetParts;
                    int sheetIndex = 0;
                    worksheets.ToList().ForEach(w =>
                    {
                        Worksheet worksheet = w.Worksheet;
                        string sheetName = doc.WorkbookPart.Workbook.Descendants<Sheet>().ElementAt(sheetIndex).Name;
                        sectionNames.Add(sheetName);
                        sectionText[sheetName] = w.GetXDocument().ToString();
                        sheetIndex++;
                    });
                    if (doc.WorkbookPart.SharedStringTablePart != null)
                    {
                        var sharedPart = doc.WorkbookPart.SharedStringTablePart;
                        sectionNames.Add(sharedStringsSectionName);
                        sectionText[sharedStringsSectionName] = sharedPart.GetXDocument().ToString();
                    }
                    if (doc.WorkbookPart.WorkbookStylesPart != null)
                    {
                        var stylePart = doc.WorkbookPart.WorkbookStylesPart;
                        sectionNames.Add(workbookStyleSectionName);
                        sectionText[workbookStyleSectionName] = stylePart.GetXDocument().ToString();
                    }
                    if (doc.WorkbookPart.CalculationChainPart != null)
                    {
                        var calcPart = doc.WorkbookPart.CalculationChainPart;
                        sectionNames.Add(calculationSectionName);
                        sectionText[calculationSectionName] = calcPart.GetXDocument().ToString();
                    }
                    var wbPart = doc.WorkbookPart;
                    sectionNames.Add(workbookSectionName);
                    sectionText[workbookSectionName] = wbPart.GetXDocument().ToString();
                    int partIndex = 0;
                    foreach (var p in doc.WorkbookPart.Parts)
                    {
                        if (p.OpenXmlPart is WorksheetPart || p.OpenXmlPart.RootElement == null) continue;
                        string partName = "part" + partIndex.ToString() + p.OpenXmlPart.RootElement.LocalName;
                        sectionNames.Add(partName);
                        sectionText[partName] = p.OpenXmlPart.GetXDocument().ToString();
                        partIndex++;
                    }
                    partIndex = 0;
                    foreach (var p in doc.Parts)
                    {
                        if (p.OpenXmlPart.RootElement == null) continue;
                        string partName = "doc" + partIndex.ToString() + p.OpenXmlPart.RootElement.LocalName;
                        sectionNames.Add(partName);
                        sectionText[partName] = p.OpenXmlPart.GetXDocument().ToString();
                        partIndex++;
                    }
                }
            }
            loaded = true;
        }
    }
}
