using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Printing;

namespace Punfai.Report.Wpf.Services
{
    public class PrintService
    {
        PrintQueueCollection printerList;
        //private readonly Persistence persistence;
        public PrintService()
        {
            //persistence = squid.Persistence;
        }

        public string GetPrinter()
        {
            if (GetPrinterList() == null) return null;

            string printerName = null;
            IEnumerator<PrintQueue> e = printerList.GetEnumerator();
            if (e.MoveNext())
                printerName = e.Current.FullName;
            return printerName;
        }
        public PrintQueue GetPrintQueue()
        {
            if (GetPrinterList() == null) return null;

            PrintQueue pq = null;
            IEnumerator<PrintQueue> e = printerList.GetEnumerator();
            if (e.MoveNext())
            {
                pq = e.Current;
            }
            return pq;
        }
        public PrintQueue GetPrintQueue(string printer)
        {
            if (GetPrinterList() == null) return null;

            PrintQueue pq = null;
            IEnumerator<PrintQueue> e = printerList.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.FullName == printer) pq = e.Current;
            }
            return pq;
        }
        public PrintQueueCollection GetPrinterList(string printServer = "local")
        {
            if (printServer == "" || printServer == "local")
            {
                LocalPrintServer ps = new LocalPrintServer();
                printerList = ps.GetPrintQueues();
            }
            else
            {
                try
                {
                    PrintServer ps = new PrintServer(printServer);
                    printerList = ps.GetPrintQueues();
                }
                catch (Exception ex)
                {
                }
            }
            return printerList;
        }
    }
}
