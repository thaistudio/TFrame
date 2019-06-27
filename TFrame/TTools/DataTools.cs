using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace TFrame.TTools
{
    public class DataTools
    {
        public static void WriteErrors(string path, List<string> errors)
        {
            File.WriteAllLines(path, errors);
        }
    }
}
