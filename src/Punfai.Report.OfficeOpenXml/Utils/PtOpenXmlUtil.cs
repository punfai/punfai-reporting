using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Punfai.Report.OfficeOpenXml.Utils
{
    public static class PtOpenXmlUtil
    {
        public static XDocument GetXDocument(this OpenXmlPart part)
        {
            try
            {
                XDocument partXDocument = part.Annotation<XDocument>();
                if (partXDocument != null)
                    return partXDocument;
                using (Stream partStream = part.GetStream())
                {
                    if (partStream.Length == 0)
                    {
                        partXDocument = new XDocument();
                        partXDocument.Declaration = new XDeclaration("1.0", "UTF-8", "yes");
                    }
                    else
                        using (XmlReader partXmlReader = XmlReader.Create(partStream))
                            partXDocument = XDocument.Load(partXmlReader);
                }
                part.AddAnnotation(partXDocument);
                return partXDocument;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void PutXDocument(this OpenXmlPart part)
        {
            XDocument partXDocument = part.GetXDocument();
            if (partXDocument != null)
            {
                using (Stream partStream = part.GetStream(FileMode.Create, FileAccess.Write))
                using (XmlWriter partXmlWriter = XmlWriter.Create(partStream))
                    partXDocument.Save(partXmlWriter);
            }
        }

        public static void PutXDocumentWithFormatting(this OpenXmlPart part)
        {
            XDocument partXDocument = part.GetXDocument();
            if (partXDocument != null)
            {
                using (Stream partStream = part.GetStream(FileMode.Create, FileAccess.Write))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.OmitXmlDeclaration = true;
                    settings.NewLineOnAttributes = true;
                    using (XmlWriter partXmlWriter = XmlWriter.Create(partStream, settings))
                        partXDocument.Save(partXmlWriter);
                }
            }
        }

        public static void PutXDocument(this OpenXmlPart part, XDocument document)
        {
            using (Stream partStream = part.GetStream(FileMode.Create, FileAccess.Write))
            using (XmlWriter partXmlWriter = XmlWriter.Create(partStream))
                document.Save(partXmlWriter);
            part.RemoveAnnotations<XDocument>();
            part.AddAnnotation(document);
        }
    }
}
