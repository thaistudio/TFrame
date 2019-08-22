using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace TFrame
{
    /// <summary>
    /// This class holds info of the view that will be dimensioned
    /// </summary>
    public class TView
    {
        public TView(View view)
        {
            TDimensionSets = new List<DimensionSet>();
            Name = view.Name;
            RevitView = view;
            HostElement = ElementTools.GetElementByMarkInView(view.LookupParameter("T_Mark").AsString(), view);
            Viewer = new FilteredElementCollector(view.Document).OfCategory(BuiltInCategory.OST_Viewers).Where(x => x.Name == Name).FirstOrDefault();
            view.CropBoxVisible = true; // Crop box has to be visible to get viewer's bounding box
            ViewerBoundingBox = Viewer.get_BoundingBox(view);
        }

        public List<DimensionSet> TDimensionSets { get; set; }
        public Element HostElement { get; set; }
        public View RevitView { get; set; }
        public string Name { get; set; }
        public BoundingBoxXYZ ViewerBoundingBox { get; set; }
        public Element Viewer { get; set; }

        void AddTDimensionSet(DimensionSet tDimSet)
        {
            if (TDimensionSets.Any(x => x.Name == tDimSet.Name)) return;
            TDimensionSets.Add(tDimSet);
        }

        void CreateTDimensionSet(Autodesk.Revit.DB.Reference reference)
        {
        }
    }
}
