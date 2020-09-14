namespace Punfai.Report
{
    public interface IReportType
    {
        string Name { get; }
        string DocumentType { get; }
        IReportFiller Filler { get; }
        ITemplate CreateTemplate(byte[] templateBytes);
        /// <summary>
        /// Loads the default template into the byte array
        /// </summary>
        /// <returns>True if there is a default template, false if not</returns>
        bool GetDefaultTemplate(out byte[] template);
    }
}
