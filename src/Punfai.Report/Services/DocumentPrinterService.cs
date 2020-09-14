using Punfai.Report.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Punfai.Report.Services
{
    public class DocumentPrinterService : IDocumentPrinterService
    {
        private readonly List<IDocumentPrinter> documentPrinters;
        public DocumentPrinterService(List<IDocumentPrinter> documentPrinters)
        {
            this.documentPrinters = documentPrinters;
        }

        public IDocumentPrinter GetPrinter(string documentPrinterName)
        {
            var printer = documentPrinters.FirstOrDefault(item => item.Name == documentPrinterName);
            if (printer == null) throw new Exception("No document printer found to print " + documentPrinterName);
            return printer;
        }
    }
}
