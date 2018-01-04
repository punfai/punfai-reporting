using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Punfai.Report.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using DocumentFormat.OpenXml.Wordprocessing;
using Punfai.Report.OfficeOpenXml.Fillers;
using System.Xml.Linq;
using Punfai.Report.OfficeOpenXml.Utils;

namespace Punfai.Report.Ibex
{
    /// <summary>
    /// The default ITemplate for a plain text document (utf8?) with a single section (Body)
    /// </summary>
    public class IbexPdfTemplate : ITemplate
    {
        private List<string> sectionNames;
        private byte[] templateBytes;
        private Dictionary<string, string> sectionText;
        private bool loaded = false;
        private bool bytesInSync = false;
        private const string headerSectionName = "Header";
        private const string mainDocumentSectionName = "MainDocument";
        private const string footerSectionName = "Footer";
        public IbexPdfTemplate(byte[] templateBytes)
        {
            if (templateBytes == null) throw new ArgumentException("templateBytes");
            this.templateBytes = templateBytes;
            bytesInSync = true;
            sectionText = new Dictionary<string, string>();
            sectionNames = new List<string>(new[] { "Header", "MainDocument", "Footer" });
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
        #endregion

        #region private section methods

        private void setSectionBytes(string sectionName)
        {
            using (Stream docstream = new MemoryStream())
            {
                docstream.Write(templateBytes, 0, templateBytes.Length);
                docstream.Position = 0;
                using (WordprocessingDocument doc = WordprocessingDocument.Open(docstream, true))
                {
                    //if (sectionName == mainDocumentSectionName)
                    //    setPartXml(sectionText[sectionName], doc.MainDocumentPart);
                    bool found = false;

                    var p1 = doc.Parts.ToList().FirstOrDefault(p => p.OpenXmlPart != null && p.OpenXmlPart.Uri.ToString() == sectionName);
                    if (p1 != null)
                    {
                        setPartXml(sectionText[sectionName], p1.OpenXmlPart);
                        found = true;
                    }
                    else
                    {
                        var p2 = doc.MainDocumentPart.Parts.ToList().FirstOrDefault(p => p.OpenXmlPart != null && p.OpenXmlPart.Uri.ToString() == sectionName);
                        if (p2 != null)
                        {
                            setPartXml(sectionText[sectionName], p2.OpenXmlPart);
                            found = true;
                        }
                    }
                    if (!found) throw new ArgumentOutOfRangeException(sectionName);
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
            //using (Stream docstream = new MemoryStream())
            //{
            //    docstream.Write(templateBytes, 0, templateBytes.Length);
            //    docstream.Position = 0;
            //    using (SpreadsheetDocument doc = SpreadsheetDocument.Open(docstream, true))
            //    {
            //        if (sectionName == sharedStringsSectionName)
            //            doc.WorkbookPart.DeletePart(doc.WorkbookPart.SharedStringTablePart);
            //        else if (sectionName == workbookStyleSectionName)
            //            doc.WorkbookPart.DeletePart(doc.WorkbookPart.WorkbookStylesPart);
            //        else if (sectionName == calculationSectionName)
            //            doc.WorkbookPart.DeletePart(doc.WorkbookPart.CalculationChainPart);
            //        //else if (sectionName == workbookSectionName)
            //        else
            //        {
            //            var sheetId = doc.WorkbookPart.Workbook.Descendants<Sheet>().Where(sheet => sheet.Name == sectionName).Select(s => s.Id).FirstOrDefault();
            //            if (sheetId == null) throw new ArgumentOutOfRangeException(sectionName);
            //            var w = doc.WorkbookPart.GetPartById(sheetId.Value);
            //            if (w == null) throw new ArgumentOutOfRangeException(sectionName);
            //            doc.WorkbookPart.DeletePart(w);
            //        }
            //    }

            //    docstream.Position = 0;
            //    byte[] newbuf = new byte[docstream.Length];
            //    docstream.Read(newbuf, 0, (int)docstream.Length);
            //    templateBytes = newbuf;
            //    bytesInSync = true;
            //}
        }
        #endregion

        #region bytes and parts
        /// <summary>
        /// Scans the template for part and creates a section for each one.
        /// </summary>
        public void LoadSections()
        {
            if (loaded) return;
            sectionNames.Clear();
            using (Stream docstream = new MemoryStream(templateBytes, false))
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Open(docstream, false))
                {
                    //var main = doc.MainDocumentPart;
                    //sectionNames.Add(mainDocumentSectionName);
                    //sectionText[mainDocumentSectionName] = main.GetXDocument().ToString();

                    int partIndex = 0;
                    var parts = doc.Parts;
                    parts.ToList().ForEach(p =>
                    {
                        OpenXmlPart part = p.OpenXmlPart;
                        string partName = part.Uri.ToString();
                        sectionNames.Add(partName);
                        sectionText[partName] = part.GetXDocument().ToString();
                        partIndex++;
                    });

                    foreach (var p in doc.MainDocumentPart.Parts)
                    {
                        OpenXmlPart part = p.OpenXmlPart;
                        if (part == null || part.RootElement == null) continue;
                        //string partName = "part" + partIndex.ToString() + p.OpenXmlPart.RootElement.LocalName;
                        string partName = part.Uri.ToString();
                        sectionNames.Add(partName);
                        sectionText[partName] = part.GetXDocument().ToString();
                        partIndex++;
                    }

                    //foreach (var p in doc.MainDocumentPart.HeaderParts)
                    //{
                    //    if (p == null || p.RootElement == null) continue;
                    //    //string partName = "part" + partIndex.ToString() + p.OpenXmlPart.RootElement.LocalName;
                    //    string partName = p.Uri.AbsolutePath;
                    //    sectionNames.Add(partName);
                    //    sectionText[partName] = p.GetXDocument().ToString();
                    //    partIndex++;
                    //}
                    //foreach (var p in doc.MainDocumentPart.FooterParts)
                    //{
                    //    if (p == null || p.RootElement == null) continue;
                    //    //string partName = "part" + partIndex.ToString() + p.OpenXmlPart.RootElement.LocalName;
                    //    string partName = p.Uri.AbsolutePath;
                    //    sectionNames.Add(partName);
                    //    sectionText[partName] = p.GetXDocument().ToString();
                    //    partIndex++;
                    //}

                }
            }
            loaded = true;
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

        #endregion
    }
}
