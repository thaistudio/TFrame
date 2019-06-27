using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFrame
{
    public static class GlobalParams
    {
        public static double FtToMm = 304.8;
        public static double Tolerence = 0.0001;
        public static bool IsUpdated { get; set; }
        public static bool IsOK { get; set; }

        //Error handler
        public static List<string> Errors = new List<string>();
        public static string ErrorPath = @"D:\Thai\Code\Revit\TFrame\Data\Errors.log";

        public static string PathSettingBeamMark = @"D:\Thai\Code\Revit\TFrame\Data\Beam Marks.xml";
    }
}
