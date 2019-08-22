using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame
{
    /// <summary>
    /// This class holds info of Revit symbols or types
    /// </summary>
    public class RevitData
    {
        public List<Element> LinearDimensioTypes { get; set; }
        Document _doc;
        
        public RevitData(Document doc)
        {
            _doc = doc;
        }

        public void GenList<T>()
        {
            LinearDimensioTypes = new List<Element>();

            FilteredElementCollector v = new FilteredElementCollector(_doc).OfClass(typeof(T));
            foreach (Element e in v)
            {
                DimensionType dimensionType = (DimensionType)e;
                if (dimensionType.FamilyName == "Linear Dimension Style") LinearDimensioTypes.Add(e);
            }
        }
    }

    public enum RevitDataList
    {

    }
}
