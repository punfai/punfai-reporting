namespace Punfai.Report.Interfaces
{
    public interface IDocumentPrinter
    {
        string Name { get; }
        string DocumentType { get; }
        object Document { get; set; }
        object Printer { get; set; }
        object Settings { get; set; }
        void Print();
    }

    public interface IDocumentPrinterService
    {
        IDocumentPrinter GetPrinter(string documentPrinterName);
    }
}
