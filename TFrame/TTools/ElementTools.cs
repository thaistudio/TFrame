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

namespace TFrame
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

        public static Element GetElementByMarkInView(string mark, View view)
        {
            Document doc = GlobalParams.Doc;
            Element e = new FilteredElementCollector(doc, view.Id).Where(x => x.LookupParameter("Mark") != null && x.LookupParameter("Mark").AsString() == mark).FirstOrDefault();
            return e;
        }

        /// <summary>
        /// Return the reference for dimensioning (a.k.a Symbol geometry reference) of an element
        /// </summary>
        /// <param name="face"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Reference GetFaceReference(Face face, Element e)
        {
            string s0 = face.Reference.ConvertToStableRepresentation(e.Document);
            string beamUniqueId0 = e.UniqueId;
            string rep0 = beamUniqueId0 + ":0:INSTANCE:" + s0;
            Reference instanceRef0 = Reference.ParseFromStableRepresentation(e.Document, s0);
            return instanceRef0;
        }
    }
}
