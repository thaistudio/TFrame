using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame
{
    public static class GlobalParams
    {
        public static double FtToMm = 304.8;
        public static double Tolerence = 0.0001;
        public static bool IsUpdated { get; set; }
        public static bool IsOK { get; set; }
        public static string UISavingPath = @"D:\Thai\Code\Revit\TFrame\Data\";

        //Error handler
        public static List<string> Errors = new List<string>();
        public static string ErrorPath = @"D:\Thai\Code\Revit\TFrame\Data\Errors.log";

        public static string PathSettingBeamMark = @"D:\Thai\Code\Revit\TFrame\Data\Beam Marks.xml";

        public static Document Doccument { get; set; }
        public static ExternalCommandData ExternalCommandData { get; set; }
        public static UIDocument UIDocument { get; set; }

        // Link
        public static string BreakLineDirectory
        {
            get { return @"C:\ProgramData\Autodesk\RVT 2019\Libraries\Canada\Detail Items\Div 01-General\M_Break Line.rfa"; }
        }
    }
}
