using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Punfai.Report.Utils
{
    public class StreamTool
    {
        public static string ToUtf8String(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader reader = new StreamReader(stream, UTF8Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
