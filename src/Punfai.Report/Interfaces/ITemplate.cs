using System.Collections.Generic;
using System.ComponentModel;

namespace Punfai.Report
{
    public interface ITemplate : IChangeTracking
    {
        IEnumerable<string> SectionNames { get; }
        byte[] GetTemplateBytes();
        /// <summary>
        /// Gets the section template in text format. If the report type doesn't support text then return whatever you want.
        /// </summary>
        /// <param name="sectionName">The name of the section you want a text template for. If only one section is supported, call it 'Body'.</param>
        /// <returns></returns>
		string GetSectionText(string sectionName);
        /// <summary>
        /// Updates the section template with the given text. If text is not supported for the report type then do nothing.
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="text"></param>
        /// <returns></returns>
		bool SetSectionText(string sectionName, string text);
    }

}
