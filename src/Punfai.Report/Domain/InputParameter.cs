using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Punfai.Report
{
    /// <summary>
    /// Parameter used by a report
    /// </summary>
    public class InputParameter
    {
        public string Name { get; set; }
        public string TestValue { get; set; }
        public string DataType { get; set; }
        public object Value { get; set; }
    }
}
