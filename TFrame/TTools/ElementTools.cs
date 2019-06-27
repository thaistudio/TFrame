using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace TFrame.TTools
{
    public class ElementTools
    {
        /// <summary>
        /// Get Mark of a member
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetMark(Element e)
        {
            string mark = e.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();
            if (mark != null) return mark;
            else return "";
        }
    }
}
