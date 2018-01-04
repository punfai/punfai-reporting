using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Packaging;
using System.Windows.Xps;
using System.Printing;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using System.Xml.Xsl;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Xps.Packaging;
using System.Windows.Controls;
using System.Windows.Media;
using Punfai.Report;
using Punfai.Report.Wpf.Utils;
using Punfai.Report.Interfaces;

namespace Punfai.Report.Wpf.Services
{
    //public class DocxPrinter : IDocumentPrinter
    //{
    //    private PrintService printService;

    //    public DocxPrinter(PrintService pserv)
    //    {
    //        printService = pserv;
    //    }

    //    public string Name { get { return "DocxPrinter"; } }
    //    public string DocumentType { get { return "docx"; } }
    //    public object Document { get; set; }
    //    public object Printer { get; set; }
    //    public object Settings { get; set; }


    //    public void Print()
    //    {
    //        List<string> files;
    //        if (Document is List<string>) files = (List<string>)Document;
    //        else
    //        {
    //            files = new List<string>();
    //            files.Add(Document.ToString());
    //        }
    //        if (Settings is PrintDialog)
    //        {
    //            PrintDialog pDialog = (PrintDialog)Settings;
    //            OfficeHelper.ConvertWordToXPS(files);
    //            foreach (string file in files)
    //            {
    //                string xpsfile = file + ".xps";
    //                XpsDocument document = new XpsDocument(xpsfile, FileAccess.Read);
    //                FixedDocumentSequence seq = document.GetFixedDocumentSequence();
    //                FileInfo fileinfo = new FileInfo(file);
    //                pDialog.PrintDocument(seq.DocumentPaginator, fileinfo.Name);
    //                document.Close();
    //            }
    //        }
    //        else
    //        {
    //            // one file only
    //            if (Printer == null) Printer = printService.GetPrinter();
    //            if (Printer == null)
    //            {
    //                return;
    //            }

    //            string xpsfile = Document.ToString() + ".xps";
    //            OfficeHelper.ConvertWordToXPS(files);

    //            PrintQueue pq = printService.GetPrintQueue(Printer.ToString());
    //            PrintDialog pDialog = new PrintDialog();
    //            pDialog.PageRangeSelection = PageRangeSelection.AllPages;
    //            pDialog.UserPageRangeEnabled = true;
    //            pDialog.PrintQueue = pq;

    //            // Display the dialog. This returns true if the user presses the Print button.
    //            Nullable<Boolean> print = pDialog.ShowDialog();
    //            if (print == true)
    //            {
    //                XpsDocument document = new XpsDocument(xpsfile, FileAccess.Read);
    //                FixedDocumentSequence seq = document.GetFixedDocumentSequence();
    //                FileInfo file = new FileInfo(Document.ToString());
    //                pDialog.PrintDocument(seq.DocumentPaginator, file.Name);
    //                //dialog.ShowTaskDialog("Printing complete", "Printed to " + pDialog.PrintQueue.Name, "File printed: " + Document.ToString());
    //                document.Close();
    //            }
    //        }
    //        /* more crap that doesn't duplex
    //        PrintQueue pq = Common.Services.PrintService.GetPrintQueue(Printer.ToString());
    //        if (pq == null)
    //        {
    //            dialog.ShowTaskDialog("Printer Problem", "Printer not set up or not available!", "Go to top menu -> Configuration -> My Details to set up your printer");
    //            return;
    //        }
    //        System.Printing.PrintTicket ticket = pq.DefaultPrintTicket;
    //        PrintCapabilities capable = pq.GetPrintCapabilities();
    //        if (capable.DuplexingCapability.Contains(Duplexing.TwoSidedLongEdge))
    //        {
    //            News.AddMessage("Printer can do duplex");
    //            ticket.Duplexing = Duplexing.TwoSidedLongEdge;
    //        }
    //        ticket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
    //        if (Settings != null)
    //        {
    //            if (Settings.ToString().Contains("landscape")) { ticket.PageOrientation = PageOrientation.Landscape; }
    //        }

    //        XpsDocumentWriter xpsdw = PrintQueue.CreateXpsDocumentWriter(pq);
    //        xpsdw.Write(seq, ticket);
    //        document.Close();
    //        */

    //        /*
    //         * Word automation crap that doesn't duplex
    //        try
    //        {
    //            Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
    //            app.Visible = false;
    //            Microsoft.Office.Interop.Word.Document doc = app.Documents.Open(Document);

    //            // print using word
    //            object wb = app.WordBasic;
    //            app.ActivePrinter = Printer.ToString();

    //            if (Settings != null)
    //            {
    //                if (Settings.ToString().Contains("landscape")) { doc.PageSetup.Orientation = WdOrientation.wdOrientLandscape; }
    //            }

    //            app.PrintOut();
    //            app.NormalTemplate.Saved = true;
    //            app.Documents.Close(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges);
    //            app.Quit();
    //        }
    //        catch (Exception ex)
    //        {
    //            News.AddError("DocxPrinter", ex.Message, ex);
    //            return;
    //        }
    //         */
    //    }
    //}

    //public class PowerPointPrinter : IDocumentPrinter
    //{
    //    private PrintService printService;

    //    public PowerPointPrinter(PrintService pserv)
    //    {
    //        printService = pserv;
    //    }

    //    public string Name { get { return "PowerPointPrinter"; } }
    //    public string DocumentType { get { return "pptx"; } }
    //    public object Document { get; set; }
    //    public object Printer { get; set; }
    //    public object Settings { get; set; }

    //    public void Print()
    //    {
    //        if (Settings is PrintDialog)
    //        {
    //            PrintDialog pDialog = (PrintDialog)Settings;
    //            string xpsfile = Document.ToString() + ".xps";
    //            OfficeHelper.ConvertPPTXtoXPS(Document.ToString(), xpsfile);
    //            XpsDocument document = new XpsDocument(xpsfile, FileAccess.Read);
    //            FixedDocumentSequence seq = document.GetFixedDocumentSequence();
    //            FileInfo file = new FileInfo(Document.ToString());
    //            pDialog.PrintDocument(seq.DocumentPaginator, file.Name);
    //            document.Close();
    //        }
    //        else
    //        {

    //            if (Printer == null) Printer = printService.GetPrinter();
    //            if (Printer == null)
    //            {
    //                //dialog.ShowTaskDialog("Printer Problem", "Printer not set up or not available!", "Go to top menu -> Configuration -> My Details to set up your printer");
    //                return;
    //            }

    //            string xpsfile = Document.ToString() + ".xps";
    //            OfficeHelper.ConvertPPTXtoXPS(Document.ToString(), xpsfile);

    //            PrintQueue pq = printService.GetPrintQueue(Printer.ToString());
    //            PrintDialog pDialog = new PrintDialog();
    //            pDialog.PageRangeSelection = PageRangeSelection.AllPages;
    //            pDialog.UserPageRangeEnabled = true;
    //            pDialog.PrintQueue = pq;

    //            // Display the dialog. This returns true if the user presses the Print button.
    //            Nullable<Boolean> print = pDialog.ShowDialog();
    //            if (print == true)
    //            {
    //                XpsDocument document = new XpsDocument(xpsfile, FileAccess.Read);
    //                FixedDocumentSequence seq = document.GetFixedDocumentSequence();
    //                FileInfo file = new FileInfo(Document.ToString());
    //                pDialog.PrintDocument(seq.DocumentPaginator, file.Name);
    //                document.Close();
    //            }
    //        }

    //        /*
    //        try
    //        {
    //            Microsoft.Office.Interop.PowerPoint.Application app;
    //            try
    //            {
    //                app = new Microsoft.Office.Interop.PowerPoint.Application();
    //            }
    //            catch (Exception ex)
    //            {
    //                News.AddError("PrintPPTX", "Problem with PowerPoint", ex);
    //                return;
    //            }
    //            app.Visible = Microsoft.Office.Core.MsoTriState.msoTrue;
    //            Microsoft.Office.Interop.PowerPoint.Presentation p = app.Presentations.Open(Document.ToString());
    //            p.PrintOptions.ActivePrinter = Printer.ToString();

    //            if (Settings != null)
    //            {
    //                if (Settings.ToString().Contains("landscape")) { p.PageSetup.Orientation = WdOrientation.wdOrientLandscape; }
    //            }
                
    //            p.PrintOut();
    //            p.Close();
    //            app.Quit();
    //        }
    //        catch (Exception ex)
    //        {
    //            News.AddError("DocxPrinter", ex.Message, ex);
    //            return;
    //        }
    //        dialog.ShowTaskDialog("Printing complete", "Printed to " + Printer.ToString(), "File printed: " + Document.ToString());
    //         */
    //    }
    //}

    public class FlowDocumentPrinter : IDocumentPrinter
    {
        private PrintService printService;

        public FlowDocumentPrinter(PrintService pserv)
        {
            printService = pserv;
        }
        public string Name { get { return "FlowDocumentPrinter"; } }
        public string DocumentType { get { return "xaml"; } }
        public object Document { get; set; }
        public object Printer { get; set; }
        public object Settings { get; set; }
        public int PageCount { get; private set; }
        public PimpedPaginator.DrawHeaderFooter Header { get; set; }
        public PimpedPaginator.DrawHeaderFooter Footer { get; set; }

        public void Print()
        {
            if (!(Document is FlowDocument))
                throw new Exception("Document is not a FlowDocument");
            FlowDocument flow = (FlowDocument)Document;
            PrintDialog pDialog = new PrintDialog();
            pDialog.PageRangeSelection = PageRangeSelection.AllPages;
            pDialog.UserPageRangeEnabled = true;
            PrintQueue pq = printService.GetPrintQueue();
            if (pq != null)
            {
                // forget if this does anything useful, other than select the chosen printer. Duplex doesn't work.
                PrintTicket ticket = pq.DefaultPrintTicket;
                PrintCapabilities capable = pq.GetPrintCapabilities();
                if (capable.DuplexingCapability.Contains(Duplexing.TwoSidedLongEdge))
                    ticket.Duplexing = Duplexing.TwoSidedLongEdge;
                ticket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
                if (Settings != null)
                {
                    if (Settings.ToString().Contains("landscape")) { ticket.PageOrientation = PageOrientation.Landscape; }
                }
                if (Settings is PimpedPaginator.Definition)
                {
                    if (((PimpedPaginator.Definition)Settings).Landscape) ticket.PageOrientation = PageOrientation.Landscape;
                }

                pDialog.PrintQueue = pq;
                pDialog.PrintTicket = ticket;
            }
            // Display the dialog. This returns true if the user presses the Print button.
            Nullable<Boolean> print = pDialog.ShowDialog();
            if (print == true)
            {
                //DocumentPaginator paginator = ((IDocumentPaginatorSource)flow).DocumentPaginator;
                //paginator.PageSize = new Size(1122, 794);
                //paginator.PageSize = new Size(794, 1122);

                PimpedPaginator.Definition def = new PimpedPaginator.Definition();
                if (Settings is PimpedPaginator.Definition)
                    def = (PimpedPaginator.Definition)Settings;
                if (pDialog.PrintTicket.PageOrientation == PageOrientation.Landscape) def.PageSize = new Size(pDialog.PrintTicket.PageMediaSize.Height.Value, pDialog.PrintTicket.PageMediaSize.Width.Value);
                else def.PageSize = new Size(pDialog.PrintTicket.PageMediaSize.Width.Value, pDialog.PrintTicket.PageMediaSize.Height.Value);

                PimpedPaginator paginator = new PimpedPaginator(flow, def);

                pDialog.PrintDocument(paginator, flow.Name);

            }
            /*
            System.Windows.Xps.XpsDocumentWriter docWriter = System.Printing.PrintQueue.CreateXpsDocumentWriter(pq);
            System.Printing.PrintTicket ticket = pq.DefaultPrintTicket;
            PrintCapabilities capable = pq.GetPrintCapabilities();
            if (capable.DuplexingCapability.Contains(Duplexing.TwoSidedLongEdge))
                ticket.Duplexing = Duplexing.TwoSidedLongEdge;
            ticket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
            if (Settings != null)
            {
                if (Settings.ToString().Contains("landscape")) { ticket.PageOrientation = PageOrientation.Landscape; }
            }

            if (docWriter != null)
            {
                DocumentPaginator paginator = ((IDocumentPaginatorSource)flow).DocumentPaginator;
                // Change the PageSize and PagePadding for the document to match the CanvasSize for the printer device.
                //paginator.PageSize = new Size(1122, 794);
                //paginator.PageSize = new Size(794, 1122);
                Thickness t = new Thickness(32, 30, 10, 10);  // copy.PagePadding;
                flow.PagePadding = new Thickness(t.Left,t.Top,t.Right,t.Bottom);
                flow.ColumnWidth = double.PositiveInfinity;
                //copy.PageWidth = 528; // allow the page to be the natural with of the output device

                // Send content to the printer.
                docWriter.Write(paginator, ticket);
                dialog.ShowTaskDialog("Printing complete", "Printed to " + pq.Name, "");
            }
            */
        }
    }

}
