using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmdDimension : TCommand
    {
        public List<Element> selBeams = new List<Element>();

        public cmdDimension() : base("Create Beam Dimensions", false) { }
           
        protected override Result MainMethod()
        {
            var v = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Viewers).Where(x => x.Name == "Section 50");
            // Urge users to select beams
            selBeams = SelectionTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralFraming);
            while (selBeams.Count == 0) selBeams = SelectionTools.UrgeSelection(BuiltInCategory.OST_StructuralFraming);

            // UI
            BeamDimUC form = new BeamDimUC(doc);
            form.ShowDialog();

            // Main actions
            foreach (Element beam in selBeams)
            {
                List<Section> sections = SectionTools.CacheSections(beam, doc, SectionTools.SectionType.CrossSection);
                
                foreach (Section section in sections)
                {
                    section.SetDimensionProperties();
                    DimensionActions.DetailSection(section);                   
                }
            }
            return Result.Succeeded;
        }
    }
}
